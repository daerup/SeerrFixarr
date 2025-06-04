using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SeerrFixarr.App.Shared;

namespace SeerrFixarr.Test.Infrastructure;

internal static class TestTokenValidator
{
    public static ClaimsPrincipal TryValidateToken(string token, string secret, TimeProvider provider,
        out SecurityToken securityKey)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters()
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ClockSkew = TimeSpan.Zero,
        }.WithTimeProvider(provider);
        return tokenHandler.ValidateToken(token, validationParameters, out securityKey);
    }
}