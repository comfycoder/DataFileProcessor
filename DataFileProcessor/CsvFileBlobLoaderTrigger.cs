using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace DataFileProcessor
{
    public static class CsvFileBlobLoaderTrigger
    {
        [FunctionName("CsvFileBlobLoaderTrigger")]
        public static void Run(
            [BlobTrigger("csv-files/{name}")]Stream blob,
            string name,
            IBinder binder,
            ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {blob.Length} Bytes");

            // Code to find and output a specific line            

            int batchId = 0;
            int maxBatchSize = 100;
            string batchName = null;
            string batchBlob = null;
            string line;
            bool isHeaderLine = true;
            string headerLine = string.Empty;
            int count = 0;
            int totalCount = 0;

            var lines = new StringBuilder();

            using (var sr = new StreamReader(blob))
            {
                while (!sr.EndOfStream)
                {
                    // Retrieve the record header row
                    line = sr.ReadLine();

                    // Only retrieve the record header line on the first pass
                    if (isHeaderLine == true)
                    {
                        isHeaderLine = false;

                        Console.WriteLine();

                        // Store the record header line
                        headerLine = line;

                        Console.WriteLine(headerLine);

                        // Skip to the next line
                        continue;
                    }
                    // If this is the first record in the batch,
                    // add the record header line as the entry
                    else if (count == 0)
                    {
                        Console.WriteLine();

                        // Set the record header line for the new batch
                        lines.AppendLine(headerLine);

                        Console.WriteLine(headerLine);
                    }

                    // Determine whether the desire maximum 
                    // record count has been reached
                    if (count < maxBatchSize - 1)
                    {
                        totalCount++;

                        Console.WriteLine(totalCount);

                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            // Add the record line to the batch
                            lines.AppendLine(line);

                            Console.WriteLine(line);

                            count++;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            totalCount++;

                            Console.WriteLine(totalCount);

                            // Add the record line to the batch
                            lines.AppendLine(line);

                            Console.WriteLine(line);

                            // Add the record line to the batch
                            batchId++;

                            WriteBatchBlob(name, binder, batchId, out batchName, out batchBlob, lines);

                            // Reset counter and lines
                            count = 0;

                            lines.Clear();
                        }                        
                    }
                }

                // Check for any remaing records that
                // remain and write them to a new blob file
                if (count > 0)
                {
                    if (lines.Length > 0)
                    {
                        batchId++;

                        // Add the record line to the batch
                        WriteBatchBlob(name, binder, batchId, out batchName, out batchBlob, lines);

                        // Reset counter and lines
                        count = 0;

                        lines.Clear();
                    }
                }
            }
        }

        private static void WriteBatchBlob(string name, IBinder binder, int batchId, out string batchName, out string batchBlob, StringBuilder lines)
        {
            batchName = $"{name}-{batchId}";
            batchBlob = lines.ToString();
            BlobAttribute dynamicBlobBinding = new BlobAttribute(blobPath: $"csv-batch-files/{batchName}");
            using (var writer = binder.Bind<TextWriter>(dynamicBlobBinding))
            {
                writer.Write(batchBlob);
            }
        }
    }
}
