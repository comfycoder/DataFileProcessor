using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using CsvHelper;
using System.Collections.Generic;
using CsvHelper.Configuration;
using DataFileProcessor.Models;
using DataFileProcessor.Services;

// See the following Pluralsight course for information on using the CsvReader class:
// https://app.pluralsight.com/library/courses/csharp-working-files-streams/table-of-contents
namespace DataFileProcessor
{
    public class CsvFileBlobTrigger
    {
        private readonly IPersonRepository _personRepository;

        public CsvFileBlobTrigger(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        [FunctionName("CsvFileBlobTrigger")]
        public void Run(
            [BlobTrigger("csv-batch-files/{name}", Connection = "AzureWebJobsStorage")]Stream blob,
            string name,
            ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed CSV blob\n Name:{name} \n Size: {blob.Length} Bytes");

            var startTime = DateTime.UtcNow;

            Console.WriteLine($"Started processing Batch ID {name} at {DateTime.UtcNow.ToString("MM-dd-yyy HH:mm:ss")}");

            using (var sr = new StreamReader(blob))
            using (var csvReader = new CsvReader(sr))
            {
                // Configure CSV reader options
                csvReader.Configuration.TrimOptions = TrimOptions.Trim; // Trim all whitespaces from fields
                csvReader.Configuration.Comment = '@'; // Set comment start character. Default is '#'
                csvReader.Configuration.AllowComments = true; // Allow comments in file
                csvReader.Configuration.IgnoreBlankLines = true; // Ignore blank lines in file
                csvReader.Configuration.Delimiter = ","; // Set the field delimiter character
                csvReader.Configuration.HasHeaderRecord = true; // Ensure a header row exists
                //csvReader.Configuration.HeaderValidated = null; // Ignore header validation step
                //csvReader.Configuration.MissingFieldFound = null; // Ignore missing field names
                    
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

                Random rnd = new Random();
                int count = 0;

                foreach (var item in records)
                {
                    count++;

                    try
                    {
                        ApplyBusinessRules(rnd, item);

                        _personRepository.UpsertPerson(item);
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"ERROR: CsvFileBlobTrigger: Unable to upsert person record with ID = {item.PersonId}.", ex);

                        throw;
                    }
                }

                try
                {
                    Console.WriteLine($"Found {count} records to process in Batch ID: {name}.");

                    _personRepository.SaveChanges();

                    log.LogInformation($"Successfully saved records for Batch ID: {name}");
                }
                catch (Exception ex)
                {
                    log.LogInformation($"Unable to save records for Batch ID: {name}");

                    log.LogError($"ERROR: CsvFileBlobTrigger: Batch ID = {name}.", ex);

                    throw;
                }
            }

            Console.WriteLine($"Finished processing Batch ID {name} at {DateTime.UtcNow.ToString("MM-dd-yyy HH:mm:ss")}");

            var endTime = DateTime.UtcNow;

            TimeSpan diff = (endTime - startTime);

            var totalTime = $"Total time to process Batch ID {name} " + string.Format("{0:00}:{1:00}:{2:00}.{3}", diff.Hours, diff.Minutes, diff.Seconds, diff.Milliseconds);

            Console.WriteLine(totalTime);

            Console.WriteLine();

            log.LogInformation(totalTime);
        }

        private void ApplyBusinessRules(Random rnd, Person item)
        {
            // Simulate a business rule that modified the record
            int number = rnd.Next(1, 10);

            item.LastName = $"{item.LastName}-{number}";
        }
    }
}
