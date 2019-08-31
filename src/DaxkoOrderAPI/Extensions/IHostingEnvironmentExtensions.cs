using Microsoft.AspNetCore.Hosting;

namespace DaxkoOrderAPI.Extensions 
{
    public static class IHostingEnvironmentExtensions
    {
        public static bool IsDevelopmentOrLocal(this IHostingEnvironment environment)
        {
            return environment.IsDevelopment()
                   || environment.EnvironmentName.ToLower() == "clone"
                   || environment.EnvironmentName.ToLower() == "local";
        }
    }
}
