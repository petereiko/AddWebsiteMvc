using AddWebsiteMvc.Business.Entities.Identity;
using AddWebsiteMvc.Business.Interfaces;
using AddWebsiteMvc.Business.Persistence;
using AddWebsiteMvc.Business.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace AddWebsiteMvc.Business
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {

            
            // Add Services

            return services;
        }
    }
}
