using Api.Endpoints;
using Api.Extensions;
using Api.Middleware;
using Application;
using Auth.Application;
using Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => { configuration.ReadFrom.Configuration(context.Configuration); });

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit       = 10;
        limiter.Window            = TimeSpan.FromMinutes(1);
        limiter.QueueLimit        = 0;
        limiter.AutoReplenishment = true;
    });
    options.AddFixedWindowLimiter("default", limiter =>
    {
        limiter.PermitLimit       = 60;
        limiter.Window            = TimeSpan.FromMinutes(1);
        limiter.QueueLimit        = 0;
        limiter.AutoReplenishment = true;
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseRateLimiter();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

await app.Services.InitializeDatabaseAsync();


app.MapApiV1Endpoints();

app.Run();