using BE_AuctionAOT.DAO.Common.User;
using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Common.Utility;
using BE_AuctionAOT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text;
using BE_AuctionAOT.DAO.Common.SystemConfiguration;
using Microsoft.AspNetCore.Components.Forms;
using BE_AuctionAOT.DAO.Common.SystemMessages;


namespace BE_AuctionAOT.Controllers.Common.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly UserDao _userDao;
        private readonly SystemConfigurationDao _systemConfigurationDao;
        private readonly MessageService _messageService;
        private readonly Number _numberUtility;
        private readonly Mail _mailUtility;
        private readonly AuthUtility _authUtility;
        private readonly IConfiguration _configuration;
        public AuthController(UserDao userDao, SystemConfigurationDao systemConfigurationDao, IConfiguration configuration, Number numberUtility, Mail mailUtility, AuthUtility authUtility, MessageService messageService)
        {
            _systemConfigurationDao = systemConfigurationDao;
            _authUtility = authUtility;
            _userDao = userDao;
            _configuration = configuration;
            _configuration = configuration;
            _numberUtility = numberUtility;
            _mailUtility = mailUtility;
            _messageService = messageService;
        }

        [HttpPost("Test")]
        public async Task<IActionResult> Test()
        {
            return Ok(_messageService.GetSystemMessages().First(x => x.Code == "1000"));
        }


            [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginControllerInputDto inputDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var output = this.Output(ResultCd.SUCCESS).Create<LoginControllerOutputDto>();

                var user = await _userDao.GetAccountLogin(inputDto);

                if (user?.user == null || user?.ResultCd == 0)
                {
                    return NotFound(user);
                }

                var isUpdate = await _userDao.updatelastLogin(user?.user?.User.UserId);
                if (isUpdate.ResultCd == ResultCd.FAILURE)
                {
                    return BadRequest(isUpdate);
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Authentication:SecretForKey"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                      new Claim("ID", user.user.User.UserId.ToString()),
                      new Claim(ClaimTypes.Role, user.user.Role.RoleId.ToString())
                    }),
                    IssuedAt = DateTime.UtcNow,
                    Issuer = _configuration["Authentication:Issuer"],
                    Audience = _configuration["Authentication:Audience"],
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                output.accessToken = tokenHandler.WriteToken(token);
                return Ok(output);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }

        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm(ConfirmInputDto code)
        {
            var output = this.Output(ResultCd.SUCCESS).Create<ConfirmOutputDto>();
            var c = await _userDao.CheckConfirmCode(code);
            output = c;
            if (c.ResultCd != ResultCd.SUCCESS)
            {
                return BadRequest(output);
            }
            return Ok(output);

        }

        [HttpPost("reSendVerifyCode")]
        public async Task<IActionResult> ReSendVerifyCode(ReSendVerifyCodeInputDto inputDto)
        {
            var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();

            var isVerify = await _systemConfigurationDao.GetValue("0002");

            string verifyCodeHash = null;
            if (isVerify != null && isVerify == "1")
            {
                var verifyCode = _numberUtility.GenerateVerificationCode(6);
                SendMailDTO sendMail = new()
                {
                    FromEmail = _configuration["SysMail:Email"],
                    Password = _configuration["SysMail:Password"],
                    ToEmail = inputDto.Email,
                    Subject = "Verify code",
                    Body = verifyCode,
                };

                if (!await _mailUtility.SendEmail(sendMail))
                {
                    string[] a = { inputDto.Email };
                    output = output.Output(ResultCd.FAILURE).CommonMessageWithInfo("Send verify code to {0} fail.", "0002", a).Create<BaseOutputDto>();
                    return Ok(output);
                }
                verifyCodeHash = BCrypt.Net.BCrypt.HashPassword(verifyCode, BCrypt.Net.BCrypt.GenerateSalt());
            }

            var c = await _userDao.UpdateVerifyCode(inputDto, verifyCodeHash);
            if (c.ResultCd != ResultCd.SUCCESS)
            {
                return BadRequest(output);
            }
            return Ok(output);

        }

        [HttpPost("createNewPassword")]
        public async Task<IActionResult> CreateNewPassword(CreateNewPasswordInputDto inputDto)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<ConfirmOutputDto>();
                output.isSuccess = false;
                var checkVerify = await _userDao.CheckResetPassword(inputDto);

                if(checkVerify.ResultCd != ResultCd.SUCCESS)
                {
                    return BadRequest(checkVerify);
                }

                var salt = BCrypt.Net.BCrypt.GenerateSalt();
                inputDto.Password = BCrypt.Net.BCrypt.HashPassword(inputDto.Password, salt);

                var updateAcc = await _userDao.UpdaetPassword(inputDto);
                if(updateAcc.ResultCd != ResultCd.SUCCESS)
                {
                    return BadRequest(updateAcc);
                }
                output.isSuccess = true;

                return Ok(output);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


        [HttpPost("confirmResetPassword")]
        public async Task<IActionResult> ConfirmResetPassword(ConfirmInputDto inputDto)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<ConfirmOutputDto>();
                var c = await _userDao.CheckConfirmCodeResetPassword(inputDto);
                output = c;
                if (c.ResultCd != ResultCd.SUCCESS)
                {
                    return BadRequest(output);
                }
                return Ok(output);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordInputDto inputDto)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var id = _authUtility.GetIdInHeader(token);


                var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();
                var c = await _userDao.CheckPassword(id,inputDto);
                if (c.ResultCd != ResultCd.SUCCESS)
                {
                    return Ok(c);
                }

                var salt = BCrypt.Net.BCrypt.GenerateSalt();
                inputDto.NewPassword = BCrypt.Net.BCrypt.HashPassword(inputDto.NewPassword, salt);

                var updateAcc = await _userDao.UpdaetPasswordById(id, inputDto);
                output = updateAcc;
                if (updateAcc.ResultCd != ResultCd.SUCCESS)
                {
                    return BadRequest(updateAcc);
                }
                return Ok(output);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordInputDto inputDto)
        {
            try
            {

                var output = this.Output(ResultCd.SUCCESS).Create<CheckAccountAlredyExistsOutputDto>();
                var user = await _userDao.GetAccountByEmial(inputDto);
                if (user.ResultCd != ResultCd.SUCCESS)
                {
                    return BadRequest(output);
                }

                var isVerify = await _systemConfigurationDao.GetValue("0002");

                string verifyCodeHash = null;
                if (isVerify != null && isVerify == "1")
                {
                    var verifyCode = _numberUtility.GenerateVerificationCode(6);
                    SendMailDTO sendMail = new()
                    {
                        FromEmail = _configuration["SysMail:Email"],
                        Password = _configuration["SysMail:Password"],
                        ToEmail = user.userProfile.Email,
                        Subject = "Verify code",
                        Body = verifyCode,
                    };

                    if (!await _mailUtility.SendEmail(sendMail))
                    {
                        string[] a = { inputDto.Email };
                        output = output.Output(ResultCd.FAILURE).CommonMessageWithInfo("Send verify code to {0} fail.", "0002", a).Create<CheckAccountAlredyExistsOutputDto>();
                        return Ok(output);
                    }
                    verifyCodeHash = BCrypt.Net.BCrypt.HashPassword(verifyCode, BCrypt.Net.BCrypt.GenerateSalt());
                }

                var requestReset = await _userDao.RequestResetPassword(user, verifyCodeHash);

                if (requestReset.ResultCd != ResultCd.SUCCESS)
                {
                    return BadRequest(requestReset);
                }
                return Ok(output);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterControllerInputDto inputDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();

                var checkAccount = await _userDao.CheckAccountAlredyExists(inputDto);

                if (checkAccount.ResultCd != ResultCd.SUCCESS)
                {
                    return BadRequest(output);
                }

                if (checkAccount.isExist == true)
                {
                    string[] a = { inputDto.Email };
                    output = this.Output(ResultCd.FAILURE).CommonMessageWithInfo("Account {0} alredy exists.", "0001", a).Create<BaseOutputDto>();
                    return Ok(output);
                }

                var isVerify = await _systemConfigurationDao.GetValue("0002");

                string verifyCodeHash = null;
                if (isVerify != null && isVerify == "1")
                {
                    var verifyCode = _numberUtility.GenerateVerificationCode(6);
                    SendMailDTO sendMail = new()
                    {
                        FromEmail = _configuration["SysMail:Email"],
                        Password = _configuration["SysMail:Password"],
                        ToEmail = inputDto.Email,
                        Subject = "Verify code",
                        Body = verifyCode,
                    };

                    if (!await _mailUtility.SendEmail(sendMail))
                    {
                        string[] a = { inputDto.Email };
                        output = output.Output(ResultCd.FAILURE).CommonMessageWithInfo("Send verify code to {0} fail.", "0002", a).Create<BaseOutputDto>();
                        return Ok(output);
                    }
                    verifyCodeHash = BCrypt.Net.BCrypt.HashPassword(verifyCode, BCrypt.Net.BCrypt.GenerateSalt());
                }



                var salt = BCrypt.Net.BCrypt.GenerateSalt();
                inputDto.Password = BCrypt.Net.BCrypt.HashPassword(inputDto.Password, salt);

                var addAccount = await _userDao.RegisterAccount(inputDto, verifyCodeHash, isVerify);
                output = addAccount;

                return Ok(output);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPost()]
        public async Task<IActionResult> checkLogin()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var id = _authUtility.GetIdInHeader(token);
            return Ok(id);
        }
    }
}
