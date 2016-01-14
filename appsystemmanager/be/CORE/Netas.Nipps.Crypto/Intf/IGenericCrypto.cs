namespace Netas.Nipps.Crypto.Intf
{
    public interface IGenericCrypto
    {
        string EncryptUserPassword(string userId, string passWord);
    }
}