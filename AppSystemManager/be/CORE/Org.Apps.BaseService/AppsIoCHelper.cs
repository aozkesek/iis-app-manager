using Autofac;
using System;

namespace Org.Apps.BaseService
{
    public delegate void RegisterComponentFunction(ContainerBuilder containerBuilder);

    public delegate void InitializeComponentFunction();

    public static class AppsIoCHelper
    {
        private static IContainer mIoCContainer;
        private static Object mLockObject = new Object();

        public static IContainer IoCContainer { get { return mIoCContainer; } }

        public static void RegisterDao<T, I>(ContainerBuilder builder, int pageSize = 1000, string connectionName = "OrgIppsDb")
        {
            builder.RegisterType<T>()
                    .As<I>()
                    .SingleInstance()
                    .PropertiesAutowired()
                    .WithProperty("PageSize", pageSize)
                    .WithProperty("ConnectionName", connectionName);
        }

        public static void Register<T, I>(ContainerBuilder builder)
        {
            builder.RegisterType<T>()
                    .As<I>()
                    .SingleInstance()
                    .PropertiesAutowired();
        }

        public static void BuildContainer()
        {
            lock (mLockObject)
            {
                if (IoCContainer != null)
                    return;

                ContainerBuilder builder = new ContainerBuilder();

                if (RegisterComponents != null)
                    RegisterComponents(builder);

                mIoCContainer = builder.Build();

                if (InitializeComponents != null)
                    InitializeComponents();
            }
        }

        public static RegisterComponentFunction RegisterComponents { get; set; }

        public static InitializeComponentFunction InitializeComponents { get; set; }
    }
}