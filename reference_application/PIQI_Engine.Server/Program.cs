using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using PIQI.Components.Models;
using PIQI.Components.Services;
using PIQI.Data;
using PIQI_Engine.Server.Engines;
using PIQI_Engine.Server.Services;
using Serilog;
using System.Reflection;

namespace PIQI_Engine.Server;

public partial class Program
{
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
    
        // Add CORS policy
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        OpenApiInfo openApi = builder.Configuration.GetSection("ApiDefinition").Get<OpenApiInfo>();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(openApi.Version, openApi);
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
        });

        builder.Services.AddSingleton<FileCacheService>();

        // Add engine
        builder.Services.AddScoped<PIQIEngine>();
        builder.Services.AddScoped<ReferenceDataEngine>();

        // Add client for FHIR server
        builder.Services.AddHttpClient<IFHIRClientProvider, FHIRClientProvider>(client =>
        {
            // Pull base URL from configuration
            var baseUrl = builder.Configuration["Fhir:BaseUrl"];

            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/fhir+json");
        });


        builder.Services.AddDbContext<PIQIDbContext>(options =>
        {
            var dbType = builder.Configuration["Database:Type"];
            var connStr = builder.Configuration.GetConnectionString("PIQIDatabase");

            switch (dbType)
            {
                case "SqlServer":
                    options.UseSqlServer(connStr);
                    break;
                case "PostgreSQL":
                    options.UseNpgsql(connStr);
                    break;
                case "MySQL":
                    options.UseMySql(connStr, ServerVersion.AutoDetect(connStr));
                    break;
                case "SQLite":
                    options.UseSqlite(connStr);
                    break;
                    // Add more as needed
            }
        });

        // Client for API SAMs, etc.
        builder.Services.AddHttpClient();

        builder.Services.AddScoped<SAMService>();
        builder.Services.AddSingleton<SAMWorkerRegistry>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var samRegistry = scope.ServiceProvider.GetRequiredService<SAMWorkerRegistry>();
            var samService = scope.ServiceProvider.GetRequiredService<SAMService>();
            var cache = scope.ServiceProvider.GetRequiredService<FileCacheService>();

            Func<string, SAM> samResolver = mnemonic =>
            {
                cache.Get<SAM>(mnemonic, out var item);
                return item?.Value ?? throw new Exception($"SAM not found for mnemonic {mnemonic}");
            };

            samRegistry.LoadFromAssembly(Assembly.GetExecutingAssembly(), samService, samResolver);

            string pluginFolder = builder.Configuration.GetValue<string>("FilePaths:PluginFolder") ?? "Plugins";

            // If the path is not rooted, make it relative to the base directory
            if (!Path.IsPathRooted(pluginFolder))
                pluginFolder = Path.Combine(AppContext.BaseDirectory, pluginFolder);
            //if custom plugings are not being added/updated, check the appsettings location
            //to ensure the path is correct and the appsettings is being read properly
            if (Directory.Exists(pluginFolder))
            {
                foreach (var dll in Directory.GetFiles(pluginFolder, "*.dll"))
                {
                    var assembly = Assembly.LoadFrom(dll);
                    samRegistry.LoadFromAssembly(assembly, samService, samResolver);
                }
            }
        }

        var loggerFactory = app.Services.GetService<ILoggerFactory>();
        var logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext().CreateLogger();
        loggerFactory.AddSerilog(logger);

        if (app.Environment.IsDevelopment())
        {
            // Redirects to HTTPS only when you are debugging locally
            app.UseHttpsRedirection();
        }
        app.UseRouting();

        app.UseCors("AllowAll");

        app.UseDefaultFiles();
        app.UseStaticFiles();

        // Swagger is exposed for the Reference App environment or Development
        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("ReferenceApp"))
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(builder.Configuration["CustomSwaggerUI:SwaggerEndpoint"], openApi.Version);
                options.InjectStylesheet(builder.Configuration["CustomSwaggerUI:CSSPath"]);
                options.InjectJavascript(builder.Configuration["CustomSwaggerUI:JSPath"], "text/javascript");
                options.DocumentTitle = builder.Configuration["CustomSwaggerUI:DocTitle"];
                options.DefaultModelsExpandDepth(-1);
            });
        }

        app.MapControllers();

        app.MapFallbackToFile("/index.html");

        app.Run();
    }
}
