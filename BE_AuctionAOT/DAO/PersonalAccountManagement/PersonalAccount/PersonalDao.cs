using AutoMapper;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Common.Utility;
using BE_AuctionAOT.DAO.AccountManagement.Account_Details;
using BE_AuctionAOT.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace BE_AuctionAOT.Controllers.Personal_Account_Management
{
	public class PersonalDao
	{
		private readonly DB_AuctionAOTContext _context;
		private readonly ImageEncryptionService _imageEncryptionService;
		private readonly BlobServiceClient _blobServiceClient;
		private readonly IMapper _mapper;
		public PersonalDao(DB_AuctionAOTContext context, IMapper mapper, ImageEncryptionService imageEncryptionService, BlobServiceClient blobServiceClient)
		{
			_mapper = mapper;
			_context = context;
			_imageEncryptionService = imageEncryptionService;
			_blobServiceClient = blobServiceClient;
		}
		public async Task<PersonalDaoOutputDto?> GetProfile(String Input)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<PersonalDaoOutputDto>();
				User user = await _context.Users.Include(o => o.UserProfile).FirstOrDefaultAsync(o => o.UserId == int.Parse(Input));
				if (user == null)
				{
					return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "User not found").Create<PersonalDaoOutputDto>();
				}
				var UserDto = _mapper.Map<User, UserDto>(user);
				if (user.UserProfile.FrontIdCard != null)
				{
					UserDto.UserProfile.FrontIdCard = await DecryptFile(user.UserProfile.FrontIdCard, Input);
				}
				if (user.UserProfile.BackIdCard != null)
				{
					UserDto.UserProfile.BackIdCard = await DecryptFile(user.UserProfile.BackIdCard, Input);
				}
				//return them cho dai
				var userRole = await _context.UserRoles.Include(o => o.Role).FirstOrDefaultAsync(o => o.UserId == int.Parse(Input));

				output.User = UserDto;
				output.User.UserRoleId = userRole.RoleId.ToString();
				output.User.UserRoleName = userRole.Role.RoleName;
				var point = await _context.Points.FirstOrDefaultAsync(o => o.UserId == int.Parse(Input));
				if (point == null)
				{
					Models.Point newPoint = new Models.Point();
					newPoint.UserId = int.Parse(Input);
					newPoint.PointsAmount = 0;
					newPoint.Currency = "VND";
					newPoint.LastUpdated = DateTime.Now;
					_context.Points.Add(newPoint);
					await _context.SaveChangesAsync();
					output.User.Point = newPoint.PointsAmount;
					output.User.Currency = newPoint.Currency;
				}
				else
				{
					output.User.Point = point.PointsAmount;
					output.User.Currency = point.Currency;
				}

				return output;
			}
			catch (Exception ex)
			{
				string[] parameters = { "error" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "1+1={0}", parameters).WithException(ex).Create<PersonalDaoOutputDto>();
			}
		}
		public async Task<GetFrontIdCardOutputDto?> GetFrontIdCard(String Input)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<GetFrontIdCardOutputDto>();
				User user = await _context.Users.Include(o => o.UserProfile).FirstOrDefaultAsync(o => o.UserId == int.Parse(Input));
				if (user == null)
				{
					return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "User not found").Create<GetFrontIdCardOutputDto>();
				}
				if (user.UserProfile.FrontIdCard == null)
				{
					return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Users need to update personal information to perform user authentication.").Create<GetFrontIdCardOutputDto>();
				}
				output.FrontIdCard = await DecryptFile(user.UserProfile.FrontIdCard, Input);
				return output;
			}
			catch (Exception ex)
			{
				string[] parameters = { "error" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "1+1={0}", parameters).WithException(ex).Create<GetFrontIdCardOutputDto>();
			}
		}
		public async Task<bool> IsEkyc(long uId)
		{
			try {
				Models.User user = await _context.Users.FirstOrDefaultAsync(o => o.UserId == uId);
				if (user == null) {
					return false;
				}
				user.IsEkyc = true;
				_context.Users.Update(user);
				await _context.SaveChangesAsync();
				return true;
			} catch (Exception ex) {
				return false;
			}
		}

		public async Task<PersonalDaoOutputDto?> EditProfile(String accountId, PersonalDaoInputDto inputDto)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<PersonalDaoOutputDto>();

				User? user = await _context.Users.Include(o => o.UserProfile).FirstOrDefaultAsync(o => o.UserId == int.Parse(accountId));
				if (user == null)
				{
					return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "User not found").Create<PersonalDaoOutputDto>();
				}
				user.Username = inputDto.Username;
				user.UserProfile!.FullName = inputDto.FullName;
				user.UserProfile.Email = inputDto.Email;
				user.UserProfile.PhoneNumber = inputDto.PhoneNumber;
				user.UserProfile.Address = inputDto.Address;
				if (inputDto.Avatar != null && inputDto.Avatar.Length > 0)
				{
					string containerName = "avataimg";
					BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
					var blobName = Guid.NewGuid().ToString() + Path.GetExtension(inputDto.Avatar.FileName);

					BlobClient blobClient = containerClient.GetBlobClient(blobName);

					var blobHttpHeaders = new BlobHttpHeaders
					{
						ContentType = inputDto.Avatar.ContentType
					};

					using (var stream = inputDto.Avatar.OpenReadStream())
					{
						await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
					}
					string fileUrl = blobClient.Uri.ToString();

					user.UserProfile.Avatar = fileUrl;
				}

				user.UserProfile.Cccd = inputDto.Cccd;
				if (inputDto.FrontIdCard != null)
				{
					user.UserProfile.FrontIdCard = await EncryptFile(inputDto.FrontIdCard, accountId);
				}
				if (inputDto.BackIdCard != null)
				{
					user.UserProfile.BackIdCard = await EncryptFile(inputDto.BackIdCard, accountId);
				}
				user.UserProfile.Dob = inputDto.Dob;
				_context.Users.Update(user);
				await _context.SaveChangesAsync();


				User userNew = await _context.Users.Include(o => o.UserProfile).FirstOrDefaultAsync(o => o.UserId == int.Parse(accountId));
				var UserDto = _mapper.Map<User, UserDto>(userNew);
				output.User = UserDto;
				return output;
			}
			catch (Exception ex)
			{
				string[] parameters = { "error" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "1+1={0}", parameters).WithException(ex).Create<PersonalDaoOutputDto>();
			}
		}
		// Phương thức mã hóa file trả về base64
		private async Task<string> EncryptFile(IFormFile file, string userId)
		{
			using var ms = new MemoryStream();
			await file.CopyToAsync(ms);
			var imageData = ms.ToArray();

			// Mã hóa hình ảnh và lấy dữ liệu byte
			byte[] encryptedData = await _imageEncryptionService.EncryptImageAsync(imageData, userId);

			// Chuyển đổi dữ liệu byte sang chuỗi Base64
			return Convert.ToBase64String(encryptedData);
		}
		//phương thức giải mã trả về base64
		private async Task<string> DecryptFile(string base64String, string userId)
		{
			// Chuyển đổi chuỗi Base64 về dữ liệu byte
			byte[] encryptedData = Convert.FromBase64String(base64String);

			// Giải mã dữ liệu
			byte[] decryptedData = await _imageEncryptionService.DecryptImageAsync(encryptedData, userId);

			// Chuyển đổi dữ liệu byte đã giải mã sang chuỗi Base64
			return Convert.ToBase64String(decryptedData);
		}
		public async Task<string> ConvertImageToBase64(IFormFile imageFile)
		{
			if (imageFile == null || imageFile.Length == 0)
			{
				throw new ArgumentException("Hình ảnh không hợp lệ.");
			}

			using (var memoryStream = new MemoryStream())
			{
				await imageFile.CopyToAsync(memoryStream);
				byte[] imageData = memoryStream.ToArray();
				return Convert.ToBase64String(imageData);
			}
		}




	}
}
