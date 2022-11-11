using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace YouShallNotPassBackend.Security
{
    public class JwtAuthority : ITokenAuthority
    {
        private readonly string issuer;
        private readonly string audience;
        private readonly SymmetricSecurityKey key;

        private readonly TokenValidationParameters tokenValidationParameters;

        public JwtAuthority(string issuer, string audience, byte[] key)
        {
            this.issuer = issuer;
            this.audience = audience;
            this.key = new SymmetricSecurityKey(key);

            tokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = this.issuer,
                ValidAudience = this.audience,
                IssuerSigningKey = this.key,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true
            };
        }

        public string GetToken(string identity)
        {
            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, identity)
                }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature)
            };

            JwtSecurityTokenHandler tokenHandler = new();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        public string? ValidateTokenAndGetIdentity(string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new();
                tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

                JwtSecurityToken jwtToken = (JwtSecurityToken)validatedToken;
                return jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
            }
            catch
            {
                return null;
            }
        }
    }

}
