using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.LicenseProvider;
using CMS.SiteProvider;
using CMS.WebFarmSync;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace BizStream.Kentico.Xperience.AspNetCore.Mvc.Testing;

/// <summary> Implementation of an <see cref="IHost"/> that manages an isolated CMS database instance. </summary>
public class XperienceTestHost : IHost
{
    #region Fields
    private readonly SqlConnectionStringBuilder connectionString;
    private readonly IHost host;
    private readonly Guid identifier;

    private bool disposed;
    #endregion

    #region Properties

    /// <summary> The connection string of the CMS database managed by the host. </summary>
    public string ConnectionString => connectionString.ConnectionString;

    /// <summary> A unique identifier of the host. </summary>
    public Guid Identifier => identifier;

    /// <inheritdoc/>
    public IServiceProvider Services => host.Services;
    #endregion

    public XperienceTestHost( IHostBuilder builder )
    {
        identifier = Guid.NewGuid();
        connectionString = new()
        {
            CurrentLanguage = "English",
            DataSource = @"(localdb)\mssqllocaldb",
            Encrypt = false,
            InitialCatalog = $"BZS_CMS_TEST_{identifier}",
            IntegratedSecurity = true,
            PersistSecurityInfo = false,
            MultipleActiveResultSets = true
        };

        host = builder.Build();

        ApplicationEvents.Finalize.Execute += OnFinalize;
        ApplicationEvents.PostStart.Execute += OnPostStart;

        // NOTE: PreInit does not fire until the host is started (when `ApplicationInitializerStartupFilter` set's Kentico's ServiceProvider to Mvc's)
        ApplicationEvents.PreInitialized.Execute += OnPreInit;
        ApplicationEvents.Initialized.Execute += OnInit;
    }

    protected virtual void Dispose( bool disposing )
    {
        if( disposed )
        {
            return;
        }

        if( disposing )
        {
            ApplicationEvents.Finalize.Execute -= OnFinalize;
            ApplicationEvents.Initialized.Execute -= OnInit;
            ApplicationEvents.PostStart.Execute -= OnPostStart;
            ApplicationEvents.PreInitialized.Execute -= OnPreInit;

            host?.Dispose();
        }

        disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose( )
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    private static void EnsureAnonymousTaskProcessorDisabled( )
    {
        var watcher = Assembly.GetAssembly( typeof( WebFarmContext ) )
            !.GetType( "CMS.WebFarmSync.AnonymousTasksProcessor" )
            !.GetProperty( "NotifyWatcher", BindingFlags.NonPublic | BindingFlags.Static )
            !.GetValue( null ) as FileSystemWatcher;

        watcher!.EnableRaisingEvents = false;
    }

    private void EnsureDatabaseDestroyed( )
    {
        // NOTE: if the db does not exist, but is specified as the `InitialCatalog`, the `SqlConnection` fails to `Open()`.
        var deletionConnectionString = new SqlConnectionStringBuilder( connectionString.ConnectionString )
        {
            InitialCatalog = string.Empty
        };

        if( !SqlInstallationHelper.DatabaseExists( connectionString.InitialCatalog, deletionConnectionString.ConnectionString ) )
        {
            return;
        }

        var error = SqlInstallationHelper.DeleteDatabase( connectionString.InitialCatalog, deletionConnectionString.ConnectionString );
        if( !string.IsNullOrEmpty( error ) )
        {
            throw new Exception( error );
        }
    }

    private void EnsureDatabaseInitialized( )
    {
        // NOTE: if the db does not exist, but is specified as the `InitialCatalog`, the `SqlConnection` fails to `Open()`.
        var creationConnectionString = new SqlConnectionStringBuilder( connectionString.ConnectionString )
        {
            InitialCatalog = string.Empty
        };

        if( SqlInstallationHelper.DatabaseExists( connectionString.InitialCatalog, creationConnectionString.ConnectionString ) )
        {
            return;
        }

        SqlInstallationHelper.CreateDatabase( connectionString.InitialCatalog, creationConnectionString.ConnectionString, null );

        var log = new StringBuilder();
        var settings = new DatabaseInstallationSettings
        {
            ApplyHotfix = true,
            ConnectionString = connectionString.ConnectionString,
            Logger = ( message, type ) => log.AppendLine( $"[{type}] {message}" ),
            ScriptsFolder = GetSQLScriptsPath()
        };

        if( !SqlInstallationHelper.InstallDatabase( settings ) )
        {
            throw new Exception( $"Failed to install test database:{Environment.NewLine}{log}" );
        }

        SetConnectionString( connectionString.ConnectionString );
    }

    private static void EnsureHashStringSalt( )
    {
        var settingService = Service.Resolve<IAppSettingsService>();

        var salt = settingService[ "CMSHashStringSalt" ];
        if( string.IsNullOrEmpty( salt ) )
        {
            // Copied from `CMS.Tests.AutomatedTestsWithLocalDB.SetConnectionAndHashSalt(DatabaseProperties)`
            settingService[ "CMSHashStringSalt" ] = "TestTestTestTest";
        }
    }

    private static void EnsureLicenseKey( )
    {
        var key = Service.Resolve<IConfiguration>()[ "CMSTestLicenseKey" ];
        if( string.IsNullOrWhiteSpace( key ) )
        {
            throw new Exception( "Cannot execute tests without a test LicenseKey. Ensure the `CMSTestLicenseKey` is set in `appsettings.json` or `appsetings.Development.json`." );
        }

        var license = new LicenseKeyInfo();
        license.LoadLicense( key, string.Empty );

        if( LicenseKeyInfoProvider.GetLicenseKeyInfo( license.Domain ) is null )
        {
            LicenseKeyInfoProvider.SetLicenseKeyInfo( license );
        }
    }

    private static void EnsureSite( )
    {
        var siteName = "NewSite";
        if( SiteInfo.Provider.Get( siteName ) != null )
        {
            return;
        }

        var site = new SiteInfo
        {
            DisplayName = "Mvc Integration Test Site",
            SiteName = siteName,
            Status = SiteStatusEnum.Running,
            DomainName = "localhost:51872",
            SitePresentationURL = "http://localhost/"
        };

        SiteInfo.Provider.Set( site );
        SiteContext.CurrentSite = site;
    }

    private static string GetSQLScriptsPath( )
    {
        var sqlPackagePath = AppDomain.CurrentDomain.BaseDirectory + "SQL.zip";
        var sqlScriptsPath = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName( sqlPackagePath )!,
            ZipStorageProvider.GetZipFileName( System.IO.Path.GetFileName( sqlPackagePath ) )
        );

        return sqlScriptsPath;
    }

    protected virtual void OnFinalize( object? _, EventArgs e )
    {
        EnsureDatabaseDestroyed();
        SetConnectionString( null );
    }

    /// <summary> <see cref="ApplicationEvents.Initialized"/> handler. </summary>
    /// <remarks> Ensures test site and license key. </remarks>
    protected virtual void OnInit( object? _, EventArgs e )
    {
        EnsureLicenseKey();
        EnsureSite();
    }

    protected virtual void OnPostStart( object? _, EventArgs e )
    {
        EnsureAnonymousTaskProcessorDisabled();
    }

    /// <summary> <see cref="ApplicationEvents.PreInitialized"/> handler. </summary>
    /// <remarks> Ensures the CMS database has been initialized, and the HashStringSalt and CMSConnectionString are set. </remarks>
    protected virtual void OnPreInit( object? _, EventArgs e )
    {
        EnsureHashStringSalt();
        EnsureDatabaseInitialized();

        Service.Resolve<IAppSettingsService>()[ "CMSLoadHashtables" ] = bool.TrueString;
        Service.Resolve<IAppSettingsService>()[ "CMSWebFarmMode" ] = WebFarmModeEnum.Disabled.ToStringRepresentation();
    }

    private static void SetConnectionString( string? value, bool force = true )
    {
        ConnectionHelper.ConnectionString = value;
        if( force )
        {
            // force the underlying configuration to be changed; ConnectionHelper.ConnectionString.set performs a null check preventing this
            Service.Resolve<IConfiguration>()
                .GetSection( "ConnectionStrings" )
                .GetSection( ConnectionHelper.ConnectionStringName )
                .Value = value;
        }
    }

    /// <inheritdoc />
    public Task StartAsync( CancellationToken cancellationToken = default )
    {
        return host.StartAsync( cancellationToken );
    }

    /// <inheritdoc />
    public Task StopAsync( CancellationToken cancellationToken = default )
    {
        return host.StopAsync( cancellationToken );
    }
}