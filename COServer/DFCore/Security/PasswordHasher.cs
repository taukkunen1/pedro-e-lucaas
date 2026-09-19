using System;
using System.Security.Cryptography;
using System.Text;

namespace Core.Security
{
    /// <summary>
    /// Hash de senha (PBKDF2-HMAC-SHA256, sal aleatorio por conta). Formato guardado:
    /// pbkdf2-sha256$iteracoes$salBase64$hashBase64.
    /// Contas antigas com senha em texto puro continuam validas ate o primeiro login bem-sucedido,
    /// quando a API troca a senha guardada pelo hash (needsUpgrade).
    /// </summary>
    public static class PasswordHasher
    {
        private const string Prefix = "pbkdf2-sha256";
        private const int Iterations = 210_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static bool IsHashed(string stored) => stored != null && stored.StartsWith(Prefix + "$", StringComparison.Ordinal);

        public static string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password ?? ""), salt, Iterations, HashAlgorithmName.SHA256, HashSize);
            return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        /// <summary>Confere a senha. needsUpgrade = true quando o valor guardado ainda era texto puro (ou tinha menos iteracoes).</summary>
        public static bool Verify(string stored, string password, out bool needsUpgrade)
        {
            needsUpgrade = false;
            if (string.IsNullOrEmpty(stored) || password == null) return false;
            if (!IsHashed(stored))
            {
                // legado: texto puro
                bool ok = CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(stored), Encoding.UTF8.GetBytes(password));
                needsUpgrade = ok;
                return ok;
            }
            string[] p = stored.Split('$');
            if (p.Length != 4 || !int.TryParse(p[1], out int iterations) || iterations < 1) return false;
            try
            {
                byte[] salt = Convert.FromBase64String(p[2]);
                byte[] expected = Convert.FromBase64String(p[3]);
                byte[] actual = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA256, expected.Length);
                bool ok = CryptographicOperations.FixedTimeEquals(actual, expected);
                needsUpgrade = ok && iterations < Iterations;
                return ok;
            }
            catch (FormatException) { return false; }
        }
    }
}
