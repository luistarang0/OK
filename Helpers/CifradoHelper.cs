using System.Security.Cryptography;
using System.Text;

namespace OK.Helpers
{
    public class CifradoHelper
    {
        public static string Hash(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            // Hasheamos la contraseña ingresada
            var enteredPasswordHash = Hash(enteredPassword);

            // Comparamos el hash ingresado con el almacenado
            return enteredPasswordHash == storedHash;
        }
    }
}
