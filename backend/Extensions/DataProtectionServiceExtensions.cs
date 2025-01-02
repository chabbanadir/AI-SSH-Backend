using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Extensions
{
    public static class DataProtectionServiceExtensions
    {
        public static IServiceCollection ConfigureDataProtection(this IServiceCollection services)
        {
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo("/app/data-protection-keys"))
                .SetApplicationName("Backend"); // Ensure the application name is consistent across instances

            return services;
        }
    }
}
