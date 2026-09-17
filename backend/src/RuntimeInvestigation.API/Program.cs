using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RuntimeInvestigation.API.Models;
using RuntimeInvestigation.Application.Features.Applications;
using RuntimeInvestigation.Application.Features.Investigations;
using RuntimeInvestigation.Application.Features.Probes;
using RuntimeInvestigation.Application.Features.Users;
using RuntimeInvestigation.Infrastructure.Persistence.Repositories;
using RuntimeInvestigation.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoDbSettings>>().Value);
builder.Services.AddScoped<IApplicationRepository, MongoApplicationRepository>();
builder.Services.AddScoped<IInvestigationRepository, MongoInvestigationRepository>();
builder.Services.AddScoped<IProbeRepository, MongoProbeRepository>();
builder.Services.AddScoped<IProbeResultRepository, MongoProbeResultRepository>();
builder.Services.AddScoped<IUserRepository, MongoUserRepository>();
builder.Services.AddScoped<RuntimeInvestigation.Application.Features.Templates.ITemplateRepository, MongoTemplateRepository>();
builder.Services.AddSingleton<DemoWorkflowStore>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<Microsoft.Extensions.Options.IOptions<JwtSettings>>().Value);

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = signingKey,
        ValidateLifetime = true
    };
});

builder.Services.AddScoped<RuntimeInvestigation.Application.Common.Interfaces.ITenantContext, RuntimeInvestigation.Application.Common.Interfaces.TenantContext>();
builder.Services.AddScoped<ApplicationService>();
builder.Services.AddScoped<InvestigationService>();
builder.Services.AddScoped<ProbeService>();
builder.Services.AddScoped<RuntimeInvestigation.Application.Features.Users.UserService>();
builder.Services.AddScoped<RuntimeInvestigation.Application.Features.Templates.TemplateService>();
builder.Services.AddScoped<RuntimeInvestigation.Application.Features.Approval.ApprovalService>();
builder.Services.AddSingleton<MockAgentProbeDispatcher>();
builder.Services.AddSingleton<IProbeDispatcher>(sp => sp.GetRequiredService<MockAgentProbeDispatcher>());
builder.Services.AddSingleton<RuntimeInvestigation.Application.Features.Probes.IAgentDispatcher>(sp => sp.GetRequiredService<MockAgentProbeDispatcher>());
builder.Services.AddHostedService<ProbeExpirationService>();

var app = builder.Build();

_ = Task.Run(async () =>
{
    try
    {
        using var scope = app.Services.CreateScope();
        var templateRepo = scope.ServiceProvider.GetService<RuntimeInvestigation.Application.Features.Templates.ITemplateRepository>();
        if (templateRepo != null)
        {
            await RuntimeInvestigation.Infrastructure.Persistence.TemplateSeeder.SeedAsync(templateRepo);
        }
    }
    catch
    {
        // Safe fallback if database is unreachable during startup
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var disableHttpsRedirection = builder.Configuration.GetValue<bool>("DisableHttpsRedirection");
if (!disableHttpsRedirection)
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseMiddleware<RuntimeInvestigation.API.Middleware.TenantContextMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

app.Run();

public partial class Program {}
