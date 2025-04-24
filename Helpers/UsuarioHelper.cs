using System.Security.Claims;
using OK.Models;

namespace OK.Helpers
{
    public class UsuarioHelper
    {
        public static int ObtenerIdUsuarioActual(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                throw new Exception("No se encontró el ID del usuario en los claims.");

            return int.Parse(userIdClaim.Value);
        }
    }
}
