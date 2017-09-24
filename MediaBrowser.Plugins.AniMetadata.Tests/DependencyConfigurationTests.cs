using System;
using MediaBrowser.Common;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using NSubstitute;
using NUnit.Framework;
using SimpleInjector;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class DependencyConfigurationTests
    {
        private class TestContainer : IDependencyContainer
        {
            public TestContainer()
            {
                Container = new Container();

                Container.Register(() => Substitute.For<IApplicationPaths>());
                Container.Register(() => Substitute.For<IHttpClient>());
                Container.Register(() => Substitute.For<ILogManager>());

                TestPlugin.EnsurePluginStaticSingletonAvailable();
            }

            public Container Container { get; }

            public void RegisterSingleInstance<T>(T obj, bool manageLifetime = true) where T : class
            {
                Container.RegisterSingleton(obj);
            }

            public void RegisterSingleInstance<T>(Func<T> func) where T : class
            {
                Container.RegisterSingleton(func);
            }

            public void Register(Type typeInterface, Type typeImplementation)
            {
                Container.Register(typeInterface, typeImplementation);
            }
        }

        [Test]
        public void BindDependencies_ConfigurationIsValid()
        {
            var container = new TestContainer();

            var dependencyConfiguration = new DependencyConfiguration();

            dependencyConfiguration.BindDependencies(container);

            container.Container.Verify(VerificationOption.VerifyAndDiagnose);
        }
    }
}