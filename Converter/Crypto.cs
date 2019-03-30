using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace SQL_Generator_WPF.Converter
{
    internal static class Crypto
    {
        private const int PBKDF2IterCount = 1000;
        private const int PBKDF2SubkeyLength = 32;
        private const int SaltSize = 16;

        private const int BytesFetched = 32;

        public static void Test()
        {
            string pass = "u12345";
            // pass: q1234
            // pass: C7iwhhlk5KNNepfLypHN2w==
            // salt: T/VQP+e9tGEX9LGNyXxxBQ==
            // full: T/VQP+e9tGEX9LGNyXxxBQu4sIYZZOSjTXqXy8qRzds=

            string sa = "T/VQP+e9tGEX9LGNyXxxBQ==";
            Console.WriteLine(sa);
            byte[] salt = Convert.FromBase64String(sa);
            Console.WriteLine($@"Salt len: {salt.Length} => {ArrToString(salt)}");
            string hash = HashPassword(pass, salt);
            Console.WriteLine($@"Salt len: {salt.Length} => {ArrToString(salt)}");
            string ha3 = HashPassword("u12345", salt);

            Console.WriteLine(hash);

            string ha =  "AE/1UD/nvbRhF/Sxjcl8cQXBLl4sYZCW8Ci8QNLQ7vQRvcLYVxaRrPjDEV77GvaAIg==";
            string ha2 = "ABUmHoydPQ6QKS2MIf4aY+4JE22+s+4kOYlQ+I7rmGeqx8zUgQyAwgXS6CpW5tzhHA==";
            Console.WriteLine(@"Verify: "+VerifyHashedPassword(ha, pass));
            Console.WriteLine(@"Verify2: "+VerifyHashedPassword(ha2, pass));
           // Console.WriteLine(@"Verify Wrong: "+VerifyHashedPassword(ha, "q12345"));
           // Console.WriteLine(@"Verify Wrong2: "+VerifyHashedPassword(ha2, "q12345"));


            Console.WriteLine($@"Hash len: {ha3.Length} => {ha3}");
        }


        private static string HashPassword(string password, byte[] salt)
        {
            if (password == null)
                throw new ArgumentNullException("password");
            //byte[] salt;
            byte[] bytes;
            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 1000))
            {
                //salt = rfc2898DeriveBytes.Salt;
                bytes = rfc2898DeriveBytes.GetBytes(BytesFetched);
            }
            byte[] inArray = new byte[salt.Length + BytesFetched+1];
            Buffer.BlockCopy((Array) salt, 0, (Array) inArray, 1, salt.Length);
            Buffer.BlockCopy((Array) bytes, 0, (Array) inArray, salt.Length+1, BytesFetched);

            Console.WriteLine($@"Key len: {bytes.Length} => {ArrToString(bytes)}");
            Console.WriteLine($@"Key: {Convert.ToBase64String(bytes)}");

            return Convert.ToBase64String(inArray);
        }

        public static string HashPassword(string password)
        {
            if (password == null)
                throw new ArgumentNullException("password");
            byte[] salt;
            byte[] bytes;
            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, 16, 1000))
            {
                salt = rfc2898DeriveBytes.Salt;
                bytes = rfc2898DeriveBytes.GetBytes(BytesFetched);
            }
            byte[] inArray = new byte[49];
            Buffer.BlockCopy((Array) salt, 0, (Array) inArray, 1, 16);
            Buffer.BlockCopy((Array) bytes, 0, (Array) inArray, 17, 32);
            return Convert.ToBase64String(inArray);
        }


        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            if (hashedPassword == null)
                return false;
            if (password == null)
                throw new ArgumentNullException("password");
            byte[] numArray = Convert.FromBase64String(hashedPassword);
            if (numArray.Length != 49 || (int) numArray[0] != 0)
                return false;
            byte[] salt = new byte[16];
            Buffer.BlockCopy((Array) numArray, 1, (Array) salt, 0, 16);
            byte[] a = new byte[32];
            Buffer.BlockCopy((Array) numArray, 17, (Array) a, 0, 32);
            byte[] bytes;
            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 1000))
                bytes = rfc2898DeriveBytes.GetBytes(BytesFetched);
            return Crypto.ByteArraysEqual(a, bytes);
        }


        public static byte[] RandomBytes(int count)
        {
            byte[] b = new byte[count];
            var r = RandomNumberGenerator.Create();
            r.GetBytes(b);
            return b;
        }

        public static string ArrToString(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder(bytes.Length);
            builder.Append("[");
            for (var i = 0; i < bytes.Length; i++)
            {
                var b = bytes[i];
                builder.Append(b);
                if (i < bytes.Length-1)
                {
                    builder.Append(", ");
                }
            }
            builder.Append("]");
            return builder.ToString();
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (object.ReferenceEquals((object) a, (object) b))
                return true;
            if (a == null || b == null || a.Length != b.Length)
                return false;
            bool flag = true;
            for (int index = 0; index < a.Length; ++index)
                flag &= (int) a[index] == (int) b[index];
            return flag;
        }
    }
}