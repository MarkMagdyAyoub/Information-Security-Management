using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebAPI.Controllers;
using WebAPI.Model.Dat;
using WebAPI.Model.Entities;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
var constr = config.GetSection("constr").Value;
var jwtOptions = config.GetSection("Jwt").Get<JwtOptions>();

// Ensure JWT signing key is valid
if (string.IsNullOrEmpty(jwtOptions?.SigningKey))
  throw new InvalidOperationException("JWT SigningKey is missing or empty in configuration.");

// Configure services
builder.Services.Configure<JwtOptions>(config.GetSection("Jwt"));
builder.Services.AddSingleton(jwtOptions);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register authentication services
builder.Services
  .AddAuthentication() // Register authentication services 
  .AddJwtBearer(  // Configures JWT Bearer authentication
      JwtBearerDefaults.AuthenticationScheme, // use default scheme
      options =>
      {
        // Saves the token for later use in the request
        options.SaveToken = true;
        // Defines how the JWT will be validated
        options.TokenValidationParameters = new TokenValidationParameters
        {
          // Ensures the token's issuer is checked
          ValidateIssuer = true,
          // Compare it with the following Issuer
          ValidIssuer = jwtOptions.Issuer,
          // Ensures the token's audience is checked
          ValidateAudience = true,
          // Compare it with the following Audience
          ValidAudience = jwtOptions.Audience,
          // Ensures the signing key is validated
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtOptions.SigningKey)
                ),
          // Ensures the token hasn’t expired
          ValidateLifetime = true,
        };
      }
    );

// Configure DbContext
builder.Services.AddDbContext<WebAppDbContext>(options => options.UseSqlServer(constr));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();