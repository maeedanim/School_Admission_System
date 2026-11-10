
using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using SchoolAdmission.BLL.Interfaces;
using SchoolAdmission.BLL.Models.Auth;
using SchoolAdmission.DAL.Data;
using SchoolAdmission.DAL.Entities.Identity;

namespace SchoolAdmission.DAL.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly SchoolAdmissionDbContext _context;
    private readonly IConfiguration _configuration;

    public RefreshTokenRepository(
        SchoolAdmissionDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<(string Token, DateTime ExpiresAtUtc)> CreateAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expiresAtUtc = now.AddDays(GetExpirationDays());

        var rawToken = GenerateRawToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = HashToken(rawToken),
            CreatedAtUtc = now,
            ExpiresAtUtc = expiresAtUtc
        };

        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync(cancellationToken);

        return (rawToken, expiresAtUtc);
    }

    public async Task<RefreshTokenRotationResult?> RotateAsync(
        string rawToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return null;
        }

        var tokenHash = HashToken(rawToken);

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                cancellationToken);

        // Lock the existing token row to prevent concurrent rotation.
        var existingToken = await _context.RefreshTokens
            .FromSqlInterpolated($"""
                SELECT *
                FROM refresh_tokens
                WHERE "TokenHash" = {tokenHash}
                FOR UPDATE
                """)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingToken is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        var now = DateTime.UtcNow;

        // Reusing a revoked token indicates possible token theft.
        if (existingToken.RevokedAtUtc is not null)
        {
            await RevokeAllActiveTokensAsync(
                existingToken.UserId,
                now,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return null;
        }

        // Expired tokens cannot be rotated.
        if (existingToken.ExpiresAtUtc <= now)
        {
            existingToken.RevokedAtUtc = now;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return null;
        }

        // Check whether the associated account still exists and is active.
        var user = await _context.Users
            .AsNoTracking()
            .Where(user => user.Id == existingToken.UserId)
            .Select(user => new
            {
                user.Id,
                user.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null || !user.IsActive)
        {
            await RevokeAllActiveTokensAsync(
                existingToken.UserId,
                now,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return null;
        }

        // Generate a replacement refresh token.
        var newRawToken = GenerateRawToken();
        var newTokenHash = HashToken(newRawToken);
        var expiresAtUtc = now.AddDays(GetExpirationDays());

        // Revoke the old token and link it to its replacement.
        existingToken.RevokedAtUtc = now;
        existingToken.ReplacedByTokenHash = newTokenHash;

        var replacementToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = existingToken.UserId,
            TokenHash = newTokenHash,
            CreatedAtUtc = now,
            ExpiresAtUtc = expiresAtUtc
        };

        _context.RefreshTokens.Add(replacementToken);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new RefreshTokenRotationResult
        {
            UserId = existingToken.UserId,
            RefreshToken = newRawToken,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    public async Task RevokeAsync(
        string rawToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return;
        }

        var tokenHash = HashToken(rawToken);

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                cancellationToken);

        // Lock the row to prevent a concurrent refresh operation.
        var token = await _context.RefreshTokens
            .FromSqlInterpolated($"""
                SELECT *
                FROM refresh_tokens
                WHERE "TokenHash" = {tokenHash}
                FOR UPDATE
                """)
            .FirstOrDefaultAsync(cancellationToken);

        if (token is null || token.RevokedAtUtc is not null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return;
        }

        token.RevokedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task RevokeAllActiveTokensAsync(
        Guid userId,
        DateTime revokedAtUtc,
        CancellationToken cancellationToken)
    {
        await _context.RefreshTokens
            .Where(token =>
                token.UserId == userId &&
                token.RevokedAtUtc == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    token => token.RevokedAtUtc,
                    revokedAtUtc),
                cancellationToken);
    }

    private int GetExpirationDays()
    {
        var days = _configuration.GetValue<int>(
            "Jwt:RefreshTokenExpirationDays");

        if (days <= 0)
        {
            throw new InvalidOperationException(
                "Refresh token expiration must be greater than zero.");
        }

        return days;
    }

    private static string GenerateRawToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return WebEncoders.Base64UrlEncode(randomBytes);
    }

    private static string HashToken(string token)
    {
        var hashBytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(token));

        return Convert.ToHexString(hashBytes);
    }
}
