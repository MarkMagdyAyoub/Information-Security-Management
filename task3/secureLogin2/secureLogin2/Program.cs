using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using secureLogin2.Model.Entities;
using SecureLoginSys.Model.Data;
using System.Text;
using WebAPI.Controllers.Services;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
var constr = config.GetSection("constr").Value;
var jwtOptions = config.GetSection("Jwt").Get<JwtOptions>();
var githubOptions = config.GetSection("GitHub").Get<GithubOption>();

if (string.IsNullOrEmpty(githubOptions?.ClientSecret) || string.IsNullOrEmpty(githubOptions?.ClientId))
  throw new InvalidOperationException("GithubOption Fields Not Completed");

if (string.IsNullOrEmpty(jwtOptions?.SigningKey))
  throw new InvalidOperationException("JWT SigningKey is missing or empty in configuration.");

builder.Services.Configure<JwtOptions>(config.GetSection("Jwt"));
builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton<JwtTokenGeneratorService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Default sign-in scheme
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
      options.SaveToken = true;
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtOptions.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
                      Encoding.UTF8.GetBytes(jwtOptions.SigningKey)
                  ),
        ValidateLifetime = true,
      };
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOAuth("GitHub", options =>
    {
      options.ClientId = githubOptions.ClientId;
      options.ClientSecret = githubOptions.ClientSecret;
      options.CallbackPath = "/user/github";
      options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
      options.TokenEndpoint = "https://github.com/login/oauth/access_token";
      options.UserInformationEndpoint = "https://api.github.com/user";
      

      options.SaveTokens = true;

      options.Scope.Add("user:email");

      options.ClaimActions.MapJsonKey("urn:github:login", "login");
      options.ClaimActions.MapJsonKey("urn:github:id", "id");
      options.ClaimActions.MapJsonKey("urn:github:url", "html_url");

      options.Events.OnCreatingTicket = async context =>
      {
        var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);

        var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
        response.EnsureSuccessStatusCode();

        var user = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        context.RunClaimActions(user.RootElement);
      };

    });

builder.Services.AddDistributedMemoryCache();


// Configure session middleware
builder.Services.AddSession(options =>
{
  options.Cookie.HttpOnly = true;
  options.Cookie.IsEssential = true;
  options.Cookie.SameSite = SameSiteMode.Lax;
  options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
  options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.AddDbContext<WebAppDbContext>(options => options.UseSqlServer(constr));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllers();

app.Run();
