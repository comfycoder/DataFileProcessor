using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataFileProcessor.Models
{
    [Table("Person", Schema = "dbo")]
    public partial class Person
    {
        public Person()
        {
        }

        [Key]
        [Column("PersonId")]
        public Guid PersonId { get; set; }

        [Column("FirstName")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Column("LastName")]
        [StringLength(50)]
        public string LastName { get; set; }

        [Column("EmailAddress")]
        [StringLength(255)]
        public string EmailAddress { get; set; }
    }
}
