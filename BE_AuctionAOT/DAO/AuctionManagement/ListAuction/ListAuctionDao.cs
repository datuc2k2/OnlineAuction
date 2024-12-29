using AutoMapper;
using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Models;
using Microsoft.EntityFrameworkCore;


namespace BE_AuctionAOT.DAO.AuctionManagement.ListAuction
{
    public class ListAuctionDao
    {
        private readonly DB_AuctionAOTContext _context;
        private readonly IMapper _mapper;

        public ListAuctionDao(DB_AuctionAOTContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<GetStatusOutputDto> GetStatusAuction(long type)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<GetStatusOutputDto>();

                var status = _context.Categories.Where(x => x.Type == type).Select(x => new Dropdown()
                {
                    Id = x.CategoryId,
                    Name = x.CategoryName,
                }).ToList();
                output.Status = status;
                return output;

            }
            catch (Exception ex)
            {
                string[] parameters = { "Loi" };
                return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co {0}", parameters).WithException(ex).Create<GetStatusOutputDto>();
            }
        }
        public async Task<ListAuctionOutputDto> GetListAuction(ListAuctionInputDto inputDto, String Input)
        {
            try
            {
                // Lấy danh sách từ database
                List<Models.Auction> auctionList = await _context.Auctions
                    .Include(a => a.AuctionRequests) // Bao gồm các yêu cầu đấu giá
                    .Where(o => o.UserId == int.Parse(Input)) // Lọc theo UserId
                    .ToListAsync();

                if (auctionList == null)
                {
                    return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "User not have Auction").Create<ListAuctionOutputDto>();
                }

                var output = this.Output(ResultCd.SUCCESS).Create<ListAuctionOutputDto>();
                var auctionDto = _mapper.Map<List<Models.Auction>, List<AuctionDto>>(auctionList);

                // Kiểm tra nếu có truyền filter
                var totalRecords = 0;
                var filteredAuctionDto = auctionDto.Where(o => (string.IsNullOrEmpty(inputDto.searchText) || o.ProductName.Contains(inputDto.searchText, StringComparison.OrdinalIgnoreCase)) &&
                                                                (!inputDto.startDate.HasValue || o.CreatedAt >= inputDto.startDate.Value.ToDateTime(TimeOnly.MinValue)) &&
                                                                (!inputDto.endDate.HasValue || o.CreatedAt <= inputDto.endDate.Value.ToDateTime(TimeOnly.MaxValue)) &&
                                                                (inputDto.status == 0 || o.Status == inputDto.status)
                                                                ).OrderByDescending(n => n.CreatedAt).ToList();
                totalRecords = filteredAuctionDto.Count();
                auctionDto = filteredAuctionDto.Skip((int)((inputDto.DisplayCount.DisplayCount - 1) * (int)inputDto.DisplayCount.PageCount))
                    .Take((int)inputDto.DisplayCount.PageCount).ToList();

                var statusDict = _context.Categories
                                                .Where(x => x.Type == 2)
                                                .Select(x => new { x.CategoryId, x.CategoryName })
                                                .ToDictionary(x => x.CategoryId, x => x.CategoryName);
                var feeDict = _context.Categories
                                                .Where(x => x.Type == 0)
                                                .Select(x => new { x.CategoryId, x.Value })
                                                .ToDictionary(x => x.CategoryId, x => x.Value);
                foreach (var item in auctionDto)
                {
                    if (statusDict.TryGetValue(item.Status, out string statusName))
                    {
                        item.StatusName = statusName; // Gán tên
                    }
                    if (feeDict.TryGetValue(item.CategoryId, out decimal? Value))
                    {
                        item.Fee = Value; // Gán tên
                    }
                }

                output.allRecords = totalRecords;
                output.Auction = auctionDto;
                return output;
            }
            catch (Exception ex)
            {
                string[] parameters = { "Loi" };
                return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co {0}", parameters).WithException(ex).Create<ListAuctionOutputDto>();
            }
        }
        public async Task<ListAuctionOutputDto> GetListAuctionAdmin(ListAuctionInputDto inputDto)
        {
            try
            {
                // Lấy danh sách từ database
                List<Models.Auction> auctionList = await _context.Auctions
                    .Include(a => a.AuctionRequests) // Bao gồm các yêu cầu đấu giá
                    .ToListAsync();

                if (auctionList == null)
                {
                    return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Don't have Auction").Create<ListAuctionOutputDto>();
                }

                var output = this.Output(ResultCd.SUCCESS).Create<ListAuctionOutputDto>();
                var auctionDto = _mapper.Map<List<Models.Auction>, List<AuctionDto>>(auctionList);

                // Kiểm tra nếu có truyền filter
                var totalRecords = 0;
                var filteredAuctionDto = auctionDto.Where(o => (string.IsNullOrEmpty(inputDto.searchText) || o.ProductName.Contains(inputDto.searchText, StringComparison.OrdinalIgnoreCase)) &&
                                                                (!inputDto.startDate.HasValue || o.CreatedAt >= inputDto.startDate.Value.ToDateTime(TimeOnly.MinValue)) &&
                                                                (!inputDto.endDate.HasValue || o.CreatedAt <= inputDto.endDate.Value.ToDateTime(TimeOnly.MaxValue)) &&
                                                                (inputDto.status == 0 || o.Status == inputDto.status)
                                                                ).OrderByDescending(n => n.CreatedAt).ToList();
                totalRecords = filteredAuctionDto.Count();
                auctionDto = filteredAuctionDto.Skip((int)((inputDto.DisplayCount.DisplayCount - 1) * (int)inputDto.DisplayCount.PageCount))
                    .Take((int)inputDto.DisplayCount.PageCount).ToList();

                var statusDict = _context.Categories
                                                .Where(x => x.Type == 2)
                                                .Select(x => new { x.CategoryId, x.CategoryName })
                                                .ToDictionary(x => x.CategoryId, x => x.CategoryName);
                foreach (var item in auctionDto)
                {
                    if (statusDict.TryGetValue(item.Status, out string statusName))
                    {
                        item.StatusName = statusName; // Gán tên
                    }
                }

                output.allRecords = totalRecords;
                output.Auction = auctionDto;
                return output;
            }
            catch (Exception ex)
            {
                string[] parameters = { "Loi" };
                return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co {0}", parameters).WithException(ex).Create<ListAuctionOutputDto>();
            }
        }
        public async Task<BaseOutputDto> InactiveAuction(int AuctionId)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();
                var auction = await _context.Auctions.FirstOrDefaultAsync(i => i.AuctionId == AuctionId);
                if (auction == null)
                {
                    return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Don't find Auction").Create<BaseOutputDto>();
                }
                auction.IsActive = false;
                auction.Status = AuctionConst.Category.CANCELED;
                _context.Auctions.Update(auction);
                await _context.SaveChangesAsync();
                return output;

            }
            catch (Exception ex)
            {
                string[] parameters = { "Loi" };
                return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("1", "Dang co {0}", parameters).WithException(ex).Create<BaseOutputDto>();
            }
        }

        public async Task<JoinedAuctionOutputDto> GetJoinedAuctionList(JoinedAuctionInputDto inputDto, long uId)
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
                                                                (inputDto.isWinner == null || o.Auction.isWinner == inputDto.isWinner)
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

        public async Task<AuctionInputDto> GetListByFillter(FillterAuctionInputDto fillter)
        {
            try
            {
                int start = 0;
                if (fillter.pageSize == 0)
                {
                    fillter.pageSize = 10;
                }
                if (fillter.pageIndex > 0)
                    start = (fillter.pageIndex - 1) * fillter.pageSize;
                var output = this.Output(ResultCd.SUCCESS).Create<AuctionInputDto>();
                var data = await (from row in _context.Auctions.Include(a => a.AuctionImages)
                                  join user in _context.Users on row.UserId equals user.UserId
                                  where row.IsActive == true &&
                                         (fillter.CategoryId != null ? row.CategoryId == fillter.CategoryId : true) &&
                                         (fillter.Status != null ? row.Status == fillter.Status : true)                                 
                                  orderby row.AuctionId descending
                                  select new Models.Auction
                                  {
                                      AuctionId = row.AuctionId,
                                      UserId = row.UserId,
                                      ProductName = row.ProductName,
                                      CategoryId = row.CategoryId,
                                      StartingPrice = row.StartingPrice,
                                      Currency = row.Currency,
                                      StepPrice = row.StepPrice,
                                      Mode = row.Mode,
                                      Description = row.Description,
                                      IsPrivate = row.IsPrivate,
                                      DepositAmount = row.DepositAmount,
                                      DepositDeadline = row.DepositDeadline,
                                      StartTime = row.StartTime,
                                      EndTime = row.EndTime,
                                      IsActive = row.IsActive,
                                      CreatedAt = row.CreatedAt,
                                      UpdatedAt = row.UpdatedAt,
                                      CreatedBy = row.CreatedBy,
                                      UpdatedBy = row.UpdatedBy,
                                      Status = row.Status,
                                      PaymentStatus = row.PaymentStatus,
                                      User = new User
                                      {
                                          UserId = user.UserId,

                                          Username = user.Username ,// Gán UserName từ bảng Users
                                         UserProfile = new UserProfile
                                         {
                                             Avatar = user.UserProfile.Avatar
                                         }
                                      },
                                      AuctionImages = row.AuctionImages,
                                  }).ToListAsync();

                switch (fillter.Status)
                {
                    case Status.StatusGoing:
                        data = data.Where(x =>
                        (fillter.StartPrice != null ? x.StartingPrice >= fillter.StartPrice : true) &&
                        (fillter.EndPrice != null ? x.StartingPrice <= fillter.EndPrice : true) &&
                        (fillter.StartDate != null ? x.StartTime.Date >= fillter.StartDate.Value.Date : true) &&
                        (fillter.EndDate != null ? x.StartTime <= fillter.EndDate : true)
                        ).ToList();
                        break;
                    case Status.StatusOnGoing:
                        data = data.Where(x =>
                        (fillter.StartPrice != null ? x.StartingPrice >= fillter.StartPrice : true) &&
                        (fillter.EndPrice != null ? x.StartingPrice <= fillter.EndPrice : true) &&
                        (fillter.StartDate != null ? x.StartTime.Date >= fillter.StartDate.Value.Date : true) &&
                        (fillter.EndDate != null ? x.StartTime <= fillter.EndDate : true)
                        ).ToList();
                        break;
                    case Status.StatusEnded:
                        data = data.Where(x =>
                        (fillter.StartPrice != null ? x.StartingPrice >= fillter.StartPrice : true) &&
                        (fillter.EndPrice != null ? x.StartingPrice <= fillter.EndPrice : true) &&
                        (fillter.StartDate != null ? x.StartTime.Date >= fillter.StartDate.Value.Date : true) &&
                        (fillter.EndDate != null ? x.StartTime <= fillter.EndDate : true)
                        ).ToList();
                        break;
                    default:
                        break;
                }

                output.TotalRecords = data.Count;
                output.Auctions = data.Skip(start).Take(fillter.pageSize).ToList();
                return output;
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<AuctionInputDto>();

            }
        }
        public async Task<AuctionInputDto> GetListByFillterViewAuction(FillterViewAuctionInputDto fillter)
        {
            try
            {
                int start = 0;
                if (fillter.pageSize == 0)
                {
                    fillter.pageSize = 10;
                }
                if (fillter.pageIndex > 0)
                    start = (fillter.pageIndex - 1) * fillter.pageSize;
                var output = this.Output(ResultCd.SUCCESS).Create<AuctionInputDto>();
                var data = await (from row in _context.Auctions.Include(a => a.AuctionImages)
                                  join user in _context.Users on row.UserId equals user.UserId
                                  where row.IsActive == true &&
                                         (fillter.Status != null ? row.Status == fillter.Status : true) 
                                         && fillter.UserId == user.UserId
                                  orderby row.AuctionId descending
                                  select new Models.Auction
                                  {
                                      AuctionId = row.AuctionId,
                                      UserId = row.UserId,
                                      ProductName = row.ProductName,
                                      CategoryId = row.CategoryId,
                                      StartingPrice = row.StartingPrice,
                                      Currency = row.Currency,
                                      StepPrice = row.StepPrice,
                                      Mode = row.Mode,
                                      Description = row.Description,
                                      IsPrivate = row.IsPrivate,
                                      DepositAmount = row.DepositAmount,
                                      DepositDeadline = row.DepositDeadline,
                                      StartTime = row.StartTime,
                                      EndTime = row.EndTime,
                                      IsActive = row.IsActive,
                                      CreatedAt = row.CreatedAt,
                                      UpdatedAt = row.UpdatedAt,
                                      CreatedBy = row.CreatedBy,
                                      UpdatedBy = row.UpdatedBy,
                                      Status = row.Status,
                                      PaymentStatus = row.PaymentStatus,
                                      User = new User
                                      {
                                          UserId = user.UserId,
                                          Username = user.Username,// Gán UserName từ bảng Users
                                          UserProfile = new UserProfile
                                          {
                                              Avatar = user.UserProfile.Avatar
                                          }
                                      },
                                      AuctionImages = row.AuctionImages,
                                  }).ToListAsync();
                output.TotalRecords = data.Count;
                output.Auctions = data.Skip(start).Take(fillter.pageSize).ToList();
                return output;
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<AuctionInputDto>();

            }
        }
    }

	}
