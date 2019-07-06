using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataFileProcessor.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DataFileProcessor
{
    public class JsonFileBlobTrigger
    {
        private readonly DataContext _dbContext;

        public JsonFileBlobTrigger(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        [FunctionName("JsonFileBlobTrigger")]
        public async Task Run(
            [BlobTrigger("json-files/{name}", Connection = "AzureWebJobsStorage")]Stream blob,
            string name,
            ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed JSON blob\n Name:{name} \n Size: {blob.Length} Bytes");

            IEnumerable<Person> records;

            try
            {
                using (StreamReader reader = new StreamReader(blob))
                {
                    var text = await reader.ReadToEndAsync();

                    records = JsonConvert.DeserializeObject<IEnumerable<Person>>(text);
                }
            }
            catch (Exception ex)
            {
                log.LogError("ERROR: JsonFileBlobTrigger: Unable to read input blob.", ex);

                return;
            }

            try
            {
                _dbContext.People.AddRange(records);

                await _dbContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                log.LogError("ERROR: JsonFileBlobTrigger: Unable to save data.", ex);
            }
        }
    }
}
