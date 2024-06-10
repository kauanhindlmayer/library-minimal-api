using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Library.Api.Auth;

public class ApiKeyAuthHandler(
    IOptionsMonitor<ApiKeyAuthSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<ApiKeyAuthSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey(HeaderNames.Authorization))
        {
            return Task.FromResult(AuthenticateResult.Fail("Authorization header is missing"));
        }

        var header = Request.Headers[HeaderNames.Authorization].ToString();
        if (header != Options.ApiKey)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, "john.doe@gmail.com"),
            new Claim(ClaimTypes.Name, "John Doe"),
        };

        var claimsIdentity = new ClaimsIdentity(claims, "ApiKey");

        var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), ApiKeySchemeConstants.SchemeName);
        
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}