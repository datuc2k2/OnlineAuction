namespace BE_AuctionOT_Cronjob.Common.Constants
{
    public class AuctionConst
    {
        public class Category
        {
            public const long INPROCESS = 11;
            public const long UNPAID = 13;
            public const long COMINGSOON = 7;
            public const long PAID = 14;
            public const long APPROVED = 12;
            public const long REJECTED = 10;
            public const long INPROGRESS = 8;
            public const long ENDED = 9;

        }
        public class Noti
        {
            public const long SYSTEM = 15;
            public const long WINNER = 40;
            public const long POST_REPORT = 17;
            public const long AUCTION_REPORT = 16;
        }
        public class DISPUTE
        {
            public const long NO_DISPUTE = 36;
            public const long PENDING = 37;
            public const long RESOLVED = 38;
            public const long DISPUTE_REJECTED = 39;
        }
    }
}
