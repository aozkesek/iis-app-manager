using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;

using NLog;

using Org.Apps.AuthManager.Logic.Intf;
using Org.Apps.AuthManager.Data.Model;

using Org.Apps.Aspect;
using Org.Apps.BaseDao;
using Org.Apps.EMail.Intf;
using Org.Apps.Crypto.Intf;

namespace Org.Apps.AuthManager.Logic.Impl
{
    public class UserLogic : IUserLogic
    {

        private static Logger mLogger = NLog.LogManager.GetCurrentClassLogger();
        private static Random RandomPasswordGenerator = new Random();

        private IEMailLogic mEmailLogic;
        private IGenericCrypto mPasswordCrypto;
        private IGenericDao<User> mUserDao;

        public IEMailLogic EmailLogic { get { return mEmailLogic; } set { mEmailLogic = value; } }
        public IGenericCrypto PasswordCrypto { get { return mPasswordCrypto; } set { mPasswordCrypto = value; } }
        public IGenericDao<User> UserDao { get { return mUserDao; } set { mUserDao = value; } }
        public Int32 PageSize { get { return UserDao.PageSize; } set { UserDao.PageSize = value; } }

        #region User Operations
        public User Get(Int32 id)
        {
            return UserDao.Get(id);
        }

        public User GetByName(String name)
        {
            return UserDao.GetByName(name);
        }

        [PerformanceLoggingAdviceAttribute]
        public void Add(User user)
        {
            bool serviceWantsToAddAdmin = user.UserName.Equals("ippsadmin");

            if (serviceWantsToAddAdmin)
                throw new GenericLogicException(LogicMessageHelper.ToString(LogicMessage.ADMIN_USER_MUST_BE_ADDED_BY_AUTOMATIC_SCRIPT));

            AddOrUpdateUser(user);
            
        }

        [PerformanceLoggingAdviceAttribute]
        public void Update(User user)
        {
            User oldUser = Get(user.UserId);
           
            bool nonAdminWantsToBeAdmin = !oldUser.UserName.Equals("ippsadmin") && user.UserName.Equals("ippsadmin");
            bool adminWantsToBeNonAdmin = oldUser.UserName.Equals("ippsadmin") && !user.UserName.Equals("ippsadmin");
            bool adminWantsToUpdateAdminStatus = oldUser.UserName.Equals("ippsadmin");

            if (adminWantsToBeNonAdmin)
                throw new AdminCanNotBeChangedException();
            else if (adminWantsToUpdateAdminStatus)
                throw new AdminCanNotBeDeletedException();
            else if (nonAdminWantsToBeAdmin)
                throw new UserCanNotBeAdminException();

            //upon changing of email or userid, generate a new password and notify the user
            bool generatePassword = !oldUser.Email.Equals(user.Email) || !oldUser.UserName.Equals(user.UserName);

            oldUser.UserName = user.UserName;
            oldUser.FirstName = user.FirstName;
            oldUser.LastName = user.LastName;
            oldUser.Email = user.Email;

            AddOrUpdateUser(oldUser, true, generatePassword);
            
        }

        [PerformanceLoggingAdviceAttribute]
        public void Remove(User user)
        {
            if (user.UserName == null || user.UserName.Equals(""))
                throw new ArgumentNullException("UserId is null.");

            if (user.UserName.Equals("ippsadmin"))
                throw new AdminCanNotBeDeletedException();

            User removeUser = GetByName(user.UserName);
            UserDao.Remove(removeUser);

        }

        public List<User> List()
        {
            List<User> users = UserDao.List();
            //do not show ippsadmin to the user
            User ippsAdmin = users.Where(u => u.UserName.Equals("ippsadmin")).Single();
            users.Remove(ippsAdmin);
            return users;
        }

        public List<User> List(Int32 pageNo)
        {
            List<User> users = UserDao.List(pageNo);
            //do not show ippsadmin to the user
            User ippsAdmin = users.Where(u => u.UserName.Equals("ippsadmin")).Single();
            users.Remove(ippsAdmin);
            return users;
        }

        [PerformanceLoggingAdviceAttribute]
        public void IssueNewPassword(string userName)
        {
            User user = UserDao.GetByName(userName);
            
            AddOrUpdateUser(user, true);
        }

        [PerformanceLoggingAdviceAttribute]
        public bool ValidatePassword(string userName, string passWord)
        {
            User user = UserDao.GetByName(userName);

            ValidateUserPassword(user, passWord);

            return true;
        }

        [PerformanceLoggingAdviceAttribute]
        public void SetPassword(string userName, string passWord)
        {
            User user = UserDao.GetByName(userName);

            HashPassword(user, passWord);
            
            UserDao.Update(user);
        
        }

        [PerformanceLoggingAdviceAttribute]
        public void UpdatePassword(string userName, string passWord, string newPassWord)
        {
            User user = UserDao.GetByName(userName);

            ValidateUserPassword(user, passWord);

            HashPassword(user, newPassWord);

            UserDao.Update(user);
    
        }

        public string GeneratePassword()
        {
            return GenerateRandomNumber();
        }
        
        #endregion

        #region Private Methods 
        private string GenerateRandomNumber()
        {
            return RandomPasswordGenerator.Next(100000, 999999).ToString();
        }

        private void HashPassword(User user, string passWord)
        {
            user.PasswordHash = PasswordCrypto.EncryptUserPassword(user.UserName, passWord);
            user.InvalidAttemptCount = 0;
            user.PasswordUpdateDate = DateTime.Now;
        }

        private void AddOrUpdateUser(User user, bool isUpdate = false, bool generatePassword = true)
        {
            string messagePart = "";

            if (generatePassword)
            {
                user.PasswordHash = GenerateRandomNumber();
                messagePart = string.Format("Kullanıcı Kodunuz: {0}\nŞifreniz: {1}\n\n", user.UserName, user.PasswordHash);
                HashPassword(user, user.PasswordHash);
            }

            if (isUpdate)
                UserDao.Update(user);
            else
                UserDao.Add(user);

            if (generatePassword)
            {
                //send an e-mail to the user to inform a new password generated and available to enter to the system 
                try
                {
                    EmailLogic.MessageTemplate = string.Format(ConfigurationManager.AppSettings.Get("PasswordNotificationTemplate").ToString(), DateTime.Now, "{0}");
                    EmailLogic.SendNewPasswordNotification(user.Email, messagePart);

                }
                catch (Exception ex)
                {
                    mLogger.Error(ex.ToString());
                }
                
            }
            
        }

        private bool ValidateUserPassword(User user, string passWord)
        {
            //newly created user is entering the system first time
            if (user.CreateDate.Equals(user.PasswordUpdateDate))
            {
                //is timed out?
                if (user.PasswordUpdateDate.AddHours(1) < DateTime.Now)
                {
                    throw new PasswordExpiredException();
                }
            }

            if (user.PasswordHash.Equals(PasswordCrypto.EncryptUserPassword(user.UserName, passWord)))
            {
                //update successfull attempt  
                user.LastSuccessAttempt = DateTime.Now;
                user.InvalidAttemptCount = 0;

                UserDao.Update(user);

                return true;
            }
            else
            {
                //update invalid attempt  
                user.LastInvalidAttempt = DateTime.Now;
                user.InvalidAttemptCount += 1;

                UserDao.Update(user);

                throw new InvalidPasswordException();
            }
        }

        #endregion

    }
}
