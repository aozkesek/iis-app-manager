using NLog;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using System;
using System.Reflection;

namespace Org.Apps.Aspect
{
    [Serializable]
    public sealed class AuthorizationAdviceAttribute : OnMethodBoundaryAspect
    {
        [NonSerialized]
        private static readonly Logger mLogger;

        static AuthorizationAdviceAttribute()
        {
            if (!PostSharpEnvironment.IsPostSharpRunning)
                mLogger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);
        }

        public override void OnEntry(MethodExecutionArgs args)
        {
            mLogger.Debug("{0}->{1} authorized.", args.Instance, args.Method);
        }
    }
}