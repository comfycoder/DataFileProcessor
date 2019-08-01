using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DataFileProcessor
{
    public static class DataFileBlobTrigger
    {
        [FunctionName("DataFileBlobTrigger")]
        public static void Run(
            [BlobTrigger("big-blobs/{name}")]Stream blob,
            string name,
            [Blob("small-blobs-out/{name}")] out string foundData,
            ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {blob.Length} Bytes");

            // Code to find and output a specific line            

            foundData = null; // Don't write an output blob by default

            int count = 0;

            string line;

            var lines = new StringBuilder();

            using (var sr = new StreamReader(blob))
            {
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();

                    if (count < 100)
                    {
                        lines.Append(line);

                        count++;
                    }
                    else
                    {
                        count = 0;
                    }

                    if (line.StartsWith("Data"))
                    {
                        foundData = line;
                        break;
                    }
                }
            }
        }
    }
}
