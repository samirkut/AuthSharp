using System;
using System.Security.Cryptography;
using System.Text;

namespace AuthSharp.Util
{
    public class TOTPGen : OTPGen
    {
        private static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public TOTPGen(string secret)
            : base(secret) { }

        public override string GetOTP()
        {
            long counter = (long)(DateTime.UtcNow - UNIX_EPOCH).TotalSeconds / 30;
            return GeneratePassword(_secret, counter);
        }

        public override int ValidSeconds()
        {
            return (int)((DateTime.UtcNow - UNIX_EPOCH).TotalSeconds % 30);
        }
    }

    public class HOTPGen : OTPGen
    {
        public long Counter { get; private set; }

        public HOTPGen(string secret, long counter)
            : base(secret)
        {
            Counter = counter;
        }

        public override string GetOTP()
        {
            Counter++;
            return GeneratePassword(_secret, Counter);
        }

        public override int ValidSeconds()
        {
            return -1;
        }
    }

    public abstract class OTPGen
    {
        protected readonly string _secret;

        protected OTPGen(string secret)
        {
            _secret = secret;
        }

        public abstract string GetOTP();

        /// <summary>
        /// Check how long the token is valid for
        /// </summary>
        /// <returns>-1 if its valid forever. Otherwise seconds</returns>
        public abstract int ValidSeconds();

        protected static string GeneratePassword(string secret, long iterationNumber, int digits = 6)
        {
            byte[] counter = BitConverter.GetBytes(iterationNumber);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(counter);
            var key = Encoding.ASCII.GetBytes(secret);
            var hmac = new HMACSHA1(key);
            var hash = hmac.ComputeHash(counter);
            int offset = hash[hash.Length - 1] & 0xf;
            int binary = ((hash[offset] & 0x7f) << 24) | ((hash[offset + 1] & 0xff) << 16) | ((hash[offset + 2] & 0xff) << 8) | (hash[offset + 3] & 0xff);
            int password = binary % (int)Math.Pow(10, digits); // 6 digits
            return password.ToString(new string('0', digits));
        }
    }
}
