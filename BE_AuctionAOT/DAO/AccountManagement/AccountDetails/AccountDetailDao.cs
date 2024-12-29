using AutoMapper;
using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Common.Utility;
using BE_AuctionAOT.DAO.AccountManagement.Account;
using BE_AuctionAOT.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_AuctionAOT.DAO.AccountManagement.Account_Details
{
	public class AccountDetailDao
	{
		private readonly DB_AuctionAOTContext _context;
		private readonly IMapper _mapper;
		private readonly ImageEncryptionService _imageEncryptionService;
		public AccountDetailDao(DB_AuctionAOTContext context, IMapper mapper, ImageEncryptionService imageEncryptionService)
		{
			_mapper = mapper;
			_context = context;
			_imageEncryptionService = imageEncryptionService;
		}
		/// <summary>
		/// dùng để lấy chi tiết thông tin của account
		/// </summary>
		/// <param name="Input">truyền vào accountId</param>
		/// <returns>trả về một model user</returns>
		public async Task<AccountDetailDaoOutputDto?> GetAccount(AccountDetailDaoInputDto Input)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<AccountDetailDaoOutputDto>();
				User user = await _context.Users.Include(o => o.UserProfile).Include(o=>o.UserRoles).FirstOrDefaultAsync(o => o.UserId == long.Parse(Input.accountId));
				if (user == null)
				{
					return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "User not found").Create<AccountDetailDaoOutputDto>();
					return output;
				}
				// giải mã ảnh cccd
				if (user.UserProfile.FrontIdCard != null)
				{
					user.UserProfile.FrontIdCard = await DecryptFile(user.UserProfile.FrontIdCard, Input.accountId);
				}
				if (user.UserProfile.BackIdCard != null)
				{
					user.UserProfile.BackIdCard = await DecryptFile(user.UserProfile.BackIdCard, Input.accountId);
				}

				output.Account = user;
				return output;
			}
			catch (Exception ex)
			{
				string[] parameters = { "error" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "1+1={0}", parameters).WithException(ex).Create<AccountDetailDaoOutputDto>();
			}
		}

		/// <summary>
		/// dùng để set active và role
		/// </summary>
		/// <param name="Input">truyền vào accountId, role và isActive</param>
		/// <returns>User đã đc edit</returns>
		public async Task<AccountDetailDaoOutputDto?> EditAccountdetail(AccountDetailDaoInputDto Input)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<AccountDetailDaoOutputDto>();
				if (!long.TryParse(Input.accountId, out long accountId))
				{
					return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Invalid account ID format").Create<AccountDetailDaoOutputDto>();
					return output;
				}

				var user = await _context.Users.Include(o => o.UserProfile).Include(o => o.UserRoles).FirstOrDefaultAsync(o => o.UserId == accountId);
				if (user == null)
				{
					return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "User not found").Create<AccountDetailDaoOutputDto>();
					return output;
				}

				// 2. xóa vai trò cũ
				var userRole = await _context.UserRoles.FirstOrDefaultAsync(o => o.UserId == accountId);

				if (userRole != null)
				{
					_context.UserRoles.Remove(userRole);
				}

				// 3. thêm vai trò mới
				var newUserRole = new UserRole
				{
					UserId = user.UserId,
					RoleId = Input.roleNew,
					AssignedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};
				await _context.UserRoles.AddAsync(newUserRole);

				//4. cap nhat isActive
				if ((bool)Input.IsActive)
				{
					user.IsActive = true;
				}
				else { user.IsActive = false; }
				await _context.SaveChangesAsync();
				
				//giải mã ảnh cccd
				if (user.UserProfile.FrontIdCard != null)
				{
					user.UserProfile.FrontIdCard = await DecryptFile(user.UserProfile.FrontIdCard, Input.accountId);
				}
				if (user.UserProfile.BackIdCard != null)
				{
					user.UserProfile.BackIdCard = await DecryptFile(user.UserProfile.BackIdCard, Input.accountId);
				}
				output.Account = user;
				return output;
			}
			catch (DbUpdateException dbEx)
			{
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Database update error: {0}", new[] { dbEx.Message }).Create<AccountDetailDaoOutputDto>();
			}
			catch (Exception ex)
			{
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "An error occurred: {0}", new[] { ex.Message }).Create<AccountDetailDaoOutputDto>();
			}
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


	}
}
