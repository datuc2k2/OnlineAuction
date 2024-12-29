using AutoMapper;
using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Common.Utility;
using BE_AuctionAOT.Controllers.Personal_Account_Management;
using BE_AuctionAOT.DAO.AuctionManagement.Auction;
using BE_AuctionAOT.DAO.Common.SystemMessages;
using BE_AuctionAOT.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using System;
using static BE_AuctionAOT.Common.Constants.AuctionConst;

namespace BE_AuctionAOT.DAO.AuctionManagement.Dispute
{
	public class DisputeDao
	{
		private readonly DB_AuctionAOTContext _context;
		private readonly IMapper _mapper;
		private readonly MessageService _messageService;

		public DisputeDao(DB_AuctionAOTContext context, IMapper mapper, MessageService messageService)
		{
			_mapper = mapper;
			_context = context;
			_messageService = messageService;
		}


		//xu li confirm thanh toan
		public async Task<JoinedAuctionOutputDto> GetWinnedAuctionForUser(WinnerAuctionInputDto inputDto, long uId)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<JoinedAuctionOutputDto>();

				List<Models.AuctionBid?> filteredAuctionBidList = await _context.AuctionBids.Include(o => o.Auction)
															.Where(u => u.UserId == uId && _context.Disputes.Any(d => d.AuctionId == u.AuctionId))
															.GroupBy(b => new { b.AuctionId, b.UserId })
															.Select(g => g.OrderByDescending(b => b.BidTime).FirstOrDefault())
															.ToListAsync();
				if (filteredAuctionBidList == null)
				{
					return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Chưa tham gia đấu giá phiên nào").Create<JoinedAuctionOutputDto>();
				}
				var filteredAuctionBidListDto = _mapper.Map<List<Models.AuctionBid>, List<AuctionBidDto>>(filteredAuctionBidList);
				var statusDict = _context.Categories
												.Where(x => x.Type == 2)
												.Select(x => new { x.CategoryId, x.CategoryName })
												.ToDictionary(x => x.CategoryId, x => x.CategoryName);
				var categorisDict = _context.Categories
												.Where(x => x.Type == 0)
												.Select(x => new { x.CategoryId, x.CategoryName })
												.ToDictionary(x => x.CategoryId, x => x.CategoryName);
				foreach (var bid in filteredAuctionBidListDto)
				{
					var dispute = _context.Disputes.FirstOrDefault(o => o.AuctionId == bid.AuctionId);
					bid.confirmStatus = dispute.WinnerConfirmed switch
					{
						null => "Chưa xác nhận thanh toán",
						true => "Đã xác nhận thanh toán",
						false => "Đã báo cáo tranh chấp",
					};
					if (bid.Auction.DepositDeadline <= DateTime.UtcNow)
					{
						bid.depositDlStatus = "Đã quá hạn xác nhận";
					}
					else
					{
						bid.depositDlStatus = "Chưa tới hạn xác nhận";
					}

					if (statusDict.TryGetValue(bid.Auction.Status, out string statusName))
					{
						bid.Auction.StatusName = statusName;
					}
					if (categorisDict.TryGetValue(bid.Auction.CategoryId, out string cateName))
					{
						bid.Auction.CategoryName = cateName;
					}
					var BidWin = _context.AuctionBids
												.Where(b => b.AuctionId == bid.AuctionId)
												.OrderByDescending(b => b.BidAmount)
												.FirstOrDefault();
					bid.Auction.EndPrice = BidWin.BidAmount;
					if (BidWin.UserId == bid.UserId)
					{
						bid.Auction.isWinner = true;
					}
					else
					{
						bid.Auction.isWinner = false;
					}
				}
				filteredAuctionBidListDto = filteredAuctionBidListDto.Where(o => (string.IsNullOrEmpty(inputDto.searchText) || o.Auction.ProductName.Contains(inputDto.searchText, StringComparison.OrdinalIgnoreCase)) &&
																(!inputDto.startDate.HasValue || o.Auction.EndTime >= inputDto.startDate.Value.ToDateTime(TimeOnly.MinValue)) &&
																(!inputDto.endDate.HasValue || o.Auction.EndTime <= inputDto.endDate.Value.ToDateTime(TimeOnly.MaxValue)) &&
																(o.Auction.isWinner == true)
																).OrderByDescending(n => n.Auction.EndTime).ToList();
				var totalRecods = filteredAuctionBidListDto.Count();
				//paging
				filteredAuctionBidListDto = filteredAuctionBidListDto.Skip((int)((inputDto.DisplayCount.DisplayCount - 1) * (int)inputDto.DisplayCount.PageCount))
					.Take((int)inputDto.DisplayCount.PageCount).ToList();

				output.auctionBidDtos = filteredAuctionBidListDto;
				output.allRecords = totalRecods;
				return output;
			}
			catch (Exception ex)
			{
				string[] parameters = { "Loi" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co {0}", parameters).WithException(ex).Create<JoinedAuctionOutputDto>();
			}
		}
		public async Task<ListBidedOutputDto> GetAuctionListWasBided(ListAuctionBiddedInputDto inputDto, long uId)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<ListBidedOutputDto>();
				List<Models.Auction> filteredAuctionBidList = await _context.Auctions
															.Where(u => u.UserId == uId && _context.Disputes.Any(d => d.AuctionId == u.AuctionId))
															.OrderByDescending(n => n.EndTime)
															.ToListAsync();
				if (filteredAuctionBidList == null)
				{
					return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Chưa tham gia đấu giá phiên nào").Create<ListBidedOutputDto>();
				}
				var filteredAuctionBidListDto = _mapper.Map<List<Models.Auction>, List<AuctionBidedDto>>(filteredAuctionBidList);
				var categorisDict = _context.Categories
												.Where(x => x.Type == 0)
												.Select(x => new { x.CategoryId, x.CategoryName })
												.ToDictionary(x => x.CategoryId, x => x.CategoryName);
				foreach (var auc in filteredAuctionBidListDto)
				{
					var dispute = _context.Disputes.FirstOrDefault(o => o.AuctionId == auc.AuctionId);
					auc.confirmStatus = dispute.CreatorConfirmed switch
					{
						null => "Chưa xác nhận thanh toán",
						true => "Đã xác nhận thanh toán",
						false => "Đã báo cáo tranh chấp",
					};
					if(auc.DepositDeadline <= DateTime.UtcNow)
					{
						auc.depositDlStatus = "Đã quá hạn xác nhận";
					}
					else
					{
						auc.depositDlStatus = "Chưa tới hạn xác nhận";
					}
					if (categorisDict.TryGetValue(auc.CategoryId, out string cateName))
					{
						auc.CategoryName = cateName;
					}
					var BidWin = _context.AuctionBids
												.Where(b => b.AuctionId == auc.AuctionId)
												.OrderByDescending(b => b.BidAmount)
												.FirstOrDefault();
					auc.EndPrice = BidWin.BidAmount;
					auc.WinnerID = BidWin.UserId;
					var userWin = await _context.Users.FirstOrDefaultAsync(o => o.UserId == BidWin.UserId);
					auc.WinnerName = userWin.Username;
				}
				filteredAuctionBidListDto = filteredAuctionBidListDto.Where(o => (string.IsNullOrEmpty(inputDto.searchText) || o.ProductName.Contains(inputDto.searchText, StringComparison.OrdinalIgnoreCase)) &&
																(!inputDto.startDate.HasValue || o.EndTime >= inputDto.startDate.Value.ToDateTime(TimeOnly.MinValue)) &&
																(!inputDto.endDate.HasValue || o.EndTime <= inputDto.endDate.Value.ToDateTime(TimeOnly.MaxValue))
																).OrderByDescending(n => n.EndTime).ToList();
				var totalRecods = filteredAuctionBidListDto.Count();
				//paging
				filteredAuctionBidListDto = filteredAuctionBidListDto.Skip((int)((inputDto.DisplayCount.DisplayCount - 1) * (int)inputDto.DisplayCount.PageCount))
					.Take((int)inputDto.DisplayCount.PageCount).ToList();
				output.Auction = filteredAuctionBidListDto;
				output.allRecords = totalRecods;
				return output;

			}
			catch (Exception ex)
			{
				string[] parameters = { "Loi" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co {0}", parameters).WithException(ex).Create<ListBidedOutputDto>();
			}
		}

		public async Task<ListDisputeOutputDto> GetDisputeListAdmin(ListDisputeInputDto inputDto)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<ListDisputeOutputDto>();
				List<Models.Dispute> disputes = await _context.Disputes.Include(o => o.Auction).Where(o => o.DisputeStatusId == 37 &&
																						o.Auction.DepositDeadline >= DateTime.UtcNow)
																						.ToListAsync();
				if (disputes == null)
				{
					return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Không có tranh chấp cần xử lí").Create<ListDisputeOutputDto>();
				}
				var disputesDto = _mapper.Map<List<Models.Dispute>, List<DisputeDto>>(disputes);
				disputesDto = disputesDto.Where(o => (string.IsNullOrEmpty(inputDto.searchText) || o.Auction.ProductName.Contains(inputDto.searchText, StringComparison.OrdinalIgnoreCase)) &&
																(!inputDto.startDate.HasValue || o.CreatedAt >= inputDto.startDate.Value.ToDateTime(TimeOnly.MinValue)) &&
																(!inputDto.endDate.HasValue || o.CreatedAt <= inputDto.endDate.Value.ToDateTime(TimeOnly.MaxValue))
																).OrderByDescending(n => n.CreatedAt).ToList();
				var totalRecods = disputesDto.Count();

				foreach (var disDto in disputesDto)
				{
					Models.User Winner = await _context.Users.FirstOrDefaultAsync(o => o.UserId == disDto.WinnerId);
					disDto.WinnerName = Winner.Username;
					Models.User Creator = await _context.Users.FirstOrDefaultAsync(o => o.UserId == disDto.CreatorId);
					disDto.CreatorName = Creator.Username;
				}
				//paging
				disputesDto = disputesDto.Skip((int)((inputDto.DisplayCount.DisplayCount - 1) * (int)inputDto.DisplayCount.PageCount))
					.Take((int)inputDto.DisplayCount.PageCount).ToList();

				output.disputeDtos = disputesDto;
				output.allRecords = totalRecods;
				return output;
			}
			catch (Exception ex)
			{
				string[] parameters = { "Loi" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co {0}", parameters).WithException(ex).Create<ListDisputeOutputDto>();
			}
		}
		public async Task<BaseOutputDto> TrueConfirmedCreator(int AuctionId)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();
				var dispute = await _context.Disputes.FirstOrDefaultAsync(i => i.AuctionId == AuctionId);
				if (dispute == null)
				{
					return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Có lỗi trong quá trình xác nhận").Create<BaseOutputDto>();
				}
				dispute.CreatorConfirmed = true;
				_context.Disputes.Update(dispute);
				await _context.SaveChangesAsync();
				return output;

			}
			catch (Exception ex)
			{
				string[] parameters = { "Loi" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co {0}", parameters).WithException(ex).Create<BaseOutputDto>();
			}
		}
		//chua xong, hoi lai trung cai confirm admin cho chac
		public async Task<BaseOutputDto> AdminConfirm(AdminDecisionInputDto inputDto)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();
				var dispute = await _context.Disputes.Include(o => o.Auction).FirstOrDefaultAsync(i => i.DisputeId == inputDto.DisputeID);
				Models.Point pointWinner = await _context.Points.FirstOrDefaultAsync(o => o.UserId == dispute.WinnerId);
				Models.Point pointCreator = await _context.Points.FirstOrDefaultAsync(o => o.UserId == dispute.CreatorId);
				if (dispute == null)
				{
					return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Có lỗi trong quá trình xác nhận").Create<BaseOutputDto>();
				}
				Models.PointTransaction pointTransaction = new PointTransaction();
				String guid = Guid.NewGuid().ToString("N").Substring(0, 16);
				if (inputDto.IsCreatorTrue)
				{
					dispute.DisputeStatusId = 38;
					dispute.AdminDecision = inputDto.Decision;
					dispute.ResolvedAt = DateTime.Now;
					pointCreator.PointsAmount = pointCreator.PointsAmount + (dispute.Auction.DepositAmount ?? 0);

					pointTransaction.UserId = dispute.CreatorId;
					pointTransaction.Amount = dispute.Auction.DepositAmount ?? 0;
					pointTransaction.Currency = "VND";
					pointTransaction.TransactionTime = DateTime.Now;
					pointTransaction.Description = $"Tiền hoàn sau xử lý tranh chấp phiên {dispute.AuctionId} : {dispute.Auction.ProductName}";
					pointTransaction.TransactionCode = guid;

				}
				else
				{
					dispute.DisputeStatusId = 39;
					dispute.AdminDecision = inputDto.Decision;
					dispute.ResolvedAt = DateTime.Now;
					pointWinner.PointsAmount = pointCreator.PointsAmount + (dispute.Auction.DepositAmount ?? 0);

					pointTransaction.UserId = dispute.WinnerId;
					pointTransaction.Amount = dispute.Auction.DepositAmount ?? 0;
					pointTransaction.Currency = "VND";
					pointTransaction.TransactionTime = DateTime.Now;
					pointTransaction.Description = $"Tiền hoàn sau xử lý tranh chấp phiên {dispute.AuctionId} : {dispute.Auction.ProductName}";
					pointTransaction.TransactionCode = guid;
				}
				_context.PointTransactions.Add(pointTransaction);
				_context.Points.Update(pointWinner);
				_context.Points.Update(pointCreator);
				_context.Disputes.Update(dispute);
				await _context.SaveChangesAsync();
				return output;

			}
			catch (Exception ex)
			{
				string[] parameters = { "Loi" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co {0}", parameters).WithException(ex).Create<BaseOutputDto>();
			}
		}

		public async Task<CustomerPaymentConfirmationOutputDto> GetDisputeById(long aucId)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<CustomerPaymentConfirmationOutputDto>();
				var dispute = await _context.Disputes.FirstOrDefaultAsync(i => i.AuctionId == aucId);
				if (dispute == null)
				{
					string[] parameters = { "I" };
					return this.Output(ResultCd.FAILURE).CommonMessageWithInfo(_messageService.GetSystemMessages().First(x => x.Code == "1001").ToString(), "1001", parameters).Create<CustomerPaymentConfirmationOutputDto>();
				}
				output.Dispute = dispute;
				return output;

			}
			catch (Exception ex)
			{
				string[] parameters = { "E" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo(_messageService.GetSystemMessages().First(x => x.Code == "0001").ToString(), "0001", parameters).WithException(ex).Create<CustomerPaymentConfirmationOutputDto>();
			}
		}

		public async Task<BaseOutputDto> Confirm(Models.Dispute dispute)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();
				_context.Update(dispute);
				_context.SaveChanges();
				return output;

			}
			catch (Exception ex)
			{
				string[] parameters = { "E" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo(_messageService.GetSystemMessages().First(x => x.Code == "0001").ToString(), "0001", parameters).WithException(ex).Create<BaseOutputDto>();
			}
		}

		public async Task<DisputeDetailDto> GetDisputeDetail(long disputeId)
		{
			try
			{
				var output = this.Output(ResultCd.SUCCESS).Create<DisputeDetailDto>();
				Models.Dispute dispute = await _context.Disputes.Include(o => o.Auction).FirstOrDefaultAsync(o => o.DisputeId == disputeId);
				if (dispute == null)
				{
					return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Có lỗi trong quá trình lấy thông tin tranh chấp").Create<DisputeDetailDto>();
				}
				var disputeDto = _mapper.Map<Models.Dispute, DisputeDto>(dispute);
				Models.User Winner = await _context.Users.FirstOrDefaultAsync(o => o.UserId == disputeDto.WinnerId);
				disputeDto.WinnerName = Winner.Username;
				Models.User Creator = await _context.Users.FirstOrDefaultAsync(o => o.UserId == disputeDto.CreatorId);
				disputeDto.CreatorName = Creator.Username;
				var categorisDict = _context.Categories
												.Where(x => x.Type == 0)
												.Select(x => new { x.CategoryId, x.CategoryName })
												.ToDictionary(x => x.CategoryId, x => x.CategoryName);
				if (categorisDict.TryGetValue(disputeDto.Auction.CategoryId, out string cateName))
				{
					disputeDto.Auction.CategoryName = cateName;
				}
				var BidWin = _context.AuctionBids
											.Where(b => b.AuctionId == disputeDto.AuctionId)
											.OrderByDescending(b => b.BidAmount)
											.FirstOrDefault();
				disputeDto.Auction.EndPrice = BidWin.BidAmount;

				var winnerEvidence = await _context.EvidenceFiles.FirstOrDefaultAsync(o => o.DisputeId == disputeDto.DisputeId && o.UploadedBy == disputeDto.WinnerId);
				var creatorEvidence = await _context.EvidenceFiles.FirstOrDefaultAsync(o => o.DisputeId == disputeDto.DisputeId && o.UploadedBy == disputeDto.CreatorId);
				var winnerEvidenceDto = _mapper.Map<Models.EvidenceFile, EvidenceFileDto>(winnerEvidence);
				var creatorEvidenceDto = _mapper.Map<Models.EvidenceFile, EvidenceFileDto>(creatorEvidence);
				output.disputeDto = disputeDto;
				output.winnerEvidence = winnerEvidenceDto;
				output.creatorEvidence = creatorEvidenceDto;
				return output;

			}
			catch (Exception ex)
			{
				string[] parameters = { "E" };
				return this.Output(ResultCd.FAILURE).CommonMessageWithInfo(_messageService.GetSystemMessages().First(x => x.Code == "0001").ToString(), "0001", parameters).WithException(ex).Create<DisputeDetailDto>();
			}
		}
	}
}
