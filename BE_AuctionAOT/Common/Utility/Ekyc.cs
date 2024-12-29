using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BE_AuctionAOT.Common.Utility
{
	public class Ekyc
	{
		private readonly IConfiguration _configuration;

		public Ekyc(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task<string> GetAccessTokenAsync()
		{
			HttpClient httpClient = new HttpClient();
			var requestBody = new Dictionary<string, string>
			  {
				  { "username", _configuration["UserCredentials:Username"] },
				  { "password", _configuration["UserCredentials:Password"] },
				  { "client_id", "clientapp" },
				  { "grant_type", "password" },
				  { "client_secret", "password" }
			  };

			var content = new FormUrlEncodedContent(requestBody);

			var response = await httpClient.PostAsync("https://api.idg.vnpt.vn/auth/oauth/token", content);
			if (!response.IsSuccessStatusCode)
			{
				throw new Exception($"API call failed with status code: {response.StatusCode}");
			}
			var responseString = await response.Content.ReadAsStringAsync();
			var jsonDoc = JsonDocument.Parse(responseString);
			var accessToken = jsonDoc.RootElement.GetProperty("access_token").GetString();
			return accessToken;


			throw new Exception("Failed to retrieve access token from the response.");
		}

		public async Task<string> UploadFileAsync(
													string accessToken,
													IFormFile fileUpload,
													string title,
													string description)
		{
			try
			{
				HttpClient httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
				httpClient.DefaultRequestHeaders.Add("Token-id", _configuration["eKyc:TokenId"]);
				httpClient.DefaultRequestHeaders.Add("Token-key", _configuration["eKyc:TokenKey"]);

				using (var form = new MultipartFormDataContent())
				{
					using (var fileStream = fileUpload.OpenReadStream())
					{
						using (var streamContent = new StreamContent(fileStream))
						{
							using (var fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync()))
							{
								fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

								// Thêm tệp tin vào form-data
								form.Add(fileContent, "file", fileUpload.FileName);

								// Thêm các trường khác vào form-data
								form.Add(new StringContent(title, Encoding.UTF8), "title");
								form.Add(new StringContent(description, Encoding.UTF8), "description");

								// Gửi request POST
								HttpResponseMessage response = await httpClient.PostAsync("https://api.idg.vnpt.vn/file-service/v1/addFile", form);

								if (!response.IsSuccessStatusCode)
								{
									throw new Exception($"API call failed with status code: {response.StatusCode}");
								}
								var responseBody = await response.Content.ReadAsStringAsync();
								var jsonDoc = JsonDocument.Parse(responseBody);
								var objectElement = jsonDoc.RootElement.GetProperty("object");
								String hash_code = null;

								// Lấy trường "hash" từ đối tượng "object"
								if (objectElement.TryGetProperty("hash", out var hashProperty))
								{
									hash_code = hashProperty.GetString();
								}
								return hash_code;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error calling API: {ex.Message}");
			}

		}
		public class MockIFormFile : IFormFile
		{
			private readonly MemoryStream _memoryStream;
			public MockIFormFile(byte[] fileBytes, string fileName)
			{
				_memoryStream = new MemoryStream(fileBytes);
				FileName = fileName;
			}

			public string ContentType { get; set; } = "image/jpeg";
			public string FileName { get; set; }
			public long Length => _memoryStream.Length;

			public string ContentDisposition => throw new NotImplementedException();

			public IHeaderDictionary Headers => throw new NotImplementedException();

			public string Name => throw new NotImplementedException();

			public void CopyTo(Stream target)
			{
				_memoryStream.CopyTo(target);
			}

			public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
			{
				return _memoryStream.CopyToAsync(target, cancellationToken);
			}

			public Stream OpenReadStream()
			{
				return _memoryStream;
			}
		}
		public async Task<string> UploadBase64ImageAsync(string accessToken, string base64Image, string title, string description, long uid)
		{
			// Chuyển Base64 thành byte[]
			byte[] imageBytes = Convert.FromBase64String(base64Image);

			// Tạo Mock IFormFile từ byte[] và tên file
			IFormFile mockFile = new MockIFormFile(imageBytes, $"front_cccd_{uid}.jpg");

			return await UploadFileAsync(accessToken, mockFile, title, description);
		}


		public async Task<CompareFaceResult> CompareFaceAsync(string accessToken, string imgFrontHash, string imgFaceHash, string clientSession)
		{
			HttpClient httpClient = new HttpClient();
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			httpClient.DefaultRequestHeaders.Add("Token-id", _configuration["eKyc:TokenId"]);
			httpClient.DefaultRequestHeaders.Add("Token-key", _configuration["eKyc:TokenKey"]);
			httpClient.DefaultRequestHeaders.Add("mac-address", "TEST1");

			var body = new
			{
				img_front = imgFrontHash,
				img_face = imgFaceHash,
				client_session = clientSession,
				token = _configuration["eKyc:Token"]
			};

			var jsonBody = JsonSerializer.Serialize(body);
			var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

			try
			{
				var response = await httpClient.PostAsync("https://api.idg.vnpt.vn/ai/v1/face/compare", content);

				if (!response.IsSuccessStatusCode)
				{
					throw new Exception($"API request failed with status code: {response.StatusCode}");
				}

				var responseBody = await response.Content.ReadAsStringAsync();

				var responseObj = JsonSerializer.Deserialize<CompareFaceResponse>(responseBody);

				if (responseObj != null && responseObj.@object != null)
				{
					// Lấy kết quả từ trường "object" trong response
					var result = responseObj.@object.result;
					var msg = responseObj.@object.msg;
					var prob = responseObj.@object.prob;

					// Trả về kết quả dưới dạng chuỗi
					return new CompareFaceResult
					{
						Result = result,
						Msg = msg,
						Prob = prob
					};
				}

				throw new Exception("Failed to retrieve the comparison results.");
			}
			catch (Exception ex)
			{
				throw new Exception($"Error calling API: {ex.Message}");
			}
		}

	}
}
