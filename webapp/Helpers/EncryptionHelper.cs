using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace webapp.Helpers
{
    public class EncryptionHelper
    {
        private readonly IDataProtector _protector;

        public EncryptionHelper(IDataProtectionProvider dataProtectionProvider)
        {
            _protector = dataProtectionProvider.CreateProtector("IdProtector");
        }

        public string Encrypt(string plainText)
        {
            return _protector.Protect(plainText);
        }

        public string Decrypt(string cipherText)
        {
            return _protector.Unprotect(cipherText);
        }
    }
}
