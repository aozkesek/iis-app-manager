using NLog;
using PostSharp.Aspects;
using System;
using System.Reflection;

namespace Org.Apps.Aspect
{
    [Serializable]
    public sealed class TraceAttribute : OnMethodBoundaryAspect
    {
        private readonly string _message;

        [NonSerialized]
        private static Logger _logger;

        [NonSerialized]
        private string _enteringMessage;

        [NonSerialized]
        private string _exitingMessage;

        public TraceAttribute()
        {
        }

        public TraceAttribute(string message)
        {
            this._message = message;
        }

        public override void RuntimeInitialize(MethodBase method)
        {
            string methodName = method.DeclaringType.FullName + "." + method.Name;

            this._enteringMessage = "Entering " + methodName;
            this._exitingMessage = "Exiting " + methodName;

            _logger = LogManager.GetLogger(method.DeclaringType.Name);
        }

        public override void OnEntry(MethodExecutionArgs args)
        {
            if (String.IsNullOrEmpty(this._message))
            {
                _logger.Trace(this._enteringMessage);
            }
            else
            {
                _logger.Trace(this._enteringMessage + "\nMessage: " + this._message);
            }
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            if (String.IsNullOrEmpty(this._message))
            {
                _logger.Trace(this._exitingMessage);
            }
            else
            {
                _logger.Trace(this._exitingMessage + "\nMessage: " + this._message);
            }
        }

        public override void OnException(MethodExecutionArgs args)
        {
            _logger.Error(args.Exception);
        }
    }
}