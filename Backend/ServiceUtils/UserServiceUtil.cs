using System.Security.Cryptography;
using Backend.Constants;
using Backend.Interfaces.ServiceUtils;

namespace Backend.ServiceUtils;

public class UserServiceUtil : ServiceUtilBase, IUserServiceUtil
{
    public string HashPassword(string passwordMd5)
    {
        var salt = RandomNumberGenerator.GetBytes(Cryptography.SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(passwordMd5, salt, Cryptography.Iterations, Cryptography.Algorithm, Cryptography.HashSize);

        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }
}