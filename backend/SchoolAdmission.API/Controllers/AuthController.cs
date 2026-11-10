
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SchoolAdmission.API.Models.Auth;
using SchoolAdmission.API.Services.Auth;
using SchoolAdmission.BLL.Interfaces;
using SchoolAdmission.BLL.Models.Auth;

namespace SchoolAdmission.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly AuthCookieService _authCookieService;

    public AuthController(
        IAuthService authService,
        AuthCookieService authCookieService)
    {
        _authService = authService;
        _authCookieService = authCookieService;
    }

    // POST: /api/auth/student/register
    [HttpPost("student/register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterStudent(
        [FromBody] StudentRegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterStudentAsync(
            request.FullName,
            request.Email,
            request.Password,
            request.BirthRegistrationNumber,
            cancellationToken);

        if (!result.Succeeded)
        {
            var statusCode = result.Status switch
            {
                RegistrationStatus.DuplicateEmail or
                RegistrationStatus.DuplicateBirthRegistrationNumber
                    => StatusCodes.Status409Conflict,

                _ => StatusCodes.Status400BadRequest
            };

            return StatusCode(statusCode, new
            {
                code = result.Status.ToString().ToUpperInvariant(),
                message = result.Errors.FirstOrDefault()
                    ?? "Student registration failed.",
                errors = result.Errors
            });
        }

        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Student registration completed successfully.",
            userId = result.UserId,
            fullName = request.FullName.Trim(),
            email = request.Email.Trim()
        });
    }

    // POST: /api/auth/student/login
    [HttpPost("student/login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginStudent(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginStudentAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (result.Status != AuthSessionStatus.Succeeded ||
            result.Session is null)
        {
            return MapAuthenticationFailure(result.Status);
        }

        var session = result.Session;

        _authCookieService.SetRefreshToken(
            Request,
            Response,
            session.RefreshToken,
            session.RefreshTokenExpiresAtUtc);

        return Ok(MapAuthResponse(session));
    }

    // POST: /api/auth/refresh
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(
        CancellationToken cancellationToken)
    {
        if (!_authCookieService.IsAllowedFrontendOrigin(Request))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                code = "INVALID_ORIGIN",
                message = "The request origin is not allowed."
            });
        }

        var rawRefreshToken =
            _authCookieService.GetRefreshToken(Request);

        if (string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            _authCookieService.ClearRefreshToken(Request, Response);

            return Unauthorized(new
            {
                code = "REFRESH_TOKEN_MISSING",
                message = "A valid refresh token is required."
            });
        }

        var result = await _authService.RefreshSessionAsync(
            rawRefreshToken,
            cancellationToken);

        if (result.Status != AuthSessionStatus.Succeeded ||
            result.Session is null)
        {
            _authCookieService.ClearRefreshToken(Request, Response);

            return Unauthorized(new
            {
                code = "REFRESH_TOKEN_INVALID",
                message = "Your session has expired. Please log in again."
            });
        }

        var session = result.Session;

        _authCookieService.SetRefreshToken(
            Request,
            Response,
            session.RefreshToken,
            session.RefreshTokenExpiresAtUtc);

        return Ok(MapAuthResponse(session));
    }

    // POST: /api/auth/logout
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken)
    {
        if (!_authCookieService.IsAllowedFrontendOrigin(Request))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                code = "INVALID_ORIGIN",
                message = "The request origin is not allowed."
            });
        }

        var rawRefreshToken =
            _authCookieService.GetRefreshToken(Request);

        await _authService.LogoutAsync(
            rawRefreshToken ?? string.Empty,
            cancellationToken);

        _authCookieService.ClearRefreshToken(Request, Response);

        return Ok(new
        {
            message = "You have been logged out successfully."
        });
    }

    // GET: /api/auth/me
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(
        CancellationToken cancellationToken)
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new
            {
                code = "INVALID_TOKEN",
                message = "The authentication token is invalid."
            });
        }

        var user = await _authService.GetCurrentUserAsync(
            userId,
            cancellationToken);

        if (user is null)
        {
            return Unauthorized(new
            {
                code = "ACCOUNT_UNAVAILABLE",
                message = "This account is unavailable."
            });
        }

        return Ok(new
        {
            userId = user.Id,
            fullName = user.FullName,
            email = user.Email,
            birthRegistrationNumber = user.BirthRegistrationNumber,
            roles = user.Roles
        });
    }

    private IActionResult MapAuthenticationFailure(
        AuthSessionStatus status)
    {
        return status switch
        {
            AuthSessionStatus.AccountLocked =>
                StatusCode(StatusCodes.Status423Locked, new
                {
                    code = "ACCOUNT_LOCKED",
                    message = "Too many failed login attempts. Please try again later."
                }),

            AuthSessionStatus.AccountInactive =>
                StatusCode(StatusCodes.Status403Forbidden, new
                {
                    code = "ACCOUNT_INACTIVE",
                    message = "This account is inactive."
                }),

            AuthSessionStatus.RoleMismatch =>
                Unauthorized(new
                {
                    code = "INVALID_CREDENTIALS",
                    message = "Invalid email or password."
                }),

            _ => Unauthorized(new
            {
                code = "INVALID_CREDENTIALS",
                message = "Invalid email or password."
            })
        };
    }

    private static AuthResponse MapAuthResponse(
        AuthSessionData session)
    {
        return new AuthResponse
        {
            AccessToken = session.AccessToken,
            ExpiresAtUtc = session.AccessTokenExpiresAtUtc,
            UserId = session.User.Id,
            FullName = session.User.FullName,
            Email = session.User.Email,
            Role = session.User.Roles.FirstOrDefault() ?? string.Empty
        };
    }
}
