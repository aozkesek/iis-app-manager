using System;
using System.Web.Mvc;
using System.Configuration;

using Netas.Nipps.BaseDao.Model.Response;
using Netas.Nipps.AuthManager.Data.Model;

namespace Netas.Nipps.SystemManager.Presentation.Helpers
{
    public sealed class CommonHelper
    {
        public const string InvalidPasswordExceptionMessage = "Netas.Nipps.BaseDao.InvalidPasswordException";
        public const string NoDataFoundExceptionMessage = "Netas.Nipps.BaseDao.NoDataFoundException";
        public const string ExpiredPasswordExceptionMessage = "Netas.Nipps.BaseDao.PasswordExpiredException";

        //this part needs to be change!!! on app_start, get key/value from appconfig then add them to a collection 
        private static string mAuthManagerServiceUrl = ConfigurationManager.AppSettings["AuthManagerServiceUrl"];
        private static string mConfigManagerServiceUrl = ConfigurationManager.AppSettings["ConfigManagerServiceUrl"];
        private static string mLogManagerServiceUrl = ConfigurationManager.AppSettings["LogManagerServiceUrl"];
        private static string mDeployManagerServiceUrl = ConfigurationManager.AppSettings["DeployManagerServiceUrl"];
        private static string mLicenseManagerServiceUrl = ConfigurationManager.AppSettings["LicenseManagerServiceUrl"];

        public static string AuthManagerServiceUrl { get { return mAuthManagerServiceUrl; } }
        public static string ConfigManagerServiceUrl { get { return mConfigManagerServiceUrl; } }
        public static string LogManagerServiceUrl { get { return mLogManagerServiceUrl; } }
        public static string DeployManagerServiceUrl { get { return mDeployManagerServiceUrl; } }
        public static string LicenseManagerServiceUrl { get { return mLicenseManagerServiceUrl; } }
        //need to be change!!!

        public static bool CheckExpiredPasswordException(Exception ex)
        {
            return CheckExpiredPasswordException(ex.Message);
        }

        public static bool CheckExpiredPasswordException(string exMessage)
        {
            return exMessage.StartsWith(ExpiredPasswordExceptionMessage);
        }
        
        public static bool CheckInvalidPasswordException(Exception ex)
        {
            return CheckInvalidPasswordException(ex.Message);
        }
        
        public static bool CheckInvalidPasswordException(string exMessage)
        {
            return exMessage.StartsWith(InvalidPasswordExceptionMessage);
        }
        
        public static bool CheckNoDataFoundException(Exception ex)
        {
            return CheckNoDataFoundException(ex.Message);
        }

        public static bool CheckNoDataFoundException(string exMessage)
        {
            return exMessage.StartsWith(NoDataFoundExceptionMessage);
        }

        public static object GetSessionVariable(Controller controller, string key, object defaultValue)
        {
            object value = controller.Session[key];
            if (value == null)
                return defaultValue;
            return value;
        }
        public static bool IsIppsAdmin(Controller controller)
        {
            User user = (User)controller.Session["User"];
            if (user.UserName.Equals("ippsadmin"))
                return true;
            return false;
        }

        public static string GetValidationClassName(ModelStateDictionary modelStates, string name)
        {
            if (modelStates != null && modelStates.Count > 0)
            {
                ModelState ms = modelStates[name];
                if (ms != null && ms.Errors != null && ms.Errors.Count > 0)
                    if (!string.IsNullOrEmpty(ms.Errors[0].ErrorMessage))
                        return "field-validation-error";
            }
                    
            return "field-validation-valid";
        }

        public static string GetValidationErrorMessages(ModelStateDictionary modelStates, string name)
        {
            if (modelStates != null && modelStates.Count > 0)
            {
                ModelState ms = modelStates[name];
                if (ms != null && ms.Errors != null)
                {
                    string em = "";
                    foreach (ModelError me in ms.Errors)
                        em += me.ErrorMessage + ". ";
                    return em;
                }
            }
                
            return "";
        }

    }
}