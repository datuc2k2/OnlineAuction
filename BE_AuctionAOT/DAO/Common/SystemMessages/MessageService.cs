using BE_AuctionAOT.Models;
using Microsoft.Extensions.Caching.Memory;

namespace BE_AuctionAOT.DAO.Common.SystemMessages
{
    public class MessageService
    {
        private readonly IMemoryCache _cache;
        private readonly DB_AuctionAOTContext _dbContext;
        public MessageService(IMemoryCache cache, DB_AuctionAOTContext dbContext)
        {
            _cache = cache;
            _dbContext = dbContext;
        }

        public IEnumerable<SystemMessage> GetSystemMessages()
        {
            var cacheKey = "SystemMessages";

            if (!_cache.TryGetValue(cacheKey, out IEnumerable<SystemMessage> messages))
            {
                messages = _dbContext.SystemMessages.ToList();

                _cache.Set(cacheKey, messages, TimeSpan.FromHours(6));
            }

            return messages;
        }
    }
}
