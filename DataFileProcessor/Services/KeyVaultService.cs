using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Caching.Memory;

namespace DataFileProcessor.Services
{
    public class KeyVaultService : IKeyVaultService
    {
        private readonly IConfiguration _config;
        private IMemoryCache _memoryCache;

        public KeyVaultService(IConfiguration config, IMemoryCache memoryCache)
        {
            _config = config;
            _memoryCache = memoryCache;
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            string secret = null;

            if (_memoryCache.TryGetValue(secretName, out secret))
            {
                if (!string.IsNullOrWhiteSpace(secret))
                {
                    return secret;
                }
            }

            var keyVaultName = _config["KeyVaultName"];

            if (!string.IsNullOrWhiteSpace(keyVaultName))
            {
                // Instantiate a new KeyVaultClient object, with an access token to Key Vault
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

                string keyVaultUri = $"https://{keyVaultName}.vault.azure.net/";

                SecretBundle secretBundle = null;

                try
                {
                    secretBundle = await kv.GetSecretAsync(keyVaultUri, secretName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }                

                if (secretBundle != null && !string.IsNullOrWhiteSpace(secretBundle.Value))
                {
                    secret = secretBundle.Value;
                }
                else
                {
                    var result = _config[secretName];

                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        secret = result;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(secret))
            {
                _memoryCache.Set<string>(secretName, secret);
            }

            return secret;
        }
    }
}
