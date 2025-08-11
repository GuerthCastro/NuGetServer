using NuGetServer.Entities.Config;
using NuGetServer.Services;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
     builder.WebHost.UseUrls("http://0.0.0.0:8080;https://0.0.0.0:8081");
}


builder.Host
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        var env = hostingContext.HostingEnvironment;

        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((hostContext, services) =>
    {
        var serviceConfig = new ServiceConfig();
        var swaggerConfig = new Swagger();
        var nuGetIndex = new NuGetIndex();
        var nuGetServer = new NuGetServer.Entities.Config.NuGetServer();

        hostContext.Configuration.Bind(nameof(ServiceConfig), serviceConfig);
        hostContext.Configuration.Bind(nameof(Swagger), swaggerConfig);
        hostContext.Configuration.Bind(nameof(NuGetServer), nuGetServer);
        hostContext.Configuration.Bind(nameof(NuGetIndex), nuGetIndex);

        serviceConfig.OS = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
        serviceConfig.Environment = builder.Environment.EnvironmentName;
        serviceConfig.ServiceVersion = Environment.GetEnvironmentVariable("BUILD_VERSION") ?? "1.0.0.0";

        services.AddControllers().AddNewtonsoftJson();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(swaggerConfig.Version, new OpenApiInfo
            {
                Title = swaggerConfig.Title,
                Version = swaggerConfig.Version,
                Description = swaggerConfig.Description
            });
        });

        services.AddSingleton(serviceConfig);
        services.AddSingleton(swaggerConfig);
        services.AddSingleton(nuGetServer);
        services.AddSingleton(nuGetIndex);
        services.AddSingleton<IPackageStorageService, PackageStorageService>();
    });

var swaggerConfig = builder.Configuration.GetSection("Swagger");

var app = builder.Build();

// Always enable Swagger UI for API documentation
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    var version = swaggerConfig["Version"];
    c.SwaggerEndpoint($"/swagger/{version}/swagger.json", swaggerConfig["Title"]);
    c.RoutePrefix = "swagger"; // Hace que Swagger est� en /swagger
});

// Enforce HTTPS in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
