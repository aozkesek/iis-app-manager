namespace Org.Apps.Crypto.Intf
{
    public interface IGenericCrypto
    {
        string EncryptUserPassword(string userId, string passWord);
    }
}