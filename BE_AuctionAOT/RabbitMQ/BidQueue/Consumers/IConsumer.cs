using BE_AuctionAOT.RabbitMQ.BidQueue.Publishers;

namespace BE_AuctionAOT.RabbitMQ.BidQueue.Consumers
{
    public interface IConsumer
    {
        Task StartListening(int auctionId);
        string HandleBid(Bid bid);
    }
}
