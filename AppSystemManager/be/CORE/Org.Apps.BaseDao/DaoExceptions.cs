using System;

namespace Org.Apps.BaseDao
{
    public enum GenericDaoMessage
    {
        NO_DATA_FOUND,
        TOO_MANY_RECORD_FOUND
    }

    public static class GenericDaoMessageHelper
    {
        public static String ToString(GenericDaoMessage gm)
        {
            switch (gm)
            {
                case GenericDaoMessage.NO_DATA_FOUND:
                    return "No data found.";

                case GenericDaoMessage.TOO_MANY_RECORD_FOUND:
                    return "Too many record found.";
            }
            return "unknown messages.";
        }
    }

    [Serializable]
    public sealed class NoDataFoundException : GenericDaoException
    {
        public NoDataFoundException()
            : base(GenericDaoMessageHelper.ToString(GenericDaoMessage.NO_DATA_FOUND))
        {
        }
    }

    [Serializable]
    public sealed class TooManyRecordFoundException : GenericDaoException
    {
        public TooManyRecordFoundException()
            : base(GenericDaoMessageHelper.ToString(GenericDaoMessage.TOO_MANY_RECORD_FOUND))
        {
        }
    }

    [Serializable]
    public class GenericDaoException : Exception
    {
        public GenericDaoException()
            : base()
        {
        }

        public GenericDaoException(String message)
            : base(message)
        {
        }

        public GenericDaoException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}