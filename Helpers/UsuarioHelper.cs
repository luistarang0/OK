using System.Security.Claims;
using OK.Models;

namespace OK.Helpers
{
    public class UsuarioHelper
    {
        public static int? ObtenerIdUsuarioActual(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return null;

            if (int.TryParse(userIdClaim.Value, out int id))
                return id;

            return null;
        }
    }
}
