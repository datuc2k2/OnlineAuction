using BE_AuctionAOT.Common.Base.Entity;
namespace BE_AuctionAOT.DAO.Common.User
{
    public class UserDaoOutputDto : BaseOutputDto
    {
        public Models.UserRole? user { get; set; }
    }
    
    public class CheckAccountAlredyExistsOutputDto : BaseOutputDto
    {
        public bool? isExist { get; set; }
    }


}
