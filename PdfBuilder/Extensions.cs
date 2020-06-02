using Microsoft.Extensions.DependencyInjection;
using PdfBuilder.Abstractions;
using System.Linq;

namespace PdfBuilder
{
    /// <summary>
    /// Helper methods for the project
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Initialize the IServiceCollection with the invariant built-in class factories, where the caller
        /// will provide a factory for IPdfBuilderOptions.
        /// </summary>
        /// <param name="services">The IServiceCollection to be updated with some default implementations
        /// of the CreatePdf interfaces, but without any factory for <see cref="IPdfBuilderOptions"/>.</param>
        /// <returns>The 'services' argument, for Fluent coding.</returns>
        public static IServiceCollection PdfBuilderBasic(this IServiceCollection services)
        {
            // Add the default HtmlBodyFactopry, if it hasn't already been added
            services
                .SafeAddTransient<IHtmlBodyFactory, HtmlBodyFactory>()
                .SafeAddTransient<IPdfBuilderResult, PdfBuilderResult>()
                .SafeAddTransient<IPdfBuilderResults, PdfBuilderResults>()
                .SafeAddTransient<IPdfBuilder, Builder>()
                ;
            return services;
        }

        /// <summary>
        /// Add the Transient to the IServiceCollection, if an implementation is not already present
        /// </summary>
        /// <typeparam name="SERV">The abstract class to register in IServiceCollection.</typeparam>
        /// <typeparam name="IMPL">The implementation class for SERV.</typeparam>
        /// <param name="services">The IServiceCollection to be updated with the implementation definition.</param>
        /// <returns>The 'services' argument, for Fluent coding.</returns>
        private static IServiceCollection SafeAddTransient<SERV, IMPL>(this IServiceCollection services)
            where SERV : class
            where IMPL: class, SERV
        {
            if (!services.Any(descriptor => descriptor.ServiceType == typeof(SERV)))
            {
                services.AddTransient<SERV, IMPL>();
            }
            return services;
        }
    }
}
