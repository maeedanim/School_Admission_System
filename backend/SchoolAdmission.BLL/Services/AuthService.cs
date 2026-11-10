
using SchoolAdmission.BLL.Interfaces;
using SchoolAdmission.BLL.Models.Auth;

namespace SchoolAdmission.BLL.Services;

public class AuthService : IAuthService
{
    private const string StudentRole = "Student";
    private const string AdminRole = "Admin";
    private const int BirthRegistrationNumberLength = 17;

    private readonly IIdentityRepository _identityRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAccessTokenGenerator _accessTokenGenerator;

    public AuthService(
        IIdentityRepository identityRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IAccessTokenGenerator accessTokenGenerator)
    {
        _identityRepository = identityRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _accessTokenGenerator = accessTokenGenerator;
    }

    public async Task<RegistrationResult> RegisterStudentAsync(
        string fullName,
        string email,
        string password,
        string birthRegistrationNumber,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        fullName = Normalize(fullName);
        email = Normalize(email);
        birthRegistrationNumber = Normalize(birthRegistrationNumber);

        var validationErrors = ValidateRegistration(
            fullName,
            email,
            password,
            birthRegistrationNumber);

        if (validationErrors.Count > 0)
        {
            return CreateRegistrationFailure(
                RegistrationStatus.InvalidInput,
                validationErrors);
        }

        if (await EmailAlreadyExistsAsync(email, cancellationToken))
        {
            return CreateRegistrationFailure(
                RegistrationStatus.DuplicateEmail,
                "An account with this email already exists.");
        }

        if (await BirthRegistrationNumberAlreadyExistsAsync(
                birthRegistrationNumber,
                cancellationToken))
        {
            return CreateRegistrationFailure(
                RegistrationStatus.DuplicateBirthRegistrationNumber,
                "An account with this birth registration number already exists.");
        }

        var creationResult = await _identityRepository.CreateStudentAsync(
            fullName,
            email,
            password,
            birthRegistrationNumber,
            cancellationToken);

        if (creationResult.BirthRegistrationNumberAlreadyExists)
        {
            return CreateRegistrationFailure(
                RegistrationStatus.DuplicateBirthRegistrationNumber,
                "An account with this birth registration number already exists.");
        }

        if (!creationResult.Succeeded)
        {
            return CreateRegistrationFailure(
                RegistrationStatus.Failed,
                creationResult.Errors);
        }

        return new RegistrationResult
        {
            Status = RegistrationStatus.Succeeded,
            UserId = creationResult.UserId
        };
    }

    public async Task<AuthSessionResult> LoginStudentAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        return await LoginAsync(
            email,
            password,
            StudentRole,
            cancellationToken);
    }

    public async Task<AuthSessionResult> LoginAdminAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        return await LoginAsync(
            email,
            password,
            AdminRole,
            cancellationToken);
    }

    private async Task<AuthSessionResult> LoginAsync(
        string email,
        string password,
        string requiredRole,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        email = Normalize(email);

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return CreateSessionFailure(
                AuthSessionStatus.InvalidCredentials);
        }

        var authenticationResult =
            await _identityRepository.AuthenticateAsync(
                email,
                password,
                requiredRole,
                cancellationToken);

        if (authenticationResult.Status !=
                AuthenticationAttemptStatus.Succeeded ||
            authenticationResult.User is null)
        {
            return CreateSessionFailure(
                MapAuthenticationStatus(authenticationResult.Status));
        }

        var session = await CreateSessionAsync(
            authenticationResult.User,
            cancellationToken);

        return new AuthSessionResult
        {
            Status = AuthSessionStatus.Succeeded,
            Session = session
        };
    }

    public async Task<AuthSessionResult> RefreshSessionAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return CreateSessionFailure(
                AuthSessionStatus.InvalidRefreshToken);
        }

        var rotationResult =
            await _refreshTokenRepository.RotateAsync(
                refreshToken,
                cancellationToken);

        if (rotationResult is null)
        {
            return CreateSessionFailure(
                AuthSessionStatus.InvalidRefreshToken);
        }

        var user = await _identityRepository.FindByIdAsync(
            rotationResult.UserId,
            cancellationToken);

        if (!IsValidAccount(user))
        {
            await _refreshTokenRepository.RevokeAsync(
                rotationResult.RefreshToken,
                cancellationToken);

            return CreateSessionFailure(
                AuthSessionStatus.InvalidRefreshToken);
        }

        var (accessToken, accessTokenExpiresAtUtc) =
            await _accessTokenGenerator.GenerateAsync(
                user!,
                cancellationToken);

        return new AuthSessionResult
        {
            Status = AuthSessionStatus.Succeeded,
            Session = new AuthSessionData
            {
                User = user!,
                AccessToken = accessToken,
                AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc,
                RefreshToken = rotationResult.RefreshToken,
                RefreshTokenExpiresAtUtc = rotationResult.ExpiresAtUtc
            }
        };
    }

    public async Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        await _refreshTokenRepository.RevokeAsync(
            refreshToken,
            cancellationToken);
    }

    public async Task<AuthUserData?> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _identityRepository.FindByIdAsync(
            userId,
            cancellationToken);

        return user is { IsActive: true } ? user : null;
    }

    private async Task<AuthSessionData> CreateSessionAsync(
        AuthUserData user,
        CancellationToken cancellationToken)
    {
        var (accessToken, accessTokenExpiresAtUtc) =
            await _accessTokenGenerator.GenerateAsync(
                user,
                cancellationToken);

        var (refreshToken, refreshTokenExpiresAtUtc) =
            await _refreshTokenRepository.CreateAsync(
                user.Id,
                cancellationToken);

        return new AuthSessionData
        {
            User = user,
            AccessToken = accessToken,
            AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc
        };
    }

    private async Task<bool> EmailAlreadyExistsAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var existingUser = await _identityRepository.FindByEmailAsync(
            email,
            cancellationToken);

        return existingUser is not null;
    }

    private Task<bool> BirthRegistrationNumberAlreadyExistsAsync(
        string birthRegistrationNumber,
        CancellationToken cancellationToken)
    {
        return _identityRepository.BirthRegistrationNumberExistsAsync(
            birthRegistrationNumber,
            cancellationToken);
    }

    private static List<string> ValidateRegistration(
        string fullName,
        string email,
        string password,
        string birthRegistrationNumber)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            errors.Add("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password is required.");
        }

        if (!IsValidBirthRegistrationNumber(birthRegistrationNumber))
        {
            errors.Add(
                "Birth registration number must contain exactly 17 digits.");
        }

        return errors;
    }

    private static bool IsValidBirthRegistrationNumber(string value)
    {
        return value.Length == BirthRegistrationNumberLength &&
               value.All(char.IsAsciiDigit);
    }

    private static bool IsValidAccount(AuthUserData? user)
    {
        return user is
        {
            IsActive: true
        } && (
            user.Roles.Contains(StudentRole) ||
            user.Roles.Contains(AdminRole)
        );
    }

    private static string Normalize(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private static RegistrationResult CreateRegistrationFailure(
        RegistrationStatus status,
        string error)
    {
        return CreateRegistrationFailure(status, new[] { error });
    }

    private static RegistrationResult CreateRegistrationFailure(
        RegistrationStatus status,
        IReadOnlyList<string> errors)
    {
        return new RegistrationResult
        {
            Status = status,
            Errors = errors
        };
    }

    private static AuthSessionResult CreateSessionFailure(
        AuthSessionStatus status)
    {
        return new AuthSessionResult
        {
            Status = status
        };
    }

    private static AuthSessionStatus MapAuthenticationStatus(
        AuthenticationAttemptStatus status)
    {
        return status switch
        {
            AuthenticationAttemptStatus.InvalidCredentials =>
                AuthSessionStatus.InvalidCredentials,

            AuthenticationAttemptStatus.AccountLocked =>
                AuthSessionStatus.AccountLocked,

            AuthenticationAttemptStatus.AccountInactive =>
                AuthSessionStatus.AccountInactive,

            AuthenticationAttemptStatus.RoleMismatch =>
                AuthSessionStatus.RoleMismatch,

            _ => AuthSessionStatus.InvalidCredentials
        };
    }
}
