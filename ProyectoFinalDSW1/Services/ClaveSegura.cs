using System.Security.Cryptography;

namespace ProyectoFinalDSW1.Services
{
    // Encripta las contraseñas con PBKDF2 (hash + sal aleatoria). Nunca se guarda la clave real.
    public static class ClaveSegura
    {
        private const int Iteraciones = 210000;
        private const int TamanoSal = 16;
        private const int TamanoHash = 32;

        public static string Hashear(string clave)
        {
            var sal = RandomNumberGenerator.GetBytes(TamanoSal);
            var hash = Rfc2898DeriveBytes.Pbkdf2(clave, sal, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);
            return $"PBKDF2${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
        }

        public static bool Verificar(string clave, string claveHash)
        {
            try
            {
                var partes = claveHash.Split('$');
                if (partes.Length != 4 || partes[0] != "PBKDF2") return false;

                var iteraciones = int.Parse(partes[1]);
                var sal = Convert.FromBase64String(partes[2]);
                var hashGuardado = Convert.FromBase64String(partes[3]);

                var hashNuevo = Rfc2898DeriveBytes.Pbkdf2(clave, sal, iteraciones, HashAlgorithmName.SHA256, hashGuardado.Length);
                return CryptographicOperations.FixedTimeEquals(hashNuevo, hashGuardado);
            }
            catch
            {
                return false;
            }
        }
    }
}
