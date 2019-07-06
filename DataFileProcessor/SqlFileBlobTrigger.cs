using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DataFileProcessor.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataFileProcessor
{
    public class SqlFileBlobTrigger
    {
        private readonly DataContext _dbContext;

        public SqlFileBlobTrigger(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        [FunctionName("SqlFileBlobTrigger")]
        public async Task Run(
            [BlobTrigger("sql-files/{name}", Connection = "AzureWebJobsStorage")]Stream blob,
            string name,
            ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed SQL blob\n Name:{name} \n Size: {blob.Length} Bytes");

            string sql = null;

            try
            {
                using (StreamReader reader = new StreamReader(blob))
                {
                    sql = await reader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                log.LogError("ERROR: SqlFileBlobTrigger: Unable to read input blob.", ex);

                return;
            }

            try
            {
                var sb = new StringBuilder();

                sb.AppendLine("BEGIN");
                sb.AppendLine(sql);
                sb.AppendLine("END");

                var commandText = sb.ToString();

                _dbContext.Database.ExecuteSqlCommand(commandText);

                await _dbContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                log.LogError("ERROR: JsonFileBlobTrigger: Unable to save data.", ex);
            }
        }
    }
}
