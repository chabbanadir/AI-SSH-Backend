using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Backend.Models;
using Backend.Data;
using Backend.Context;
using Backend.Interfaces;
using Backend.Services;
using Backend.Repository;
using Microsoft.Extensions.Options; // crucial "using" to fix the IOptions error
using Microsoft.AspNetCore.Authentication.JwtBearer;  // For JwtBearerDefaults
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<ITokenService, TokenService>();

// 1. Read the config
var jwtSettingsSection = builder.Configuration.GetSection("Jwt");

var jwtConfig = jwtSettingsSection.Get<JwtConfig>(); 
// or read them individually, e.g. jwtSettingsSection["Key"]

builder.Services.Configure<JwtConfig>(jwtSettingsSection);

// 2. Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set this to true in production
    options.SaveToken = true; 
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtConfig.Issuer,
        ValidAudience = jwtConfig.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtConfig.Key)),

        ClockSkew = TimeSpan.Zero // Remove default 5 min buffer
    };
});

// Configure services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreDB")));

// 2. Tell DI how to resolve IAppDbContext: use the same AppDbContext 
builder.Services.AddScoped<IAppDbContext>(provider =>
    provider.GetRequiredService<AppDbContext>());

// 2) Add Identity services
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// 3) Add your AuthService and UserService
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddDistributedMemoryCache(); // Add this line before AddSession

// 3. RRegister Repositories (that depend on IAppDbContext)
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));


// Enable session for temporary data storage
builder.Services.AddSession();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseHttpsRedirection();

app.Run();

