using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Core.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static List<string> Claims(this ClaimsPrincipal claimsPrincipal, string claimType)
        {
            var result = claimsPrincipal?.FindAll(claimType)?.Select(x => x.Value).ToList();
            return result;
        }

        public static List<string> ClaimRoles(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal?.Claims(ClaimTypes.Role);
        }

        public static string GetUserEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value
                ?? user.FindFirst("email")?.Value
                ?? user.FindFirst("Email")?.Value
                ?? user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value
                ?? string.Empty;
        }
        // 1. Integer ID'yi döndüren metod
        public static int GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            var userId = claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userId != null ? int.Parse(userId) : 0;
        }

        // 2. Guid (RowGuid) değerini döndüren metod
        public static Guid GetUserRowGuid(this ClaimsPrincipal claimsPrincipal)
        {
            var rowGuid = claimsPrincipal?.FindFirst("RowGuid")?.Value
                          ?? claimsPrincipal?.FindFirst("rowguid")?.Value;

            return rowGuid != null ? Guid.Parse(rowGuid) : Guid.Empty;
        }
    }
}
