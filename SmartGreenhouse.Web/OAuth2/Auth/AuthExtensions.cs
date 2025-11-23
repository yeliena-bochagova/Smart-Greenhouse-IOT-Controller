using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

public static class AuthExtensions
{
    public static IServiceCollection AddGoogleOAuth(this IServiceCollection services, IConfiguration config)
    {
        var googleAuth = config.GetSection("Authentication:Google");

        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "Google";
            })
            .AddCookie()
            .AddOpenIdConnect("Google", options =>
            {
                options.ClientId = googleAuth["ClientId"];
                options.ClientSecret = googleAuth["ClientSecret"];
                options.Authority = "https://accounts.google.com";
                options.ResponseType = "code";

                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                options.SaveTokens = true;

                options.GetClaimsFromUserInfoEndpoint = true;
            });

        return services;
    }
}
