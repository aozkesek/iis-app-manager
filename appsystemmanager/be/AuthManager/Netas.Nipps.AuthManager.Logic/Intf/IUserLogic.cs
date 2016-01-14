
using Netas.Nipps.BaseDao;
using Netas.Nipps.AuthManager.Data.Model;

namespace Netas.Nipps.AuthManager.Logic.Intf
{
    public interface IUserLogic : IGenericLogic<User>
    {
        bool ValidatePassword(string userName, string passWord);
        void SetPassword(string userName, string passWord);
        void UpdatePassword(string userName, string passWord, string newPassWord);
        void IssueNewPassword(string userName);
        string GeneratePassword(); 

    }
}
