using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options; // crucial "using" to fix the IOptions error
using Microsoft.AspNetCore.Authentication.JwtBearer;  // For JwtBearerDefaults
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;

using Backend.Models;
using Backend.Context;
using Backend.Interfaces;
using Backend.Services;
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
builder.Services.AddHttpClient<IAIService, AIService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<ISSHService, SSHService>();
builder.Services.AddSingleton<SSHSessionManager>();
builder.Services.AddScoped<IBulkInsertService, BulkInsertService>();
builder.Services.AddLogging();

// Register Repositories (that depend on IAppDbContext)
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Add distributed memory cache (for session)
builder.Services.AddDistributedMemoryCache();

// Enable session for temporary data storage
builder.Services.AddSession();

// Add Controllers and Swagger
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseRouting();

app.UseSession(); // If using session-based authentication

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        await DbInitializer.InitializeAsync(services);
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        // Optionally, rethrow the exception if you want the application to stop
        // throw;
    }
}

app.Run(); // This should keep the application running
