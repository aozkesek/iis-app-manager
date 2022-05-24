using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Org.Apps.SystemManager.Presentation.Base
{
    public enum MaintenanceOperation
    {
        Deploy = 0,
        Undeploy = 1,
        Backup = 2,
        Restore = 3,
        Upgrade = 4
    }
}