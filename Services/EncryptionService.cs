using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ComputerStore.Services
{
    public interface IEncryptionService
    {
        string EncryptCredentials(object credentials);
        T? DecryptCredentials<T>(string encryptedData) where T : class;
        bool IsConfigurationValid(string encryptedData);
    }

    public class EncryptionService : IEncryptionService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EncryptionService> _logger;
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptionService(IConfiguration configuration, ILogger<EncryptionService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            // Usar una clave basada en el connection string para que sea única por instalación
            var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "default";
            var keySource = $"ComputerStore_Marketing_{connectionString}";
            
            using var sha256 = SHA256.Create();
            _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(keySource))[..16]; // AES-128
            _iv = sha256.ComputeHash(Encoding.UTF8.GetBytes($"{keySource}_IV"))[..16];
        }

        public string EncryptCredentials(object credentials)
        {
            try
            {
                var json = JsonSerializer.Serialize(credentials);
                var plainTextBytes = Encoding.UTF8.GetBytes(json);
                
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;
                
                using var encryptor = aes.CreateEncryptor();
                var encryptedBytes = encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);
                
                var result = Convert.ToBase64String(encryptedBytes);
                _logger.LogDebug("? Credenciales encriptadas exitosamente");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "?? Error encriptando credenciales");
                throw new InvalidOperationException("Error encriptando credenciales de marketing", ex);
            }
        }

        public T? DecryptCredentials<T>(string encryptedData) where T : class
        {
            try
            {
                var encryptedBytes = Convert.FromBase64String(encryptedData);
                
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;
                
                using var decryptor = aes.CreateDecryptor();
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                
                var json = Encoding.UTF8.GetString(decryptedBytes);
                var result = JsonSerializer.Deserialize<T>(json);
                
                _logger.LogDebug("? Credenciales desencriptadas exitosamente");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "?? Error desencriptando credenciales");
                return null;
            }
        }

        public bool IsConfigurationValid(string encryptedData)
        {
            try
            {
                var decrypted = DecryptCredentials<Dictionary<string, object>>(encryptedData);
                return decrypted != null && decrypted.Count > 0;
            }
            catch
            {
                return false;
            }
        }
    }

    // Clases para deserializar credenciales - MOVIDAS PARA EVITAR DUPLICADOS
    public class GoogleAdsCredentialsData
    {
        public string AccountId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }

    public class FacebookAdsCredentialsData
    {
        public string AppId { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public string AppSecret { get; set; } = string.Empty;
    }

    public class EmailMarketingCredentialsData
    {
        public string Platform { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public Dictionary<string, string> Extra { get; set; } = new();
    }
}