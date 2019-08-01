using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LargeBlobBuilder
{
    public partial class Person
    {
        public Person()
        {
        }

        [Index(0)]
        public Guid PersonId { get; set; }

        [Index(1)]
        public string FirstName { get; set; }

        [Index(2)]
        public string LastName { get; set; }

        [Index(3)]
        public string EmailAddress { get; set; }
    }
}
