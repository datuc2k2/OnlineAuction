using AutoMapper;
using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BE_AuctionAOT.DAO.AuctionManagement.AuctionRequest
{
	public class AuctionRequestDao
	{
		private readonly DB_AuctionAOTContext _context;
		private readonly IMapper _mapper;
		public AuctionRequestDao(DB_AuctionAOTContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		public BaseOutputDto CreateAuctionRequest(CreateAuctionRequestInputDto inputDto)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();

				var request = new Models.AuctionRequest()
				{
					AuctionId = inputDto.AuctionId,
					UserId = inputDto.UserId,
					RequestDetails = inputDto.RequestDetails,
					Type = inputDto.Type,
					IsApproved = null
				};

				_context.Add(request);
				_context.SaveChanges();
				return output;
			}
			catch (Exception ex)
			{
				return this.Output(ResultCd.FAILURE).WithException(ex).Create<BaseOutputDto>();
			}
		}

		public async Task<AuctionRequestOutputDto> GetListAuctionRequest(AuctionRequestInputDto inputDto)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<AuctionRequestOutputDto>();
				var baseQuery = from ar in _context.AuctionRequests
								join u in _context.Users on ar.UserId equals u.UserId
								join up in _context.UserProfiles on u.UserId equals up.UserId
								join a in _context.Auctions on ar.AuctionId equals a.AuctionId
								where
									(string.IsNullOrEmpty(inputDto.searchName) || a.ProductName.ToLower().Contains(inputDto.searchName.ToLower())) &&
									(inputDto.isProcessed == null ||
										(inputDto.isProcessed == true && ar.IsApproved != null) ||
										(inputDto.isProcessed == false && ar.IsApproved == null)) &&
									(!inputDto.startDate.HasValue || ar.CreatedAt >= inputDto.startDate.Value.ToDateTime(TimeOnly.MinValue)) &&
									(!inputDto.endDate.HasValue || ar.CreatedAt <= inputDto.endDate.Value.ToDateTime(TimeOnly.MaxValue))
								select new AuctionRequestFromDto
								{
									RequestId = ar.RequestId,
									AuctionId = ar.AuctionId,
									UserId = ar.UserId,
									Type = ar.Type,
									RequestDetails = ar.RequestDetails,
									IsApproved = ar.IsApproved,
									CreatedAt = ar.CreatedAt,
									ApprovedAt = ar.ApprovedAt,
									ApprovedBy = ar.ApprovedBy,
									User = new UserDto
									{
										Username = u.Username,
										UserProfile = new UserProfileDto
										{
											Avatar = up.Avatar
										}
									},
									Auction = new AuctionDto
									{
										ProductName = a.ProductName,
										CategoryId = a.CategoryId,
										CategoryName = _context.Categories.Where(c => c.CategoryId == a.CategoryId).Select(c => c.CategoryName).FirstOrDefault(),
										StartingPrice = a.StartingPrice,
										Currency = a.Currency,
										StepPrice = a.StepPrice,
										Mode = a.Mode,
										ModeName = _context.Categories.Where(x => x.CategoryId == a.Mode).Select(x => x.CategoryName).FirstOrDefault(),
										Description = a.Description,
										IsPrivate = a.IsPrivate,
										DepositAmount = a.DepositAmount,
										DepositDeadline = a.DepositDeadline,
										StartTime = a.StartTime,
										EndTime = a.EndTime,
										IsActive = a.IsActive,
										CreatedAt = a.CreatedAt,
										UpdatedAt = a.UpdatedAt,
										Status = a.Status,
										PaymentStatus = a.PaymentStatus,
										AuctionImages = _context.AuctionImages
											.Where(x => x.AuctionId == a.AuctionId)
											.GroupBy(x => x.AuctionId)
											.Select(g => g.Select(img => new AuctionImageDto
											{
												ImageUrl = img.ImageUrl
											}).ToList()).FirstOrDefault()
									}
								};
				var totalRecords = await baseQuery.CountAsync();

				var pageIndex = inputDto.DisplayCount.DisplayCount - 1;
				var pageSize = inputDto.DisplayCount.PageCount;

				var AucRqlist = await baseQuery
								.OrderByDescending(x => x.CreatedAt)
								.Skip((int)(pageIndex * pageSize))
								.Take((int)pageSize)
								.ToListAsync();

				output.AuctionRequests = AucRqlist;
				output.allRecords = totalRecords;
				return output;
			}
			catch (Exception ex)
			{
				return this.Output(ResultCd.FAILURE).WithException(ex).Create<AuctionRequestOutputDto>();
			}
		}
		public async Task<BaseOutputDto> IsAcceptedRequest(IsAcceptedAucRequestInputDto inputDto)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();

				var request = await _context.AuctionRequests.FirstOrDefaultAsync(x => x.AuctionId == inputDto.AuctionId);
				var auction = await _context.Auctions.FirstOrDefaultAsync(x => x.AuctionId == inputDto.AuctionId);
				if (request == null)
				{
					output.ResultCd = 0; return output;
				}
				request.IsApproved = inputDto.IsAccepted;
				request.ApprovedAt = DateTime.Now;
				if (!inputDto.IsAccepted) { request.RequestDetails = inputDto.Reason; }
				if (inputDto.IsAccepted)
				{
					auction.Status = AuctionConst.Category.ACCEPTED;
				}
				else
				{
					auction.Status = AuctionConst.Category.REJECT;
				}

				_context.AuctionRequests.Update(request);
				_context.Auctions.Update(auction);
				await _context.SaveChangesAsync();
				return output;
			}
			catch (Exception ex)
			{
				return this.Output(ResultCd.FAILURE).WithException(ex).Create<BaseOutputDto>();
			}
		}
	}
}
