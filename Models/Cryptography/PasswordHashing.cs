using System;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace cafeInformationSystem.Models.Cryptography
{
    public class PasswordHashing
    {
        public const int DEGREE_OF_PARALLELISM = 1;
        public const int MEMORY_SIZE = 64 * 1024;  // 64 мегабайт (Я прочитал рекомендую вообще 128)
        public const int ITERATIONS = 100;
        public const int MAX_LENGHT_PASSWORD = 255;  // Тоже самое должно стоять в Employee

        // (48 + 96) * 4/3 = 192 символа как в бд в формате Base64
        private const int SALT_SIZE = 48;  // байта
        private const int HASH_SIZE = 96;  // байта 

        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty");
            }

            byte[] salt = GetSalt(SALT_SIZE);

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                DegreeOfParallelism = DEGREE_OF_PARALLELISM,
                MemorySize = MEMORY_SIZE,
                Iterations = ITERATIONS,
                Salt = salt,
                KnownSecret = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("HOST_DB")!)
            };

            byte[] hash = argon2.GetBytes(HASH_SIZE);

            byte[] combinedBytes = new byte[salt.Length + hash.Length];

            Buffer.BlockCopy(salt, 0, combinedBytes, 0, salt.Length);
            Buffer.BlockCopy(hash, 0, combinedBytes, salt.Length, hash.Length);

            string base64Hash = Convert.ToBase64String(combinedBytes);

            // INFO! если буду менять параметры для хеширования, чтобы если что-то не так сразу в ошибку шло.
            if (base64Hash.Length > MAX_LENGHT_PASSWORD)
            {
                throw new InvalidOperationException(
                    $"Hash length {base64Hash.Length} exceeds maximum {MAX_LENGHT_PASSWORD}. " +
                    $"Configuration: Salt={SALT_SIZE}, Hash={HASH_SIZE}");
            }

            return base64Hash;
        }

        public static byte[] GetSalt(int saltSize)
        {
            byte[] salt = new byte[saltSize];
            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }
            return salt;
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            try
            {
                if (hashedPassword.Length > MAX_LENGHT_PASSWORD)
                {
                    return false;
                }


                byte[] hashBytes = Convert.FromBase64String(hashedPassword);

                int expectedSize = SALT_SIZE + HASH_SIZE;
                if (hashBytes.Length != expectedSize)
                {
                    return false;
                }

                byte[] salt = new byte[SALT_SIZE];
                byte[] storedHash = new byte[HASH_SIZE];

                Buffer.BlockCopy(hashBytes, 0, salt, 0, SALT_SIZE);
                Buffer.BlockCopy(hashBytes, SALT_SIZE, storedHash, 0, HASH_SIZE);

                var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
                {
                    DegreeOfParallelism = DEGREE_OF_PARALLELISM,
                    MemorySize = MEMORY_SIZE,
                    Iterations = ITERATIONS,
                    Salt = salt,
                    KnownSecret = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("HOST_DB")!)
                };

                byte[] computedHash = argon2.GetBytes(HASH_SIZE);

                return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
