using System.Net.Mail;
using System.Net;
using BE_AuctionOT_Cronjob.Common.Base.Entity;

namespace BE_AuctionOT_Cronjob.Common.Utility
{
    public class Mail
    {
        public async Task<bool> SendEmail(SendMailDTO sendMailDTO)
        {
            MailMessage mail = new MailMessage();

            mail.From = new MailAddress(sendMailDTO.FromEmail);
            mail.To.Add(sendMailDTO.ToEmail);
            mail.Subject = sendMailDTO.Subject;
            mail.Body = sendMailDTO.Body;

            using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
            {
                smtpClient.Credentials = new NetworkCredential(sendMailDTO.FromEmail, sendMailDTO.Password);
                smtpClient.EnableSsl = true;

                try
                {
                    smtpClient.Send(mail);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
