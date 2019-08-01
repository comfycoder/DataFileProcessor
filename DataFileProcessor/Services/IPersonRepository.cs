using DataFileProcessor.Models;

namespace DataFileProcessor.Services
{
    public interface IPersonRepository
    {
        bool RecordExists(Person person);
        int SaveChanges();
        void UpsertPerson(Person person);
    }
}