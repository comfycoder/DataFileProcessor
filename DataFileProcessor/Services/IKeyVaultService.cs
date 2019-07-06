using System.Threading.Tasks;

namespace DataFileProcessor.Services
{
    public interface IKeyVaultService
    {
        Task<string> GetSecretAsync(string secretName);
    }
}