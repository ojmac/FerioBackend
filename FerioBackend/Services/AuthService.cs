using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace FerioBackend.Services
{
    public class AuthService
    {
        // Hashea la contraseña de manera segura
        public static string HashPassword(string password)
        {
           
            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Genera el hash de la contraseña con el salt usando PBKDF2
            var hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000, 
                numBytesRequested: 256 / 8 
            );

            
            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        //  Verifica la contraseña
        public static bool VerifyPasswordHash(string password, string storedHash)
        {
           
            var parts = storedHash.Split(':');
            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);

            
            var hashToCompare = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
            );

            
            return hash.SequenceEqual(hashToCompare);
        }

        
       

    }
}
