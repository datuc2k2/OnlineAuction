using AutoMapper;
using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.DAO.Common.User;
using BE_AuctionAOT.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;

namespace BE_AuctionAOT.DAO.AccountManagement.Account
{
	public class AccountDao
	{
		private readonly DB_AuctionAOTContext _context;
		private readonly IMapper _mapper;
		public AccountDao(DB_AuctionAOTContext context, IMapper mapper)
		{
			_mapper = mapper;
			_context = context;
		}
		/// <summary>
		/// Use to get account list in screen list and search
		/// </summary>
		/// <param name="Input"> Input has page current and search input, just username</param>
		/// <returns>List pageCount account was search or list all, all record was search</returns>
		public async Task<AccountDaoOutputDto?> GetAccountList(AccountDaoInputDto Input)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<AccountDaoOutputDto>();
				var query = from user in _context.Users
							join userProfile in _context.UserProfiles on user.UserId equals userProfile.UserId
							where
								(string.IsNullOrEmpty(Input.searchUserName) || user.Username.Contains(Input.searchUserName)) &&
								(string.IsNullOrEmpty(Input.searchEmail) || userProfile.Email.Contains(Input.searchEmail)) &&
								(!Input.startDate.HasValue || user.CreatedAt >= Input.startDate.Value.ToDateTime(TimeOnly.MinValue)) &&
								(!Input.endDate.HasValue || user.CreatedAt <= Input.endDate.Value.ToDateTime(TimeOnly.MaxValue)) &&
								(Input.status == null || user.IsActive == Input.status)
							orderby user.CreatedAt descending
							select new UserFromDto
							{
								UserId = user.UserId,
								Username = user.Username,
								IsActive = user.IsActive,
								CreatedAt = user.CreatedAt,
								UpdatedAt = user.UpdatedAt,
								UserProfile = userProfile == null ? null : new UserProfileDto
								{
									FullName = userProfile.FullName,
									Email = userProfile.Email,
									PhoneNumber = userProfile.PhoneNumber,
									Address = userProfile.Address,
									Avatar = userProfile.Avatar,
									Cccd = userProfile.Cccd,
									Dob = userProfile.Dob,
									CreatedAt = userProfile.CreatedAt,
									UpdatedAt = userProfile.UpdatedAt
								}
							};
				int totalRecords = await query.CountAsync();
				var users = await query.ToListAsync();
				var paginatedUsers = await query
					.Skip((int)((Input.DisplayCount.DisplayCount - 1) * (int)Input.DisplayCount.PageCount))
															.Take((int)Input.DisplayCount.PageCount).ToListAsync();
				output.Users = paginatedUsers;
				output.allRecords = totalRecords;
				return output;
			}
			catch (Exception ex)
			{
				string[] parameters = { "error" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co loi ", parameters).WithException(ex).Create<AccountDaoOutputDto>();
			}
		}

		public async Task<AccountInviteDaoOutputDto?> GetAccountInvite()
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<AccountInviteDaoOutputDto>();

				var users = await _context.Users
					.Select(user => new
					{
						user.UserId,
						user.Username,
						Email = user.UserProfile.Email
					})
					.ToListAsync();

				List<AccountInvite> invites = users.Select(user => new AccountInvite
				{
					UserId = user.UserId,
					Username = user.Username,
					Email = user.Email
				}).ToList();

				output.AccountInvites = invites;
				return output;
			}
			catch (Exception ex)
			{
				string[] parameters = { "error" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co loi ", parameters).WithException(ex).Create<AccountInviteDaoOutputDto>();
			}
		}

		public async Task<OtherUserInforOutputDto> GetOtherUserInfor(int userId)
		{
			try
			{
				var users = from user in _context.Users
						   join userProfile in _context.UserProfiles on user.UserId equals userProfile.UserId
						   where
							   (user.UserId == userId)
						   select new OtherUserInforOutputDto
						   {
							   Username = user.Username,
							   Avatar = user.UserProfile.Avatar,
							   CreatedAt = user.CreatedAt,
							   LastLogin = user.LastLogin,
							   IsEkyc = user.IsEkyc,
							   IsActive = user.IsActive
						   };
				var otherUserInfor = await users.FirstOrDefaultAsync();

				return otherUserInfor;
			}
			catch (Exception ex)
			{
				return null;
			}
		}

		public AccountGetForAuction GetAccountByUserId(int userId)
		{
			try
			{
				var users = _context.Users
					.Select(user => new
					{
						user.UserId,
						user.Username,
						Email = user.UserProfile.Email,
						Avatar = user.UserProfile.Avatar,
					})
					.ToList();

				AccountGetForAuction accountById = users.Where(u => u.UserId == userId).Select(user => new AccountGetForAuction
				{
					UserId = user.UserId,
					Username = user.Username,
					Email = user.Email,
					Avatar = user.Avatar,
				}).FirstOrDefault();

				return accountById;
			}
			catch (Exception ex)
			{
				return null;
			}
		}

	}
}
