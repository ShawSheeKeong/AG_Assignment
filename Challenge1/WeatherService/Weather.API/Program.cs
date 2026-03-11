using Microsoft.EntityFrameworkCore;
using Polly;
using Serilog;
using Weather.Application.Interfaces;
using Weather.Application.Services;
using Weather.Domain.Options;
using Weather.Infrastructure;
using Weather.Infrastructure.External;
using Weather.Infrastructure.Repositories;
using Weather.Infrastructure.Workers;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

var settings = builder.Configuration.GetSection(nameof(OpenWeatherSettings));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));
builder.Services.AddScoped<WeatherService>();
builder.Services.AddScoped<AlertService>();
builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddHttpClient<IWeatherProvider, OpenWeatherClient>(client =>
{
    client.BaseAddress = new Uri(settings.Get<OpenWeatherSettings>()!.Uri);
}).AddTransientHttpErrorPolicy(policy =>
    policy.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2)));
builder.Services.AddHostedService<TemperatureWorker>();

builder.Services.Configure<OpenWeatherSettings>(settings);
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(nameof(EmailSettings)));
builder.Services.Configure<SubscriptionJobSettings>(builder.Configuration.GetSection(nameof(SubscriptionJobSettings)));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
