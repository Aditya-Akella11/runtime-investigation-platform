using RuntimeInvestigation.Application.Features.Applications;
using RuntimeInvestigation.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoDbSettings>>().Value);
builder.Services.AddSingleton<IApplicationRepository, MongoApplicationRepository>();
builder.Services.AddScoped<ApplicationService>();

var app = builder.Build();

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

app.UseAuthorization();
app.MapControllers();

app.Run();
