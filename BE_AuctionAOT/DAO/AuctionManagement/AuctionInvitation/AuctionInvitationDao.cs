using AutoMapper;
using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Controllers.AuctionManagement.Auction;
using BE_AuctionAOT.DAO.AuctionManagement.Payment;
using BE_AuctionAOT.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace BE_AuctionAOT.DAO.AuctionManagement.AuctionInvitation
{
	public class AuctionInvitationDao
	{
		private readonly DB_AuctionAOTContext _context;
		private readonly IMapper _mapper;
		public AuctionInvitationDao(DB_AuctionAOTContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		public CreateInvitationOutputDto CreateInvitation(CreateInvitationInputDto inputDto)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<CreateInvitationOutputDto>();
				var aucs = new List<Models.AuctionInvitation>();
				foreach (var item in inputDto.InvitedIds)
				{
					aucs.Add(new Models.AuctionInvitation
					{
						AuctionId = inputDto.AuctionId,
						InvitedUserId = item
					});
				}
				_context.AuctionInvitations.AddRange(aucs);
				_context.SaveChanges();
				return output;
			}
			catch (Exception ex)
			{
				return this.Output(ResultCd.FAILURE).WithException(ex).Create<CreateInvitationOutputDto>();
			}
		}

		public BaseOutputDto DeleteInvitation(CreateInvitationInputDto inputDto)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();
				var aucs = new List<Models.AuctionInvitation>();
				foreach (var item in inputDto.InvitedIds)
				{
					aucs.Add(new Models.AuctionInvitation
					{
						AuctionId = inputDto.AuctionId,
						InvitedUserId = item
					});
				}
				_context.AuctionInvitations.RemoveRange(aucs);
				_context.SaveChanges();
				return output;
			}
			catch (Exception ex)
			{
				return this.Output(ResultCd.FAILURE).WithException(ex).Create<BaseOutputDto>();
			}
		}

		public GetCurInvitationOutputDto GetCurInvitation(long auctionID)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<GetCurInvitationOutputDto>();
				var curaucs = _context.AuctionInvitations.Where(x => x.AuctionId == auctionID).Select(x => x.InvitationId).ToList();
				output.Invs = curaucs;
				return output;
			}
			catch (Exception ex)
			{
				return this.Output(ResultCd.FAILURE).WithException(ex).Create<GetCurInvitationOutputDto>();
			}
		}

		/// <summary>
		/// Hàm lấy lời mời tham gia đấu giá của người dùng
		/// </summary>
		/// <param name="UserId">Truyền userId của người dùng</param>
		/// <param name="inputDto">Truyền thông tin List, search text</param>
		/// <returns>List những lời mời tham gia đấu giá, kèm theo thông tin auction được mời</returns>
		public async Task<AuctionInvitationOutputDto> getListAuctionInvitation(long UserId, AuctionInvitationInputDto inputDto)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<AuctionInvitationOutputDto>();
				List<Models.AuctionInvitation> listAuctionInvatation = await _context.AuctionInvitations.Include(o => o.Auction)
																					.Where(o => (o.InvitedUserId == UserId) &&
																					(string.IsNullOrEmpty(inputDto.searchName) || o.Auction.ProductName.ToLower().Contains(inputDto.searchName.ToLower())) &&
																					(!inputDto.startDate.HasValue || o.InvitedAt >= inputDto.startDate.Value.ToDateTime(TimeOnly.MinValue)) &&
																					(!inputDto.endDate.HasValue || o.InvitedAt <= inputDto.endDate.Value.ToDateTime(TimeOnly.MaxValue)))
																					.OrderByDescending(n => n.InvitedAt).ToListAsync();
				if (listAuctionInvatation == null)
				{
					return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Không có lời mời tham gia đấu giá riêng tư nào").Create<AuctionInvitationOutputDto>();
				}
				var totalRecords = listAuctionInvatation.Count();
				//phan trang
				listAuctionInvatation = listAuctionInvatation.Skip((int)((inputDto.DisplayCount.DisplayCount - 1) * (int)inputDto.DisplayCount.PageCount))
					.Take((int)inputDto.DisplayCount.PageCount).ToList();


				var listInvitationDto = _mapper.Map<List<Models.AuctionInvitation>, List<AuctionInvitationDto>>(listAuctionInvatation);
				output.auctionInvitations = listInvitationDto;
				output.allRecords = totalRecords;
				return output;
			}
			catch (Exception ex)
			{
				string[] parameters = { "Loi" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co {0}", parameters).WithException(ex).Create<AuctionInvitationOutputDto>();
			}
		}

		public async Task<BaseOutputDto> isAccepttAuctionInvitation(long UserId, IsAcceptedInvitation inputDto)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();
				//update accept cho lời mời
				Models.AuctionInvitation auctionInvitation = await _context.AuctionInvitations
																			.FirstOrDefaultAsync(o => o.InvitedUserId == UserId
																									&& o.AuctionId == long.Parse(inputDto.auctionId));
				if (auctionInvitation == null)
				{
					return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Lỗi trong quá trình accept").Create<AuctionInvitationOutputDto>();
				}
				if (inputDto.isAccept)
				{
					auctionInvitation.IsAccepted = true;
					auctionInvitation.AcceptedAt = DateTime.Now;
					_context.AuctionInvitations.Update(auctionInvitation);
					await _context.SaveChangesAsync();
				}
				else
				{
					auctionInvitation.IsAccepted = false;
					auctionInvitation.AcceptedAt = DateTime.Now;
					_context.AuctionInvitations.Update(auctionInvitation);
					await _context.SaveChangesAsync();
				}

				return output;
			}
			catch (Exception ex)
			{
				string[] parameters = { "Loi" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co {0}", parameters).WithException(ex).Create<BaseOutputDto>();
			}
		}

	}
}
