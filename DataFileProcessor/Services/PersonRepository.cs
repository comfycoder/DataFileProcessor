using DataFileProcessor.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DataFileProcessor.Services
{
    public class PersonRepository : IPersonRepository
    {
        private readonly DataContext _dbContext;

        public PersonRepository(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool RecordExists(Person person)
        {
            var exists = _dbContext.People.AsNoTracking()
               .Any(x => x.PersonId == person.PersonId);

            return exists;
        }

        public void UpsertPerson(Person person)
        {
            if (RecordExists(person))
            {
                _dbContext.People.Update(person);
            }
            else
            {
                _dbContext.People.Add(person);
            }
        }

        public int SaveChanges()
        {
            return _dbContext.SaveChanges();
        }
    }
}
