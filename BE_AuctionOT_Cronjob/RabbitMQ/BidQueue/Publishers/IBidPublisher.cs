using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_AuctionOT_Cronjob.RabbitMQ.BidQueue.Publishers
{
    public interface IBidPublisher
    {
        void PublishBid(Bid bid);
        void CreateANewQueue(string queueName);
    }
}
