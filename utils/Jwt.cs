using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Magnolia_cares.authentication.entities;

namespace Magnolia_cares.utils
{
    public class JwtTokenGenerator
    {
        public static string GenerateToken(dynamic data, string role_type)
        {
            var claimsList = new List<Claim>();

            var users = ((IEnumerable<dynamic>)data).ToList();

            foreach (var res in users)
            {
                var id = res.id.ToString();
                var organization_id = "";

                if (res != null && res.organization_id != null)
                {
                    organization_id = res.organization_id.ToString();
                }

                claimsList.AddRange(new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, id),
                new Claim("name", res.first_name),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Email, res.email),
                new Claim("id", id),
                });
                if (role_type == "user")
                {
                    claimsList.Add(new Claim("org_id", organization_id));
                    Console.WriteLine(organization_id); //raw data

                }

                if (role_type == "super-admin")
                {
                    claimsList.AddRange(new[]
                    {
                new Claim("role_type", "SA")
                });
                }
                else
                {
                    claimsList.AddRange(new[]
                    {
                new Claim("role_type", role_type)
                 });
                }
            }

            string secretKey = "qwertyuiopasdfghjklzxcvbnm1234567890";

            var key = Encoding.UTF8.GetBytes(secretKey);
            var securityKey = new SymmetricSecurityKey(key);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddDays(10); // Set expiration time to 10 days from now in UTC

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claimsList),
                Expires = expiration,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            string jwtTokenString = tokenHandler.WriteToken(token);

            return jwtTokenString;
        }

    }
}
