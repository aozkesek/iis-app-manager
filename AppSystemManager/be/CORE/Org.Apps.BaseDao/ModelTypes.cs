namespace Org.Apps.BaseDao
{
    public enum Status
    {
        DELETED = 0,
        ACTIVE = 1
    }

    public sealed class StatusHelper
    {
        public static string ToString(Status s)
        {
            switch (s)
            {
                case Status.DELETED: return "Deleted";
                case Status.ACTIVE: return "Active";

                default: return "UnknownStatus";
            }
        }
    }

    public enum AppsLogLevel
    {
        Off = 0,
        Trace = 1,
        Debug = 2,
        Info = 3,
        Warn = 4,
        Error = 5,
        Fatal = 6
    }

    public sealed class AppsLogLevelHelper
    {
        public static string ToString(AppsLogLevel l)
        {
            switch (l)
            {
                case AppsLogLevel.Trace: return "Trace";
                case AppsLogLevel.Debug: return "Debug";
                case AppsLogLevel.Info: return "Info";
                case AppsLogLevel.Warn: return "Warn";
                case AppsLogLevel.Error: return "Error";
                case AppsLogLevel.Fatal: return "Fatal";

                default: return "Off";
            }
        }
    }
}