namespace Netas.Nipps.BaseDao
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

    public enum NippsLogLevel
    {
        Off = 0,
        Trace = 1,
        Debug = 2,
        Info = 3,
        Warn = 4,
        Error = 5,
        Fatal = 6
    }

    public sealed class NippsLogLevelHelper
    {
        public static string ToString(NippsLogLevel l)
        {
            switch (l)
            {
                case NippsLogLevel.Trace: return "Trace";
                case NippsLogLevel.Debug: return "Debug";
                case NippsLogLevel.Info: return "Info";
                case NippsLogLevel.Warn: return "Warn";
                case NippsLogLevel.Error: return "Error";
                case NippsLogLevel.Fatal: return "Fatal";

                default: return "Off";
            }
        }
    }
}