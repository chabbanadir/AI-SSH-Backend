using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options; // crucial "using" to fix the IOptions error
using Microsoft.AspNetCore.Authentication.JwtBearer;  // For JwtBearerDefaults
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Backend.Middleware;
using Backend.Models;
using Backend.Context;
using Backend.Interfaces;
using Backend.Services;
using Backend.Services.Security;
using Backend.Services.SSH;
using Backend.Services.AI;
using Backend.Models.Entities;
using Backend.Interfaces.AI;


using Backend.Repository;
using Backend.Data;
using Backend.Extensions;

var builder = WebApplication.CreateBuilder(args);




// Configure services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreDB")));

// Tell DI how to resolve IAppDbContext: use the same AppDbContext 
builder.Services.AddScoped<IAppDbContext>(provider =>
    provider.GetRequiredService<AppDbContext>());

// Add Identity services
builder.Services.AddIdentity<AppUser, AppRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Use Extension Methods for Configuration
builder.Services.ConfigureIdentityOptions();
builder.Services.ConfigureCookieSettings();
builder.Services.ConfigureAuthorizationPolicies();
builder.Services.ConfigureDataProtection();

// Register other services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddHttpContextAccessor();

// Add AuthService and UserService
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

// AI Service
builder.Services.AddHttpClient<IAIService, AIService>(); 
builder.Services.AddSingleton<IAiConversationManager, AiConversationManager>();
builder.Services.AddScoped<IAiConversationService, AiConversationService>();

builder.Services.AddScoped<ISSHService, SSHService>();
builder.Services.AddSingleton<ISSHSessionManager, SSHSessionManager>();
builder.Services.AddLogging();

// Register Repositories (that depend on IAppDbContext)
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Add distributed memory cache (for session)
builder.Services.AddDistributedMemoryCache();

// Gemini

builder.Services.AddSession(options =>
{
    // Example: you can tweak idle timeout or cookie name
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add Controllers and Swagger
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Optional: add general info, doc version, etc.
    // options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    options.AddSecurityDefinition("cookieAuth", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Cookie,
        Name = ".AspNetCore.Identity.Application", // or your cookie name
        Description = "Cookie-based auth (ASP.NET Core Identity)"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "cookieAuth"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyCorsPolicy", policy =>
    {
        // You CANNOT use "*" for AllowAnyOrigin when AllowCredentials is used
        policy.WithOrigins("http://172.24.0.2:5173" , "http://172.24.0.3:5173") 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


// Program.cs
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors("MyCorsPolicy");

app.UseRouting();

app.UseSession();             // 1) Session
app.UseAuthentication();      // 2) Identity cookie auth
app.UseAuthorization();       // 3) Policies/roles
app.UseMiddleware<AuditMiddleware>(); // (Optional) if you want it here

app.MapControllers();
app.Run();
