using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

public static class AuthExtensions
{
    public static IServiceCollection AddGoogleOAuth(this IServiceCollection services, IConfiguration config)
    {
        var googleAuth = config.GetSection("Authentication:Google");

        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddGoogle(options =>
            {
                options.ClientId = googleAuth["ClientId"];
                options.ClientSecret = googleAuth["ClientSecret"];
                options.CallbackPath = "/signin-google"; // стандартний шлях
            });

        return services;
    }
}
