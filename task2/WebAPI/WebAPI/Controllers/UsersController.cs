using Azure.Core;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Model.Dat;
using WebAPI.Model.Entities;
using WebAPI.Model.DTOs;
using WebAPI.Controllers.Services;
namespace WebAPI.Controllers;

[ApiController]
[Route("[Controller]")]
public class UsersController : ControllerBase
{
  private readonly WebAppDbContext _context;
  private JwtOptions _jwtOptions;
  private readonly ILogger<UsersController> _logger;

  public UsersController(WebAppDbContext context, JwtOptions jwtOptions, ILogger<UsersController> logger)
  {
    _context = context;
    _jwtOptions = jwtOptions;
    _logger = logger;
  }


  [HttpPost]
  [Route("SignUp")]
  public async Task<ActionResult<int>> SignUp([FromBody] UserSignUpRequest userRequest)
  {
    // Check data completeness
    if (!userRequest.IsDataCompleted())
      return BadRequest("User Data Not Completed");

    // Check if the username already exists
    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == userRequest.Username);
    if (existingUser != null)
      return Conflict("Username Already Taken");

    // Map DTO to User entity
    var newUser = new User
    {
      Name = userRequest.Name,
      Username = userRequest.Username,
      Password = BCrypt.Net.BCrypt.HashPassword(userRequest.Password),
      TwoFactorSecret = Otp.SecretKey()
    };

    try
    {
      // Add user into the database
      await _context.Users.AddAsync(newUser);
      await _context.SaveChangesAsync();
      return Ok(newUser.Id);
    }
    catch (Exception ex)
    {
      return StatusCode(500, $"Error Adding user: {ex.Message}");
    }
  }


  [HttpPost]
  [Route("LogIn")]
  public async Task<ActionResult> LogIn([FromBody] UserLoginRequest loginRequest)
  {
    if (loginRequest == null || string.IsNullOrWhiteSpace(loginRequest.Username) ||
        string.IsNullOrWhiteSpace(loginRequest.Password))
    {
      return BadRequest("Invalid login request");
    }

    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginRequest.Username);

    if (existingUser == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, existingUser.Password))
      return Unauthorized("Invalid username or password");

    var qrCode = Otp.QrCodeAsBase64(existingUser.TwoFactorSecret!, existingUser.Username!, _jwtOptions.Issuer!);

    return Ok(new { QrCode = qrCode });
  }

  [HttpPost]
  [Route("VerifyTwoFactor")]
  public ActionResult<UserLoginRespond> VerifyTwoFactor([FromBody] TFRequest twoFactorRequest)
  {
    if (twoFactorRequest == null || string.IsNullOrWhiteSpace(twoFactorRequest.OtpCode) ||
        !twoFactorRequest.UserId.HasValue)
    {
      return BadRequest("Invalid 2FA request");
    }

    var existingUser = _context.Users
        .FirstOrDefault(u => u.Id == twoFactorRequest.UserId);

    if (existingUser == null)
    {
      return Unauthorized("Invalid user");
    }

    if (string.IsNullOrEmpty(existingUser.TwoFactorSecret) || Otp.Verified(existingUser.TwoFactorSecret, twoFactorRequest.OtpCode))
    {
      return Unauthorized("Invalid OTP code");
    }

    string token = new JwtTokenGeneratorService(_jwtOptions).GenerateToken(existingUser.Username!);
    return Ok(new UserLoginRespond
    {
      Token = token
    });
  }



  [HttpPut]
  [Route("UpdateUser/{id}")]
  public async Task<ActionResult<int>> UpdateUser(int id, UserUpdateRequest user)
  {
    if (user == null || string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Password))
      return BadRequest("User data not completed");

    var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

    if (existingUser == null)
      return NotFound("User not found");

    // Update fields (if provided)
    bool dataChanged = false;
    if (!string.IsNullOrWhiteSpace(user.Name))
    {
      existingUser.Name = user.Name;
      dataChanged = true;
    }
    if (!string.IsNullOrWhiteSpace(user.Password))
    {
      dataChanged = true;
      existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
    }

    try
    {
      if (dataChanged)
        await _context.SaveChangesAsync();
      return Ok(existingUser.Id);
    }
    catch (DbUpdateConcurrencyException)
    {
      if (!await _context.Users.AnyAsync(x => x.Id == id))
        return NotFound("User no longer exists");
      return Conflict("User was updated by another process");
    }
    catch (Exception)
    {
      return StatusCode(500, "Failed to update user");
    }
  }
}