using Weather.BusinessLogic;
using Weather.Clients;
using Weather.Clients.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Weather API",
        Version = "v1",
        Description = "Weather service API providing temperature, wind, and precipitation data"
    });
});

// Register IHttpContextAccessor for MockeryHandler to read incoming request headers
builder.Services.AddHttpContextAccessor();

// Register MockeryHandlerOptions from configuration
builder.Services.Configure<MockeryHandlerOptions>(
    builder.Configuration.GetSection("Mockery"));

// Register MockeryHandlerFactory for creating service-specific handlers
builder.Services.AddSingleton<IMockeryHandlerFactory, MockeryHandlerFactory>();

// Register the MockeryClient HttpClient (used by MockeryHandler to call the mock service)
builder.Services.AddHttpClient("MockeryClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Mockery:BaseUrl"]
        ?? "http://localhost:5000");
});

// Register HTTP clients for sensor services with service-specific MockeryHandlers
builder.Services.AddHttpClient<ITemperatureSensorClient, TemperatureSensorClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["SensorServices:Temperature:BaseUrl"]
        ?? "http://localhost:5001");
})
.AddHttpMessageHandler(sp => sp.GetRequiredService<IMockeryHandlerFactory>().Create("TemperatureSensor"));

builder.Services.AddHttpClient<IWindSensorClient, WindSensorClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["SensorServices:Wind:BaseUrl"]
        ?? "http://localhost:5002");
})
.AddHttpMessageHandler(sp => sp.GetRequiredService<IMockeryHandlerFactory>().Create("WindSensor"));

builder.Services.AddHttpClient<IPrecipitationSensorClient, PrecipitationSensorClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["SensorServices:Precipitation:BaseUrl"]
        ?? "http://localhost:5003");
})
.AddHttpMessageHandler(sp => sp.GetRequiredService<IMockeryHandlerFactory>().Create("PrecipitationSensor"));

// Register business logic services
builder.Services.AddScoped<IWeatherBusinessLogic, WeatherBusinessLogic>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
