using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BE_AuctionAOT.Common.Utility
{
    public class AuthUtility
    {
        public long GetIdInHeader(string token)
        {

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var jwtToken = handler.ReadJwtToken(token);

                // Lấy claim "ID" từ token
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "ID");
                if (userIdClaim == null)
                {
                    Console.WriteLine("Invalid token: ID claim is missing.");
                    return -1;
                }

                // Chuyển đổi id từ chuỗi sang int
                if (long.TryParse(userIdClaim.Value, out long userId))
                {
                    return userId;
                }
                else
                {
                    Console.WriteLine("Invalid token: ID claim is not a valid integer.");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while reading token: {ex.Message}");
                return -1;
            }
        }
        public string GetRoleInHeader(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            try
            {
                var jwtToken = handler.ReadJwtToken(token);

                // Lấy claim "ID" từ token
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role");
                if (userIdClaim == null)
                {
                    Console.WriteLine("Invalid token: ID claim is missing.");
                    return null;
                }

                if (userIdClaim.Value != null)
                {
                    return userIdClaim.Value.ToString();
                }
                else
                {
                    Console.WriteLine("Invalid token: Type claim is missing.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while reading token: {ex.Message}");
                return null;
            }
        }
    }
}
