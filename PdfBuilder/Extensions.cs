using Microsoft.Extensions.DependencyInjection;
using PdfBuilder.Abstractions;
using System.Linq;

namespace PdfBuilder
{
    public static class Extensions
    {
        /// <summary>
        /// Add the Transient to the IServiceCollection, if an implementation is not already present
        /// </summary>
        /// <typeparam name="SERV">The abstract class to register for DI</typeparam>
        /// <typeparam name="IMPL">The implementation class for <see cref="SERV"/></typeparam>
        /// <param name="services">The IServiceCollection to be updated with the implementation definition</param>
        /// <returns></returns>
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

        /// <summary>
        /// Initialize the IServiceCollection with the invariant built-in class factories, where the caller
        /// will provide a factory for IPdfBuilderOptions
        /// </summary>
        /// <param name="services">The IServiceCollection to be updated with the
        /// default PdfBuilder factories, but without any factory for IPdfBuilderOptions</param>
        /// <returns></returns>
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
    }
}