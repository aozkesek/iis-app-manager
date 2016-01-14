using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netas.Nipps.LogManager.Data.Model
{
    public enum NippsModuleStatus
    {
        Running = 0,
        Stopped = 1,
        Unknown = 2
    }

    public sealed class NippsModuleStatusHelper
    {
        public static string ToString(NippsModuleStatus s)
        {
            switch (s)
            {
                case NippsModuleStatus.Running: return "Running";
                case NippsModuleStatus.Stopped: return "Stopped";
                
                default: return "Unknown";
            }
        }
    }


}
