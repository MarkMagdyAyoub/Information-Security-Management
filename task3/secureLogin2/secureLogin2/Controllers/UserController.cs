using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using secureLogin2.Model.DTOs;
using secureLogin2.Model.Entities;
using SecureLoginSys.Controllers.Utilities;
using SecureLoginSys.Model.Data;
using SecureLoginSys.Model.DTOs;
using SecureLoginSys.Model.Entities;
using System;
using WebAPI.Controllers.Services;

namespace SecureLoginSys.Controllers;

[Route("[Controller]")]
public class UserController : ControllerBase
{
  private readonly WebAppDbContext _context;
  private readonly JwtOptions _jwtOptions;
  private readonly JwtTokenGeneratorService _jwtService; 

  public UserController(WebAppDbContext context, JwtOptions jwtOptions, JwtTokenGeneratorService jwtService)
  {
    _context = context;
    _jwtOptions = jwtOptions;
    _jwtService = jwtService;
  }

  [HttpPost]
  [Route("register")]
  public async Task<IActionResult> Register([FromBody] SignUpViewModel model)
  {
    try
    {
      if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
        return BadRequest("All fields are required.");

      if (!model.Password.IsValidPassword(out string message))
        return BadRequest(message);

      if (await _context.Users.AnyAsync(u => u.Email == model.Email || u.Username == model.Username))
        return BadRequest("Email or username already exists.");

      var user = new User
      {
        Username = model.Username,
        Email = model.Email,
        PasswordHash = model.Password.HashPassword(),
        AuthMethod = "manual",
        CreatedAt = DateTime.UtcNow
      };

      _context.Users.Add(user);
      await _context.SaveChangesAsync();

      return Ok("Registration successful.");
    }
    catch (Exception ex)
    {
      return StatusCode(500, $"Internal server error: {ex.Message}");
    }
  }

  [HttpPost]
  [Route("login")]
  public async Task<IActionResult> Login([FromBody] LoginViewModel model)
  {
    try
    {
      var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username && u.AuthMethod == "manual");
      if (user == null || !model.Password!.VerifyPassword(user.PasswordHash!))
        return Unauthorized("Invalid email or password.");

      var token = _jwtService.GenerateToken(user.Username!);

      var log = new LoginLogInfo
      {
        UserId = user.Id,
        Timestamp = DateTime.UtcNow,
        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
      };

      await _context.LoginLogInfos.AddAsync(log);
      await _context.SaveChangesAsync();

      return Ok(new TokenResponse { Token = token, Message = "Login successful." });
    }
    catch (Exception ex)
    {
      return StatusCode(500, $"Internal server error: {ex.Message}");
    }
  }

  [HttpGet]
  [Route("signin-github")]
  public IActionResult GitHubLogin()
  {
    HttpContext.Session.SetString("TestSession", "SessionIsWorking");
    var redirectUrl = "https://localhost:7144/user/github-response";
    var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
    return Challenge(properties, "GitHub");
  }

  [HttpGet]
  [Route("github-response")]
  public async Task<IActionResult> GitHubResponse()
  {
    var result = await HttpContext.AuthenticateAsync("GitHub");
    if (!result.Succeeded)
      return Unauthorized("GitHub authentication failed: " + result.Failure?.Message);

    var claims = result.Principal?.Identities.FirstOrDefault()?.Claims;
    var githubUsername = claims?.FirstOrDefault(c => c.Type == "urn:github:login")?.Value;
    var githubEmail = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value ?? $"{githubUsername}@github.com";

    if (string.IsNullOrEmpty(githubUsername))
      return BadRequest("GitHub login failed.");

    var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == githubUsername && u.AuthMethod == "github");
    if (user == null)
    {
      user = new User
      {
        Username = githubUsername,
        Email = githubEmail,
        AuthMethod = "github",
        GitHubId = claims?.FirstOrDefault(c => c.Type == "urn:github:id")?.Value,
        CreatedAt = DateTime.UtcNow
      };

      var log = new LoginLogInfo
      {
        UserId = user.Id,
        Timestamp = DateTime.UtcNow,
        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
      };

      await _context.LoginLogInfos.AddAsync(log);
      await _context.Users.AddAsync(user);
      await _context.SaveChangesAsync();
    }

    var token = _jwtService.GenerateToken(user.Username!);
    return Ok(new TokenResponse { Token = token, Message = "GitHub login successful." });
  }

  [HttpPost("logout")]
  public async Task<IActionResult> Logout()
  {
    var token = Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");

    if (!string.IsNullOrEmpty(token))
    {
      new JwtTokenGeneratorService(_jwtOptions).InvalidateToken(token);
    }

    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    
    HttpContext.Session.Clear();

    return Ok(new { message = "Logged out successfully" });
  }
}