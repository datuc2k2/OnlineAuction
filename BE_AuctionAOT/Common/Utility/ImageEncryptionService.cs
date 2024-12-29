using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace BE_AuctionAOT.Common.Utility
{
	public class ImageEncryptionService
	{
		private readonly KeyClient _keyClient;
		private readonly IConfiguration _configuration;
		private readonly SecretClient _secretClient;

		public ImageEncryptionService(IConfiguration configuration, SecretClient secretClient)
		{
			_configuration = configuration;

			var credential = new ClientSecretCredential(
				_configuration["AzureKeyVault:TenantId"],
				_configuration["AzureKeyVault:ClientId"],
				_configuration["AzureKeyVault:ClientSecret"]
			);

			_keyClient = new KeyClient(
				new Uri(_configuration["AzureKeyVault:VaultUrl"]),
				credential
			);
			_secretClient = secretClient;
		}

		private async Task<byte[]> GetOrCreateAesKeyForUser(string userId)
		{
			var secretName = $"aes-key-{userId}";
			string aesKey;

			try
			{
				// Truy xuất khóa AES đã tồn tại từ Key Vault
				KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);
				aesKey = secret.Value;
			}
			catch (RequestFailedException ex) when (ex.Status == 404)
			{
				Console.WriteLine($"Secret not found: {ex.Message}");
				// Tạo khóa AES mới
				aesKey = GenerateAesKey(); // Hàm này sẽ tạo ra một khóa AES mới (chuỗi Base64)
										   // Lưu trữ khóa AES mới dưới dạng bí mật trong Key Vault
				await _secretClient.SetSecretAsync(secretName, aesKey);
			}
			catch (RequestFailedException getEx)
			{
				Console.WriteLine($"GetSecretAsync failed: {getEx.Message}");
				throw; // Xử lý lỗi nếu cần
			}

			// Chuyển đổi chuỗi Base64 thành mảng byte
			return Convert.FromBase64String(aesKey);
		}

		// Hàm mã hóa hình ảnh
		public async Task<byte[]> EncryptImageAsync(byte[] imageData, string userId)
		{
			var aesKey = await GetOrCreateAesKeyForUser(userId);

			// Tạo IV ngẫu nhiên
			using var aes = Aes.Create();
			aes.Key = aesKey;
			aes.GenerateIV(); // Tạo Initialization Vector (IV)

			using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
			using var memoryStream = new MemoryStream();
			// Ghi IV vào đầu dữ liệu
			memoryStream.Write(aes.IV, 0, aes.IV.Length);

			using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
			{
				await cryptoStream.WriteAsync(imageData, 0, imageData.Length);
				await cryptoStream.FlushFinalBlockAsync();
			}

			return memoryStream.ToArray(); // Trả về dữ liệu đã mã hóa
		}

		// Hàm giải mã hình ảnh
		public async Task<byte[]> DecryptImageAsync(byte[] encryptedData, string userId)
		{
			var aesKey = await GetOrCreateAesKeyForUser(userId);
			using var memoryStream = new MemoryStream(encryptedData);

			// Đọc IV từ dữ liệu đã mã hóa
			byte[] iv = new byte[16]; // AES block size = 16 bytes
			memoryStream.Read(iv, 0, iv.Length);

			using var aes = Aes.Create();
			aes.Key = aesKey;
			aes.IV = iv;

			using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
			using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
			using var decryptedStream = new MemoryStream();

			await cryptoStream.CopyToAsync(decryptedStream);
			return decryptedStream.ToArray(); // Trả về dữ liệu đã giải mã
		}

		// Hàm tạo khóa AES ngẫu nhiên
		private string GenerateAesKey()
		{
			using (var aes = Aes.Create())
			{
				aes.KeySize = 256; // Sử dụng AES-256
				aes.GenerateKey(); // Tạo khóa ngẫu nhiên
				return Convert.ToBase64String(aes.Key); // Trả về khóa dưới dạng chuỗi Base64
			}
		}



		// Lấy hoặc tạo khóa AES từ Key Vault
		//private async Task<KeyVaultKey> GetOrCreateAesKeyForUser(string userId)
		//{
		//	var keyName = $"aes-key-{userId}";
		//	KeyVaultKey key;

		//	try
		//	{
		//		key = await _keyClient.GetKeyAsync(keyName);
		//	}
		//	catch (RequestFailedException ex)
		//	{
		//		Console.WriteLine($"GetKeyAsync failed: {ex.Message}");

		//		// Create key options
		//		var keyOptions = new CreateOctKeyOptions(keyName)
		//		{
		//			KeySize = 256,
		//			ExpiresOn = DateTimeOffset.UtcNow.AddYears(1),
		//			Enabled = true
		//		};

		//		try
		//		{
		//			key = await _keyClient.CreateOctKeyAsync(keyOptions);
		//		}
		//		catch (RequestFailedException createEx)
		//		{
		//			Console.WriteLine($"CreateOctKeyAsync failed: {createEx.Message}, Status: {createEx.Status}, ErrorCode: {createEx.ErrorCode}");
		//			throw; // Rethrow or handle as necessary
		//		}
		//	}
		//	return key;
		//}


		// Mã hóa hình ảnh
		//public async Task<byte[]> EncryptImageAsync(byte[] imageData, string userId)
		//{
		//	var key = await GetOrCreateAesKeyForUser(userId);

		//	var cryptoClient = new CryptographyClient(key.Id, new ClientSecretCredential(
		//		_configuration["AzureKeyVault:TenantId"],
		//		_configuration["AzureKeyVault:ClientId"],
		//		_configuration["AzureKeyVault:ClientSecret"]
		//	));

		//	var encryptResult = await cryptoClient.EncryptAsync(EncryptionAlgorithm.A256Gcm, imageData);
		//	return encryptResult.Ciphertext;
		//}

		// Giải mã hình ảnh
		//public async Task<byte[]> DecryptImageAsync(byte[] encryptedData, string userId)
		//{
		//	var key = await GetOrCreateAesKeyForUser(userId);

		//	var cryptoClient = new CryptographyClient(key.Id, new ClientSecretCredential(
		//		_configuration["AzureKeyVault:TenantId"],
		//		_configuration["AzureKeyVault:ClientId"],
		//		_configuration["AzureKeyVault:ClientSecret"]
		//	));

		//	var decryptResult = await cryptoClient.DecryptAsync(EncryptionAlgorithm.A256Gcm, encryptedData);
		//	return decryptResult.Plaintext;
		//}
	}
}
