using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Extensions
{
   public static class CookieServiceExtensions
{
    public static IServiceCollection ConfigureCookieSettings(this IServiceCollection services)
    {

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensure cookies are sent over HTTPS only
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.LoginPath = "/api/users/login";
            options.LogoutPath = "/api/users/logout";
            options.AccessDeniedPath = "/api/users/access-denied";
            options.SlidingExpiration = true;
        });


        return services;
    }
}

}