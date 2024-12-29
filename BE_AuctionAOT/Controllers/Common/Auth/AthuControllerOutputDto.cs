using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Models;
using System.ComponentModel.DataAnnotations;

namespace BE_AuctionAOT.Controllers.Common.Auth
{
    public class LoginControllerOutputDto : BaseOutputDto
    {
        public string accessToken { get; set; }

    }

    public class ConfirmOutputDto : BaseOutputDto
    {
        public bool isSuccess { get; set; }
    }

    public class GetUserOutputDto : BaseOutputDto
    {
        public UserProfile? userProfile { get; set; }
    }


}
