using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace BizStream.Kentico.Xperience.AspNetCore.Mvc.Testing;

/// <summary> Implementation of a <see cref="WebApplicationFactory{TEntryPoint}"/> that supports an isolated Xperience Mvc application. </summary>
/// <typeparam name="TStartup"> A conventions-based Startup type. </typeparam>
/// <remarks> <see cref="IStartup"/> is not supported for <typeparamref name="TStartup"/>, due to a limitation of the Mvc.Testing framework. </remarks>
public class XperienceWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
{
    #region Fields
    private XperienceTestHost host;
    #endregion

    #region Properties

    /// <summary> The current <see cref="XperienceTestHost"/>. </summary>
    public XperienceTestHost Host => host;
    #endregion

    /// <inheritdoc/>
    protected override void ConfigureWebHost( IWebHostBuilder builder )
    {
        builder.UseContentRoot( AppDomain.CurrentDomain.BaseDirectory )
            .UseEnvironment( Environments.Development )
            .UseStaticWebAssets();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Overrides the default implementation to initialize an <see cref="XperienceTestHost"/>.
    /// The initialized host is immediately started.
    /// </remarks>
    protected override IHost CreateHost( IHostBuilder builder )
    {
        host = new XperienceTestHost( builder );

        host.Start();
        return host;
    }

    /// <inheritdoc/>
    protected override IHostBuilder CreateHostBuilder( )
    {
        return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(
                builder => builder.UseStartup<TStartup>()
                    .UseStaticWebAssets()
            );
    }
}