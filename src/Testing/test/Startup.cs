using Kentico.Content.Web.Mvc;
using Kentico.Web.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BizStream.Kentico.Xperience.AspNetCore.Mvc.Testing.Tests
{

    public class Startup
    {

        public void ConfigureServices( IServiceCollection services )
        {
            services.AddMvc();

            services.AddAuthentication();
            services.AddAuthorization();

            services.AddCors();
            services.AddRouting();

            services.AddKentico()
                .SetAdminCookiesSameSiteNone()
                .DisableVirtualContextSecurityForLocalhost();
        }

        public void Configure( IApplicationBuilder app )
        {
            app.UseXperienceTesting();

            app.UseCookiePolicy();
            app.UseCors();
            app.UseStaticFiles();

            app.UseKentico();

            app.UseRouting();
            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(
               endpoints =>
               {
                   endpoints.Kentico().MapRoutes();
                   endpoints.MapControllerRoute( name: "default", pattern: "{controller=Home}/{action=Index}/{id?}" );
               }
           );
        }

    }

}
