using System.Threading.Tasks;
using CMS.Base;
using CMS.Search;
using CMS.WebFarmSync;
using Microsoft.AspNetCore.Http;

namespace BizStream.Kentico.Xperience.AspNetCore.Mvc.Testing
{

    public class IntegrationTestMiddleware
    {
        #region Fields
        private readonly RequestDelegate next;
        #endregion

        public IntegrationTestMiddleware( RequestDelegate next )
            => this.next = next;

        private CMSActionContext TestActionContext( )
            => new CMSActionContext()
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

        private void DisableWebFarmsFeatures( )
        {
            WebFarmContext.WebFarmEnabled = false;
            WebFarmContext.UseTasksForExternalApplication = false;
        }

        private void DisabledSearchFeatures( )
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
}
