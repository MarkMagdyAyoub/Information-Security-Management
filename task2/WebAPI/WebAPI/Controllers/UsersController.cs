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
  public async Task<ActionResult<int>> SignUp(User user) {
    // check data completeness
    if (!user.IsDataCompleted())
      return BadRequest("User Data Not Completed");

    // Check if the username already exists
    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
    
    if (existingUser != null)
      return Conflict("Username Already Taken");

    // hash password using bcrypt hash algorithm
    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
   
    try
    {
      // add user into the database
      await _context.Users.AddAsync(user);
      await _context.SaveChangesAsync();
      return Ok(user.Id);
    }
    catch (Exception)
    {
      return StatusCode(500, "Error Adding user");
    }
  }

  [HttpPost]
  [Route("Login")]
  public async Task<ActionResult<UserLoginRespond>> LogIn([FromBody] UserLoginRequest loginRequest)
  {
    if (string.IsNullOrWhiteSpace(loginRequest.Username) || string.IsNullOrWhiteSpace(loginRequest.Password))
      return BadRequest("Username and password are required");

    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginRequest.Username);

    if (existingUser == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, existingUser.Password))
      return Unauthorized("Invalid Username Or Password");

    // generate user token
    string token = new JwtTokenGeneratorService(_jwtOptions).GenerateToken(loginRequest.Username);
    return Ok(new UserLoginRespond { Id = existingUser.Id , Token = token});
  }


  [HttpPut]
  [Route("UpdateUser/{id}")]
  public async Task<ActionResult<int>> UpdateUser(int id, UserUpdateRequest user){
    if (user == null || string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Password))
      return BadRequest("User data not completed");

    var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

    if (existingUser == null)
      return NotFound("User not found");

    // Update fields (if provided)
    bool dataChanged = false;
    if (!string.IsNullOrWhiteSpace(user.Name)){
      existingUser.Name = user.Name;
      dataChanged = true;
    }
    if (!string.IsNullOrWhiteSpace(user.Password)){
      dataChanged = true;
      existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
    }
    
    try{
      if(dataChanged)
        await _context.SaveChangesAsync();
      return Ok(existingUser.Id);
    }
    catch (DbUpdateConcurrencyException){
      if (!await _context.Users.AnyAsync(x => x.Id == id))
        return NotFound("User no longer exists");
      return Conflict("User was updated by another process");
    }
    catch (Exception){
      return StatusCode(500,"Failed to update user");
    }
  }
}
