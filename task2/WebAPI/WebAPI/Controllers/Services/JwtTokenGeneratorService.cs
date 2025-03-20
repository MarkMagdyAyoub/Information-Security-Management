using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI.Model.DTOs;
using WebAPI.Model.Entities;

namespace WebAPI.Controllers.Services;
public class JwtTokenGeneratorService
{
  private JwtOptions _jwtOptions;

  public JwtTokenGeneratorService(JwtOptions jwtOptions) {
    _jwtOptions = jwtOptions;
  }

  public string GenerateToken(string username) {
    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDecriptor = new SecurityTokenDescriptor
    {
      Issuer = _jwtOptions.Issuer,
      Audience = _jwtOptions.Audience,
      SigningCredentials =
          new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey)),
            SecurityAlgorithms.HmacSha256
          ),
      Subject = 
      new ClaimsIdentity(
          new Claim[] { 
            new Claim(ClaimTypes.NameIdentifier , username)
          }
        ),
    };
    var securityToken = tokenHandler.CreateToken(tokenDecriptor);
    return tokenHandler.WriteToken(securityToken);
  }
}
