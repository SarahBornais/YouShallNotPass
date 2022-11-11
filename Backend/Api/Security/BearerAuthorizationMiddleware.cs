using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace YouShallNotPassBackend.Security
{
    public class BearerAuthorizationMiddleware
    {
        private const string schemePrefix = "Bearer ";

        private readonly RequestDelegate next;
        private readonly ITokenAuthority tokenAuthority;

        public BearerAuthorizationMiddleware(RequestDelegate next, ITokenAuthority tokenAuthority)
        {
            this.next = next;
            this.tokenAuthority = tokenAuthority;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string? token = GetTokenFromHeader(context);
            if (token != null) context.Items["User"] = tokenAuthority.ValidateTokenAndGetIdentity(token);

            await next(context);
        }

        private static string? GetTokenFromHeader(HttpContext context)
        {
            string headerValue = context.Request.Headers["Authorization"];
            if (headerValue == null || !headerValue.StartsWith(schemePrefix)) return null;

            return headerValue[schemePrefix.Length..];
        }
    }
}
