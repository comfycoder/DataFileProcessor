//using System;
//using DataFileProcessor.Models;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Host;
//using Microsoft.Extensions.Logging;

//namespace DataFileProcessor
//{
//    public static class CsvFileQueueTrigger
//    {
//        [FunctionName("CsvFileQueueTrigger")]
//        public static void Run([QueueTrigger("csv-batch-files", Connection = "AzureWebJobsStorage")]string queueItem,
//            ICollector<Person> queueItems,
//            ILogger log)
//        {
//            //log.LogInformation($"C# Queue trigger function processed: {queueItems}");

            
//        }
//    }
//}
