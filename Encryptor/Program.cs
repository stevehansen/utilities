using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

namespace Encryptor
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            AnsiConsole.Render(new FigletText("Encryptor").Color(Color.Yellow));

            try
            {
                var path = configuration["Path"];
                if (string.IsNullOrEmpty(path))
                    path = AnsiConsole.Ask<string>("Enter the [green]path[/]:");

                var data = File.ReadAllBytes(path);

                // salt, utf8 string
                var salt = configuration["Salt"];
                var hasSalt = salt != null;
                if (!hasSalt)
                    salt = GetSecureRandomString(40);
                // password, utf8 string
                var password = configuration["Password"];
                var iterations = int.Parse(configuration["Iterations"]);

                var base64Data = EncryptSignature(salt, password, iterations, data);
                var encryptedPath = Path.ChangeExtension(path, ".enc.txt");
                File.WriteAllText(encryptedPath, base64Data);

                var table = new Table();

                table.AddColumn("Key").AddColumn("Value");

                if (!hasSalt)
                    table.AddRow("Salt", salt);

                table.AddRow("Encrypted length", base64Data.Length.ToString());
                table.AddRow("Encrypted path", encryptedPath);

                AnsiConsole.Render(table);
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
        }

        /// <summary>
        /// Gets a cryptographic secure (using <see cref="RNGCryptoServiceProvider"/>) random string composed of <paramref name="length"/> bytes encoded as url safe base64.
        /// </summary>
        /// <param name="length">The number of random bytes.</param>
        private static string GetSecureRandomString(int length = 32)
        {
            using var rng = new RNGCryptoServiceProvider();
            var data = new byte[length];
            rng.GetBytes(data);

            // https://en.wikipedia.org/wiki/Password_strength#Random_passwords
            return ToSafeBase64String(data);
        }

        /// <summary>
        /// Converts the <paramref name="data"/> to a base64 string that is url safe (no + / =)
        /// </summary>
        private static string ToSafeBase64String(byte[] data)
        {
            return Convert.ToBase64String(data).Replace('+', '-').Replace('/', '_').Replace("=", null);
        }

        private static string EncryptSignature(string salt, string password, int iterations, byte[] data)
        {
            byte[] key;
            byte[] iv;
            using (var keyGenerator = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), iterations))
            {
                key = keyGenerator.GetBytes(32);
                iv = keyGenerator.GetBytes(16);
            }

            byte[] encrypted;
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC; // same as for encryption
                aes.Padding = PaddingMode.PKCS7;

                using (var cryptoTransform = aes.CreateEncryptor())
                    encrypted = cryptoTransform.TransformFinalBlock(data, 0, data.Length);
            }

            return Convert.ToBase64String(encrypted);
        }
    }
}
