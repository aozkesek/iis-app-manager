using System;
using System.Reflection;

namespace Org.Apps.LicenseManager.Service
{
    public sealed class LicenseConfig : MarshalByRefObject
    {
        public static readonly string DllName = "Org.Apps.License.dll";
        
        private static readonly string _TypeName = "Org.Apps.License.Demo";

        private Assembly _LicenseDllAssembly = null;
        private Type _DemoType = null;
        private PropertyInfo _StartOf = null;
        private PropertyInfo _ValidFor = null;
        private PropertyInfo _Ticks = null;

        public void LoadFrom(string basePath)
        {
            _LicenseDllAssembly = Assembly.LoadFrom(basePath + DllName);
            _DemoType = _LicenseDllAssembly.GetType(_TypeName);
            _StartOf = _DemoType.GetProperty("StartOf");
            _ValidFor = _DemoType.GetProperty("ValidFor");
            _Ticks = _DemoType.GetProperty("Ticks");
        }

        public long StartOf()
        {
            if (_StartOf == null)
                throw new AccessViolationException(DllName); ;
            
            return (long)_StartOf.GetGetMethod().Invoke(null, null);
        }

        public long ValidFor()
        {
            if (_ValidFor == null)
                throw new AccessViolationException(DllName); ;
            
            return (long)_ValidFor.GetGetMethod().Invoke(null, null);
        }

        public long Ticks()
        {
            if (_Ticks == null)
                throw new AccessViolationException(DllName); ;

            long ticks = (long)_Ticks.GetGetMethod().Invoke(null, null);
            long nowTicks = DateTime.Now.Ticks;

            if (ticks > nowTicks)
                throw new AccessViolationException(DllName); ;

            return nowTicks;
        }

        public static string Source(long validFor)
        {
            return @" 
                    using System;
                    namespace Org.Apps.License
                    {
                        public sealed class Demo : MarshalByRefObject
                        {
                            public static long ValidFor { get { return /_VALIDFOR_/; } }
                            public static long StartOf { get { return /_STARTOF_/; } }
                            public static long Ticks { get { return /_TICKS_/; } }
                        }
                    }
                    "
                .Replace("/_STARTOF_/", WebApiApplication.StartOf.ToString())
                .Replace("/_VALIDFOR_/", validFor.ToString())
                .Replace("/_TICKS_/", DateTime.Now.Ticks.ToString());
        }
    }
}