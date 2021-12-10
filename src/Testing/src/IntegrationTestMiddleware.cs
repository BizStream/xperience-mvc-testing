using CMS.Base;
using CMS.Search;
using CMS.WebFarmSync;
using Microsoft.AspNetCore.Http;

namespace BizStream.Kentico.Xperience.AspNetCore.Mvc.Testing;

/// <summary> Mvc Middleware providing support for Isolated Mvc Tests. </summary>
public class IntegrationTestMiddleware
{
    #region Fields
    private readonly RequestDelegate next;
    #endregion

    public IntegrationTestMiddleware( RequestDelegate next )
    {
        this.next = next;
    }

    /// <summary> Returns a <see cref="CMSActionContext"/> that disables functionality not needed for Isolated Mvc Tests. </summary>
    /// <remarks> Adapted from CMS.Tests.AutomatedTest.DisableUnnecessaryFunctionality. </remarks>
    private static CMSActionContext TestActionContext( )
    {
        return new()
        {
            LogSynchronization = false,
            LogExport = false,
            LogIntegration = false,
            EnableLogContext = false,
            LogWebFarmTasks = false,
            CreateVersion = false,
            AllowAsyncActions = false,
            SendEmails = false,
            SendNotifications = false,
            TouchParent = false,
            UseGlobalAdminContext = false,
            UpdateUserCounts = false,
            UseCacheForSynchronizationXMLs = false,
            EnableSmartSearchIndexer = false,
            CreateSearchTask = false
        };
    }

    /// <summary> Disables WebFarm functionality. </summary>
    private static void DisableWebFarmsFeatures( )
    {
        WebFarmContext.WebFarmEnabled = false;
        WebFarmContext.UseTasksForExternalApplication = false;
    }

    private static void DisabledSearchFeatures( )
    {
        SearchIndexInfoProvider.SearchEnabled = false;
    }

    public async Task InvokeAsync( HttpContext context )
    {
        DisabledSearchFeatures();
        DisableWebFarmsFeatures();

        using( TestActionContext() )
        {
            await next( context );
        }
    }
}