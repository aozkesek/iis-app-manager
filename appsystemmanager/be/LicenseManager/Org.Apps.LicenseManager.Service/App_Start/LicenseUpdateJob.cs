using System;
using Quartz;

namespace Org.Apps.LicenseManager.Service
{
    public class LicenseUpdateJob : IJob
    {

        public static readonly object LicenseUpdateJobLock = new object();

        public void Execute(IJobExecutionContext context)
        {
            lock (LicenseUpdateJobLock)
            {
                NLog.LogManager.GetCurrentClassLogger().Debug("compiling...");
                LicenseWrapper.LicenseCompile();
            }
        }
    }
}