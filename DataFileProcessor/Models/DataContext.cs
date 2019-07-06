using System;
using System.Data.SqlClient;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace DataFileProcessor.Models
{
    public partial class DataContext : DbContext
    {
        private readonly IConfiguration _config;
        private static AzureServiceTokenProvider _azureServiceTokenProvider = new AzureServiceTokenProvider();

        public DataContext()
        {
        }

        public DataContext(DbContextOptions<DataContext> options, IConfiguration config)
            : base(options)
        {
            _config = config;

            // Get database connection string set on the DbContext in Startup.cs
            var dbConnection = (SqlConnection)this.Database.GetDbConnection();

            // Get TenantId from the configuration environment variable
            var tenantId = _config["TenantId"];

            // Get an Azure database access token for an MSI or a local user
            // This ensures a fresh and valid token is retrieved for every request
            // The AzureServiceTokenProvider class caches the token in memory and 
            // retrieves it from Azure AD just before expiration. 
            // Consequently, you no longer have to check the expiration before 
            // the GetAccessTokenAsync method. Just call the method when you want 
            // to use the token.
            var token = _azureServiceTokenProvider.GetAccessTokenAsync("https://database.windows.net/", tenantId).GetAwaiter().GetResult();

            // Set the AccessToken value on the SqlConnection object instance
            dbConnection.AccessToken = token;
        }

        public virtual DbSet<Person> People { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity<Person>(entity =>
            {
                entity.Property(e => e.PersonId).HasDefaultValueSql("(newid())");
            });
        }
    }
}
