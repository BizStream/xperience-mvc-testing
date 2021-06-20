using Kentico.Content.Web.Mvc;
using Kentico.Web.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BizStream.Kentico.Xperience.AspNetCore.Mvc.Testing
{

    /// <summary> Provides a base startup type with the minimal configuration required to host an Isolated Mvc Test using <see cref="XperienceWebApplicationFactory{TStartup}"/>. </summary>
    public abstract class XperienceTestStartup
    {

        /// <summary> Configures the application as an Xperience Mvc app with Isiolated Integration support. </summary>
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

            ConfigureTests( app );
        }

        /// <summary> Configures services required to host an Xperience Mvc app with Isolated Integration Support. </summary>
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

            ConfigureTestServices( services );
        }

        /// <summary> Configure the app for Isolated Mvc tests. </summary>
        public abstract void ConfigureTests( IApplicationBuilder app );

        /// <summary> Configure services needed to run Isolated Mvc Tests. </summary>
        public abstract void ConfigureTestServices( IServiceCollection services );

    }

}
