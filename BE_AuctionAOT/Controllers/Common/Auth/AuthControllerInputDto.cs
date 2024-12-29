using System.ComponentModel.DataAnnotations;

namespace BE_AuctionAOT.Controllers.Common.Auth
{
    public class AuthControllerInputDto
    {
    }

    public class LoginControllerInputDto
    {
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
    }
    public class RegisterControllerInputDto
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? Password { get; set; }
    }

    public class ConfirmInputDto
    {
        [Required]
        public string? Email { get; set; }
        [Required]
        public string Code { get; set; } = string.Empty;
    }

    public class CreateNewPasswordInputDto
    {
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
    }

    public class ResetPasswordInputDto
    {
        [Required]
        public string? Email { get; set; }
    }

    public class ReSendVerifyCodeInputDto
    {
        [Required]
        public string? Email { get; set; }
    }

    public class ChangePasswordInputDto
    {
        [Required]
        public string? OldPassword { get; set; }
        [Required]
        public string? NewPassword { get; set; }
    }
}
