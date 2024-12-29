using AutoMapper;
using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.DAO.AuctionManagement.Dispute;
using BE_AuctionAOT.DAO.Common.SystemMessages;
using BE_AuctionAOT.Models;

namespace BE_AuctionAOT.DAO.AuctionManagement.EvidenceFile
{
    public class EvidenceFileDao
    {
        private readonly DB_AuctionAOTContext _context;
        private readonly MessageService _messageService;

        public EvidenceFileDao(DB_AuctionAOTContext context, MessageService messageService)
        {
            _context = context;
            _messageService = messageService;
        }

        public BaseOutputDto SaveEvidenceFile(List<Models.EvidenceFile> evidenceFile)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();
                _context.AddRange(evidenceFile);
                _context.SaveChanges();
                return output;

            }
            catch (Exception ex)
            {
                string[] parameters = {};
                return this.Output(ResultCd.FAILURE).CommonMessageWithInfo(_messageService.GetSystemMessages().First(x => x.Code == "0001").ToString(), "0001", parameters).WithException(ex).Create<BaseOutputDto>();
            }
        }
    }
}
