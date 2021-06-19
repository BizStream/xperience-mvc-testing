using System;
using System.Net.Http;
using System.Threading.Tasks;
using CMS.Base;
using CMS.Tests;
using Microsoft.AspNetCore.Hosting;
using NUnit.Framework;

namespace BizStream.Kentico.Xperience.AspNetCore.Mvc.Testing
{

    /// <summary> Allows integration tests to run in isolation with Mvc support. </summary>
    /// <typeparam name="TStartup"> A conventions-based Startup type. </typeparam>
    /// <remarks> <see cref="IStartup"/> is not supported for <typeparamref name="TStartup"/>, due to a limitation of the Mvc.Testing framework. </remarks>
    public abstract class AutomatedTestsWithIsolatedWebApplication<TStartup> : AutomatedTestsWithData
        where TStartup : class
    {
        #region Fields

        /// <summary> An instance of an <see cref="HttpClient"/> initialized from the current <see cref="Factory"/>. </summary>
        /// <remarks> This property is managed and initialized for each test run. It is not recommended to store a reference or manually dispose of this object. </remarks>
        protected HttpClient Client { get; private set; }

        /// <summary> An instance of <see cref="XperienceWebApplicationFactory{TStartup}"/>, initialized for the current test run. </summary>
        /// <remarks> This property is managed and initialized for each test run. It is not recommended to store a reference or manually dispose of this object. </remarks>
        /// <seealso cref="CreateWebApplicationFactory"/>
        protected XperienceWebApplicationFactory<TStartup> Factory { get; private set; }
        #endregion

        #region Properties

        /// <summary> An artificial delay added between tests. </summary>
        /// <seealso cref="AutomatedTestsWithIsolatedWebApplicationTearDown"/>
        protected virtual TimeSpan ArtificialDelay => TimeSpan.FromSeconds( 1.5 );
        #endregion

        /// <summary> Create an instance of an <see cref="XperienceWebApplicationFactory{TStartup}"/>. </summary>
        /// <remarks> Intended to be overriden in derived implementation to customize the factory instance. </remarks>
        protected virtual XperienceWebApplicationFactory<TStartup> CreateWebApplicationFactory( )
            => new XperienceWebApplicationFactory<TStartup>();

        /// <summary> Test OneTimeTearDown. </summary>
        /// <remarks> Ensures proper disposal of <see cref="Factory"/> and <see cref="Client"/>. </remarks>
        [OneTimeTearDown]
        public void AutomatedTestsWithIsolatedWebApplicationOneTimeTearDown( )
        {
            // ensure final disposal
            Client?.Dispose();
            Factory?.Dispose();
        }

        /// <summary> Test SetUp. </summary>
        /// <remarks> Initializes the <see cref="Factory"/> and <see cref="Client"/> for the new test run. </remarks>
        [SetUp]
        public void AutomatedTestsWithIsolatedWebApplicationSetUp( )
        {
            if( !IsFirstTestInFixture )
            {
                // we dispose here, rather then in the `TearDown`, to be consistent with `AutomatedTests`' design
                Client?.Dispose();
                Factory?.Dispose();
            }

            SystemContext.WebApplicationPhysicalPath = TemporaryAppPath;

            Factory = CreateWebApplicationFactory();
            Client = Factory.CreateClient();

            ApplicationInitialized = true;
        }

        /// <summary> Test TearDown. </summary>
        /// <remarks> Ensures shutdown of the current <see cref="XperienceWebApplicationFactory{TStartup}.Host"/> prior to the <see cref="Factory"/>'s disposal, and adds an artifical delay. </remarks>
        /// <seealso cref="AutomatedTestsWithIsolatedWebApplicationSetUp"/>
        /// <seealso cref="XperienceTestHost"/>
        [TearDown]
        public async Task AutomatedTestsWithIsolatedWebApplicationTearDown( )
        {
            await Factory?.Host?.StopAsync();

            // NOTE: the runtime occasionally throw IO related exceptions, suspected to be due to async IO..? regardless, an artificial delay seems to remedy the issue™..
            await Task.Delay( ArtificialDelay );
        }

        /// <summary> Not Supported, CMSApplication Initialization is handled by the Mvc Integration Framework. </summary>
        protected sealed override void InitApplication( )
            => throw new NotSupportedException();

        /// <summary> Not Supported, CMSApplication Pre-Initialization is handled by the Mvc Integration Framework. </summary>
        protected sealed override void PreInitApplication( )
            => throw new NotSupportedException();

        /// <summary> Not Supported, services are configured via the <typeparamref name="TStartup"/> type. </summary>
        protected sealed override void RegisterTestServices( )
            => throw new NotSupportedException();

    }

}
