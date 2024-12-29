using BE_AuctionAOT.Common.Base.Entity;

namespace BE_AuctionAOT.DAO.Common.SystemConfiguration
{
    public class SystemConfigurationOutputDto
    {
    }

    public class ListSysConfigOutputDto : BaseOutputDto
    {
        public List<SysConfig>? sysConfig {  get; set; }
    }
    public class SysConfig
    {
        public string? code { get; set; }
        public string? value { get; set; }
    }
}
