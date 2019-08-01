using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace LargeBlobBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            GenerateBigCsvFile();
        }

        private static void GenerateBigCsvFile()
        {
            using (var sr = new StreamReader(@"C:\srcMM\DataFileProcessor\MockData\MOCK_DATA.csv"))
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

                var list = new List<Person>();

                var records = csvReader.GetRecords<Person>();

                foreach (var item in records)
                {
                    var person = new Person();

                    person.PersonId = Guid.NewGuid();
                    person.FirstName = item.FirstName;
                    person.LastName = item.LastName;
                    person.EmailAddress = item.EmailAddress;

                    list.Add(person);
                }

                var newList = new List<Person>();

                for (int i = 0; i < 1000; i++)
                {
                    foreach (var item in list)
                    {
                        var person = new Person();

                        person.PersonId = Guid.NewGuid();
                        person.FirstName = item.FirstName + i;
                        person.LastName = item.LastName + i;
                        person.EmailAddress = item.EmailAddress;

                        newList.Add(person);
                    }
                }

                using (var writer = new StreamWriter(@"C:\srcMM\DataFileProcessor\LargeBlobData\LARGE_BLOB_DATA.csv"))
                using (var csv = new CsvWriter(writer))
                {
                    csv.WriteRecords(newList);
                }
            }
        }

        private static void GenerateBigTextFile()
        {
            using (var sw = new StreamWriter(@"c:\temp\bigblob.txt"))
            {
                for (int i = 0; i < 40_000_000; i++)
                {
                    sw.WriteLine("Some line we are not interested in processing");
                }

                sw.WriteLine("Data: 42");
            }
        }
    }
}
