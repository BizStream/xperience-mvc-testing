using Microsoft.AspNetCore.Builder;

namespace BizStream.Kentico.Xperience.AspNetCore.Mvc.Testing
{

    /// <summary> Extensions to <see cref="IApplicationBuilder"/> to support Isolated Mvc Tests. </summary>
    public static class IApplicationBuilderExtensions
    {

        /// <summary> Configures the <paramref name="app"/> for Isolated Mvc Tests. </summary>
        public static IApplicationBuilder UseXperienceTesting( this IApplicationBuilder app )
            => app.UseMiddleware<IntegrationTestMiddleware>();

    }

}
