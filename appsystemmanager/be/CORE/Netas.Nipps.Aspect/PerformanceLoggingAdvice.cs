using NLog;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using System;
using System.Reflection;

namespace Netas.Nipps.Aspect
{
    [Serializable]
    public sealed class PerformanceLoggingAdviceAttribute : MethodInterceptionAspect
    {
        [NonSerialized]
        private static readonly Logger mLogger;

        static PerformanceLoggingAdviceAttribute()
        {
            if (!PostSharpEnvironment.IsPostSharpRunning)
                mLogger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);
        }

        public override void OnInvoke(MethodInterceptionArgs args)
        {
            DateTime startTime = DateTime.Now;
            args.Proceed();
            long duration = (DateTime.Now.Ticks - startTime.Ticks) / 10000;
            // give info only the process that takes more than 200 msec 
            if (duration > 200)
                mLogger.Info("{0}->{1} proceeded in {2} msec.", args.Instance, args.Method, duration);
        }
    }
}