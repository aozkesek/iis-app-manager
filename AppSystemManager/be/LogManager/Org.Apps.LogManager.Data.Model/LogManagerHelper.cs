using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Org.Apps.LogManager.Data.Model
{
    public enum AppsModuleStatus
    {
        Running = 0,
        Stopped = 1,
        Unknown = 2
    }

    public sealed class AppsModuleStatusHelper
    {
        public static string ToString(AppsModuleStatus s)
        {
            switch (s)
            {
                case AppsModuleStatus.Running: return "Running";
                case AppsModuleStatus.Stopped: return "Stopped";
                
                default: return "Unknown";
            }
        }
    }


}
