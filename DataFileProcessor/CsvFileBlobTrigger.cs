using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using CsvHelper;
using System.Collections.Generic;
using System.Diagnostics;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using DataFileProcessor.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;

// See the following Pluralsight course for information on using the CsvReader class:
// https://app.pluralsight.com/library/courses/csharp-working-files-streams/table-of-contents
namespace DataFileProcessor
{
    public class CsvFileBlobTrigger
    {
        private readonly DataContext _dbContext;

        public CsvFileBlobTrigger(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        [FunctionName("CsvFileBlobTrigger")]
        public async Task Run(
            [BlobTrigger("csv-files/{name}", Connection = "AzureWebJobsStorage")]Stream blob,
            string name,
            ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed CSV blob\n Name:{name} \n Size: {blob.Length} Bytes");

            using (var sr = new StreamReader(blob))
            using (var csvReader = new CsvReader(sr))
            {
                IEnumerable<Person> records = null;

                try
                {
                    records = csvReader.GetRecords<Person>();
                }
                catch (Exception ex)
                {
                    log.LogError("ERROR: CsvFileBlobTrigger: Unable to read input blob.", ex);

                    return;
                }                

                // Configure CSV reader options
                csvReader.Configuration.TrimOptions = TrimOptions.Trim; // Trim all whitespaces from fields
                csvReader.Configuration.Comment = '@'; // Set comment start character. Default is '#'
                csvReader.Configuration.AllowComments = true; // Allow comments in file
                csvReader.Configuration.IgnoreBlankLines = true; // Ignore blank lines in file
                csvReader.Configuration.Delimiter = ","; // Set the field delimiter character
                csvReader.Configuration.HasHeaderRecord = true; // Ensure a header row exists
                //csvReader.Configuration.HeaderValidated = null; // Ignore header validation step
                //csvReader.Configuration.MissingFieldFound = null; // Ignore missing field names

                try
                {
                    _dbContext.People.AddRange(records);

                    await _dbContext.SaveChangesAsync();

                }
                catch (Exception ex)
                {
                    log.LogError("ERROR: CsvFileBlobTrigger: Unable to save data.", ex);
                }
            }
        }
    }
}
