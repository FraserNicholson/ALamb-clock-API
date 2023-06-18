using System.Net;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Shared.Options;

namespace ALamb_clock_API.Middleware;

public class ApiKeyAuthenticationMiddleware 
{
    private readonly RequestDelegate _next;
    private readonly AuthenticationOptions _authenticationOptions;

    public ApiKeyAuthenticationMiddleware(RequestDelegate next, IOptions<AuthenticationOptions> authenticationOptions)
    {
        _next = next;
        _authenticationOptions = authenticationOptions.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var apiKeyHeader = context.Request.Headers[_authenticationOptions.ApiKeyHeaderName];
        
        if (apiKeyHeader != _authenticationOptions.ApiKeyValue)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.StartAsync();
            return;
        }

        await _next(context);
    }
}