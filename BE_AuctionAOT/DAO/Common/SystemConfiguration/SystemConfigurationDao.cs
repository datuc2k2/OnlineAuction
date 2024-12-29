using AutoMapper;
using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.DAO.Common.User;
using BE_AuctionAOT.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BE_AuctionAOT.DAO.Common.SystemConfiguration
{
    public class SystemConfigurationDao
    {
        private readonly DB_AuctionAOTContext _context;
        public SystemConfigurationDao(DB_AuctionAOTContext context)
        {
            _context = context;
        }
        public async Task<string?> GetValue(string input)
        {
            return await _context.SystemConfigurations.Where(x => x.KeyId.Contains(input)).Select(x => x.Value).FirstOrDefaultAsync();
        }

        public ListSysConfigOutputDto GetValues(List<string> input)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<ListSysConfigOutputDto>();
                var lisValue = _context.SystemConfigurations.Where(x => input.Contains(x.KeyId)).Select(x => new SysConfig() { code = x.KeyId, value = x.Value }).ToList();
                output.sysConfig = lisValue;
                return output;
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<ListSysConfigOutputDto>();
            }
        }
    }
}
