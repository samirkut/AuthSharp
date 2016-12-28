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
            long counter = (long)DateTime.UtcNow.Subtract(UNIX_EPOCH).TotalSeconds / 30;
            return GeneratePassword(counter);
        }

        public static int ValidSeconds
        {
            get { return 30 - (int)((DateTime.UtcNow - UNIX_EPOCH).TotalSeconds % 30); }
        }

        public static int MaxValidSeconds
        {
            get { return 30; }
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
            return GeneratePassword(Counter);
        }
    }

    public abstract class OTPGen
    {
        private readonly HMACSHA1 _hmac;

        protected OTPGen(string secret)
        {
            try
            {
                _hmac = new HMACSHA1(Base32.FromBase32String(secret));
            }
            catch(Exception ex)
            {
                //this can happen on invalid inputs for secrets..
                _hmac = null;
            }
        }

        public abstract string GetOTP();

        protected string GeneratePassword(long iterationNumber, int digits = 6)
        {
            if (_hmac == null)
            {
                return new string('-', digits);
            }

            byte[] counter = BitConverter.GetBytes(iterationNumber);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(counter);
            var hash = _hmac.ComputeHash(counter);
            int offset = hash[hash.Length - 1] & 0xf;
            int binary = ((hash[offset] & 0x7f) << 24) | ((hash[offset + 1] & 0xff) << 16) | ((hash[offset + 2] & 0xff) << 8) | (hash[offset + 3] & 0xff);
            int password = binary % (int)Math.Pow(10, digits); // 6 digits
            return password.ToString(new string('0', digits));
        }
    }
}
