using System;

namespace Org.Apps.BaseDao
{
    #region helpers

    public enum LogicMessage
    {
        ADMIN_CAN_NOT_CHANGE_OWN_USERID,
        NON_ADMIN_CAN_NOT_CHOOSE_ADMIN_USERID,
        ADMIN_USER_MUST_BE_ADDED_BY_AUTOMATIC_SCRIPT,
        ADMIN_USER_CAN_NOT_DELETED,
        NOT_FOUND_AN_ACTIVE_RECORD,
        PASSWORD_EXPIRED,
        PASSWORD_IS_INVALID
    }

    public static class LogicMessageHelper
    {
        public static String ToString(LogicMessage gm)
        {
            switch (gm)
            {
                case LogicMessage.ADMIN_CAN_NOT_CHANGE_OWN_USERID:
                    return "Admin kullanıcısı, kullanıcı kodunu değiştiremez.";

                case LogicMessage.NON_ADMIN_CAN_NOT_CHOOSE_ADMIN_USERID:
                    return "Admin kullanıcı kodunu alamazsınız.";

                case LogicMessage.ADMIN_USER_MUST_BE_ADDED_BY_AUTOMATIC_SCRIPT:
                    return "Admin kullanıcısı ancak kurulum scripti ile eklenebilir.";

                case LogicMessage.ADMIN_USER_CAN_NOT_DELETED:
                    return "Admin kullanıcısını silemezsiniz.";

                case LogicMessage.NOT_FOUND_AN_ACTIVE_RECORD:
                    return "Aktif durumda bir kayıt bulunmamaktadır.";

                case LogicMessage.PASSWORD_EXPIRED:
                    return "Şifre kullanım zamanı geçmiş.";

                case LogicMessage.PASSWORD_IS_INVALID:
                    return "Şifre hatalı girildi.";
            }
            return "unknown messages.";
        }
    }

    [Serializable]
    public sealed class UserCanNotBeAdminException : GenericLogicException
    {
        public UserCanNotBeAdminException()
            : base(LogicMessageHelper.ToString(LogicMessage.NON_ADMIN_CAN_NOT_CHOOSE_ADMIN_USERID))
        {
        }
    }

    [Serializable]
    public sealed class AdminCanNotBeDeletedException : GenericLogicException
    {
        public AdminCanNotBeDeletedException()
            : base(LogicMessageHelper.ToString(LogicMessage.ADMIN_USER_CAN_NOT_DELETED))
        {
        }
    }

    [Serializable]
    public sealed class AdminCanNotBeChangedException : GenericLogicException
    {
        public AdminCanNotBeChangedException()
            : base(LogicMessageHelper.ToString(LogicMessage.ADMIN_CAN_NOT_CHANGE_OWN_USERID))
        {
        }
    }

    [Serializable]
    public sealed class PasswordExpiredException : GenericLogicException
    {
        public PasswordExpiredException()
            : base(LogicMessageHelper.ToString(LogicMessage.PASSWORD_EXPIRED))
        {
        }
    }

    [Serializable]
    public sealed class RecordNotFoundException : GenericLogicException
    {
        public RecordNotFoundException()
            : base(LogicMessageHelper.ToString(LogicMessage.NOT_FOUND_AN_ACTIVE_RECORD))
        {
        }
    }

    [Serializable]
    public sealed class InvalidPasswordException : GenericLogicException
    {
        public InvalidPasswordException()
            : base(LogicMessageHelper.ToString(LogicMessage.PASSWORD_IS_INVALID))
        {
        }
    }

    [Serializable]
    public class GenericLogicException : Exception
    {
        public GenericLogicException()
            : base()
        {
        }

        public GenericLogicException(String message)
            : base(message)
        {
        }

        public GenericLogicException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    #endregion helpers
}