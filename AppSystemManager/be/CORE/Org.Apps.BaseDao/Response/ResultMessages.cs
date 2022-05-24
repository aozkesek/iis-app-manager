using System;

namespace Org.Apps.BaseDao.Model.Response
{
    [Serializable]
    public enum ResultMessages
    {
        REQUEST_INVALID_PARAMETER,

        RESPONSE_NO_DATA_FOUND,
        RESPONSE_INVALID_PASSWORD
    }

    [Serializable]
    public enum Result
    {
        OK = 0,
        SUCCESSWITHWARN = 1, //result is OK but something missed, to get more info check out the ResultMessage
        FAIL = -1
    }

    public static class ResultMessagesHelper
    {
        public static String ToString(ResultMessages rm)
        {
            switch (rm)
            {
                case ResultMessages.REQUEST_INVALID_PARAMETER:
                    return "Request: invalid parameter entered.";

                case ResultMessages.RESPONSE_NO_DATA_FOUND:
                    return "Response: no data found.";

                case ResultMessages.RESPONSE_INVALID_PASSWORD:
                    return "Response: Invalid password.";
            }
            return "unknown result messages.";
        }
    }
}