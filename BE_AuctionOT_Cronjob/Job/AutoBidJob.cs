using BE_AuctionOT_Cronjob.Modelss;
using BE_AuctionOT_Cronjob.RabbitMQ.BidQueue.Publishers;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace BE_AuctionOT_Cronjob.Job
{
    public class AutoBidJob : IJob
    {
        private readonly DB_AuctionAOTContext _context;
        private readonly IBidPublisher _bidPublisher;

        public AutoBidJob(DB_AuctionAOTContext context, IBidPublisher bidPublisher)
        {
            _context = context;
            _bidPublisher = bidPublisher;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Console.WriteLine("AutoBidJob Start");

                var autoBids = _context.AutoBids.ToList();
                foreach (var autoBid in autoBids)
                {
                    //Get Auction 
                    var auction = _context.Auctions.Where(a => a.AuctionId == autoBid.AuctionId).FirstOrDefault();
                    if (auction != null && DateTime.Now < auction.EndTime && autoBid != null)
                    {
                        //Check first bid
                        var listAuctionBidByAuctionIdAndUserId = _context.AuctionBids.Include(ab => ab.User).ThenInclude(u => u.UserProfile).Where(b => b.AuctionId == autoBid.AuctionId && b.UserId == autoBid.UserId).ToList();
                        if (listAuctionBidByAuctionIdAndUserId.Count == 0)
                        {
                            //Minus deposite amount
                            var userCurrentPoint = _context.Points.FirstOrDefault(o => o.UserId == autoBid.UserId);
                            if (userCurrentPoint.PointsAmount < ((int) auction.DepositAmount))
                            {
                                continue;
                            }
                            else
                            {
                                var point = _context.Points.FirstOrDefault(o => o.UserId == autoBid.UserId);
                                if (point == null)
                                {
                                    continue;
                                }
                                point.PointsAmount = (userCurrentPoint.PointsAmount - (int)auction.DepositAmount);
                                _context.Points.Update(point);
                                _context.SaveChanges();
                            }
                        }

                        var auctionBids = _context.AuctionBids
                                                  .Where(a => a.AuctionId == autoBid.AuctionId)
                                                  .ToList();
                        //if (auctionBids.Any())
                        //{
                        // Find the maximum bid and corresponding auction bid
                        var maxAuctionBid = auctionBids.OrderByDescending(b => b.BidAmount).FirstOrDefault();
                        if (maxAuctionBid != null && maxAuctionBid.UserId != autoBid.UserId && (((int)maxAuctionBid.BidAmount + (int)auction.StepPrice)) <= (int)autoBid.MaxBidAmount)
                        {
                            //Bid for him
                            Bid bid = new Bid()
                            {
                                AuctionId = (int)auction.AuctionId,
                                UserId = Int32.Parse(autoBid.UserId + ""),
                                BidAmount = maxAuctionBid.BidAmount + auction.StepPrice,
                                Timestamp = DateTime.Now,
                            };
                            _bidPublisher.PublishBid(bid);
                        }

                        if (maxAuctionBid == null)
                        {
                            Bid bid = new Bid()
                            {
                                AuctionId = (int)auction.AuctionId,
                                UserId = Int32.Parse(autoBid.UserId + ""),
                                BidAmount = auction.StepPrice,
                                Timestamp = DateTime.Now,
                            };
                            _bidPublisher.PublishBid(bid);
                        }
                        //}
                    }

                }
                await Task.CompletedTask;
                Console.WriteLine("AutoBidJob End");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error AutoBidJob: {ex}");
            }
        }
    }
}
