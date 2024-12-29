using System.Security.Cryptography;

namespace BE_AuctionAOT.RabbitMQ.BidQueue.Publishers
{
    public interface IBidPublisher
    {
        void PublishBid(Bid bid);
        void CreateANewQueue(string queueName);
    }
}
