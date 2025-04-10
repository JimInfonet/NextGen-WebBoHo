namespace InfonetCSCR.Services.Helpers;

public static class AuthenticationHelper
{
  public static string CreateToken(string userCode, string issuer, string audience, byte[] key, int expireTime)
  {
    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(new[]
        {
              new Claim("Id", Guid.NewGuid().ToString()),
              new Claim(JwtRegisteredClaimNames.Sub, userCode),
              new Claim(JwtRegisteredClaimNames.Email, userCode),
              new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
          }),
      Expires = DateTime.UtcNow.AddMinutes(expireTime),
      Issuer = issuer,
      Audience = audience,
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
    };
    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);
    var jwtToken = tokenHandler.WriteToken(token);
    return jwtToken;
  }

  public static string GenerateRefreshToken()
  {
    var randomNumber = new byte[64];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomNumber);
    return Convert.ToBase64String(randomNumber);
  }

  public static ClaimsPrincipal GetPrincipalFromExpiredToken(string? token, string issuer, string audience, byte[] key)
  {
    var tokenValidationParameters = new TokenValidationParameters
    {
      ValidIssuer = issuer,
      ValidAudience = audience,
      IssuerSigningKey = new SymmetricSecurityKey(key),

      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateIssuerSigningKey = true,
      ValidateLifetime = false,
    };
    var tokenHandler = new JwtSecurityTokenHandler();
    SecurityToken securityToken;
    var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
    var jwtSecurityToken = securityToken as JwtSecurityToken;
    if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
      throw new SecurityTokenException("Invalid token");
    return principal;
  }
}
