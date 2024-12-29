using System.Text;

namespace BE_AuctionAOT.Common.Utility
{
    public class Number
    {
        public string GenerateVerificationCode(int length)
        {
            const string chars = "0123456789";
            StringBuilder result = new StringBuilder(length);
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }
    }
}
