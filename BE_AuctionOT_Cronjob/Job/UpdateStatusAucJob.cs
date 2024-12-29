using BE_AuctionOT_Cronjob.Common.Base.Entity;
using BE_AuctionOT_Cronjob.Common.Constants;
using BE_AuctionOT_Cronjob.Common.Utility;
using BE_AuctionOT_Cronjob.Modelss;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Quartz;

namespace BE_AuctionOT_Cronjob.Job
{
    public class UpdateStatusAucJob : IJob
    {
        private readonly DB_AuctionAOTContext _context;
        private readonly IMemoryCache _cache;
        private readonly Mail _mailUtility;
        private readonly IConfiguration _configuration;
        public UpdateStatusAucJob(DB_AuctionAOTContext context, IMemoryCache memoryCache, Mail mailUtility, IConfiguration configuration)
        {
            _context = context;
            _cache = memoryCache;
            _mailUtility = mailUtility;
            _configuration = configuration;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Console.WriteLine("UpdateStatusAucJob Start");


                // Get messages from Cache
                const string cacheKey = "SystemMessages";
                var systemMessages = new List<SystemMessage>();

                if (!_cache.TryGetValue(cacheKey, out systemMessages))
                {
                    systemMessages = await _context.SystemMessages.ToListAsync();

                    _cache.Set(cacheKey, systemMessages, TimeSpan.FromHours(6));
                }

                // Update unpaid auctions to rejected
                var payLates = await _context.Auctions
                    .Where(x => x.StartTime <= DateTime.Now &&
                                x.PaymentStatus == AuctionConst.Category.UNPAID &&
                                x.Status == AuctionConst.Category.APPROVED)
                    .ToListAsync();

                foreach (var auction in payLates)
                {
                    auction.Status = AuctionConst.Category.REJECTED;
                }

                // Update paid and approved auctions to in-progress
                var upcomingAucs = await _context.Auctions
                    .Where(x => x.StartTime <= DateTime.Now &&
                                x.EndTime > DateTime.Now &&
                                x.PaymentStatus == AuctionConst.Category.PAID &&
                                x.Status == AuctionConst.Category.COMINGSOON)
                    .ToListAsync();

                foreach (var auction in upcomingAucs)
                {
                    auction.Status = AuctionConst.Category.INPROGRESS;
                }

                // Update ended auctions to ended
                var endAucs = await _context.Auctions
                    .Where(x => x.EndTime <= DateTime.Now &&
                                x.PaymentStatus == AuctionConst.Category.PAID &&
                                x.Status == AuctionConst.Category.INPROGRESS)
                    .ToListAsync();

                var depositList = new List<Deposit>();
                var pointList = new List<Point>();
                long? winId;
                string? msg = string.Empty;
                string? winnerMail;

                foreach (var auction in endAucs)
                {
                    auction.Status = AuctionConst.Category.ENDED;

                    winId = _context.AuctionBids
                        .Where(x => x.AuctionId == auction.AuctionId && x.BidTime <= auction.EndTime)
                        .OrderByDescending(x => x.BidTime).ThenByDescending(x => x.BidAmount)
                        .Select(x => x.UserId).FirstOrDefault();

                    if (winId != null && winId != 0)
                    {
                        // create Disputes
                        var dispute = new Dispute()
                        {
                            AuctionId = auction.AuctionId,
                            WinnerId = (long)winId,
                            CreatorId = auction.UserId,
                            DisputeStatusId = AuctionConst.DISPUTE.NO_DISPUTE
                        };

                        _context.Disputes.Add(dispute);

                        msg = systemMessages?.First(x => x.Code == "1000").Message;
                        // create noti for winnwer 
                        var noti = new Notification()
                        {
                            UserId = (long)winId,
                            Type = (int)AuctionConst.Noti.WINNER,
                            Title = CommonConst.TITLE_WINMAIL,
                            Content = MessageUtility.ReplacePlaceholders(msg, auction.AuctionId, auction.ProductName)
                        };
                        _context.Notifications.Add(noti);

                        // send mail to Winner

                        winnerMail = _context.UserProfiles.FirstOrDefault(x => x.UserId == winId)?.Email;
                        if (!string.IsNullOrEmpty(winnerMail))
                        {
                            SendMailDTO sendMail = new()
                            {
                                FromEmail = _configuration["SysMail:Email"],
                                Password = _configuration["SysMail:Password"],
                                ToEmail = winnerMail,
                                Subject = noti.Title,
                                Body = noti.Content,
                            };

                            if (!await _mailUtility.SendEmail(sendMail))
                            {
                                Console.WriteLine($"Send Winner mail to {winnerMail} fail.");
                            }
                        }

                        // return point
                        depositList = _context.Deposits
                            .Where(x => x.AuctionId == auction.AuctionId && x.UserId != winId)
                            .OrderBy(x => x.DepositDate).ToList();

                        pointList = _context.Points.Where(x => depositList.Select(p => p.UserId).Contains(x.UserId)).ToList();

                        foreach (var deposit in depositList)
                        {
                            var point = pointList.Where(x => x.UserId == deposit.UserId).FirstOrDefault();
                            if (point != null)
                            {
                                deposit.DepositStatus = "False";
                                point.PointsAmount += deposit.DepositAmount;
                                point.LastUpdated = DateTime.Now;

                                PointTransaction newPointTransaction = new()
                                {
                                    UserId = deposit.UserId,
                                    Amount = deposit.DepositAmount,
                                    Currency = deposit.Currency.ToString(),
                                    TransactionTime = DateTime.Now,
                                    Description = $"Trả tiền cọc cho phiên đấu giá: AuctionId: {auction.AuctionId}, tên phiên: {auction.ProductName}",
                                    TransactionCode = Guid.NewGuid().ToString("N").Substring(0, 16)
                                };

                                _context.PointTransactions.Add(newPointTransaction);
                            }
                        }
                    }
                }

                // Save all changes once
                await _context.SaveChangesAsync();

                Console.WriteLine("UpdateStatusAucJob End");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating auction statuses: {ex}");
            }
        }
    }
}
