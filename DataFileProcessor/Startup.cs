using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.EntityFrameworkCore;
using DataFileProcessor.Models;
using System;
using Microsoft.ApplicationInsights;
using System.Net.Http;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using DataFileProcessor.Services;
using System.Data.SqlClient;

[assembly: FunctionsStartup(typeof(DataFileProcessor.Startup))]
namespace DataFileProcessor
{
    // Use dependency injection in .NET Azure Functions
    // https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
    // View or download a sample of different service lifetimes on GitHub.
    // https://github.com/Azure/azure-functions-dotnet-extensions/tree/master/src/samples/DependencyInjection/Scopes
    public class Startup : FunctionsStartup
    {
        public IConfigurationRoot Configuration { get; set; }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            // https://blog.jongallant.com/2018/01/azure-function-config/

            var basePath = Directory.GetCurrentDirectory();

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            this.Configuration = config;

            // https://docs.microsoft.com/en-us/azure/azure-functions/functions-monitoring#log-custom-telemetry-in-c-functions
            builder.Services.AddSingleton<TelemetryClient>();

            builder.Services.AddSingleton<HttpClient>();

            builder.Services.AddSingleton<IKeyVaultService, KeyVaultService>();

            var connectionString = GetSqlConnectionString();

            // The following registers DbContextOptions for 
            // dependency injection as scoped by default
            // NOTE: The SQL Server database AccessToken is retrieved in the
            // DbContext class to ensure it is current and valid for each request
            builder.Services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(connectionString,
                sqlServerOptionsAction: sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                })
            );

            builder.Services.AddScoped<IPersonRepository, PersonRepository>();
        }

        /// <summary>
        /// Build a valid SQL DB Connection String
        /// </summary>
        /// <returns>
        /// A valid SQL DB Connection String
        /// </returns>
        public string GetSqlConnectionString()
        {
            string tenantId = !string.IsNullOrWhiteSpace(Configuration["TenantId"]) ? Configuration["TenantId"] :
                throw new ApplicationException($"ERROR: Missing value for configuration parameter called 'TenantId'");

            string dbServer = !string.IsNullOrWhiteSpace(Configuration["DbServerName"]) ? Configuration["DbServerName"] :
                throw new ApplicationException($"ERROR: Missing value for configuration parameter called 'DbServerName'");

            string dbName = !string.IsNullOrWhiteSpace(Configuration["DbName"]) ? Configuration["DbName"] :
                throw new ApplicationException($"ERROR: Missing value for configuration parameter called 'DbName'");

            var builder = new SqlConnectionStringBuilder();

            builder["Data Source"] = $"tcp:{dbServer}.database.windows.net,1433";
            builder["Initial Catalog"] = dbName;
            builder["Connect Timeout"] = 30;
            builder["Persist Security Info"] = false;
            builder["TrustServerCertificate"] = false;
            builder["Encrypt"] = true;
            builder["MultipleActiveResultSets"] = false;

            var connectionString = builder.ToString();

            return connectionString;
        }
    }
}