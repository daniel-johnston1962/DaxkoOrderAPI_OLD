using AutoMapper;
using DaxkoOrderAPI.Data;
using DaxkoOrderAPI.Data.Orders;
using DaxkoOrderAPI.Features.Handlers;
using DaxkoOrderAPI.Mapper;
using DaxkoOrderAPI.Repositories;
using DaxkoOrderAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DaxkoOrderAPI.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void AddServices(this IServiceCollection services, IConfiguration configuration, IHostingEnvironment environment)
        {
            services.AddTransient<IOrderItemHandler, OrderItemHandler>();
            services.AddTransient<ISaveOrderHandler, SaveOrderHandler>();
            services.AddTransient<IPastOrderHandler, PastOrderHandler>();
            services.AddTransient<IItemIDHandler, ItemIDHandler>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddScoped<IRepository<Item>, DaxkoRepository<Item>>();
            services.AddScoped<IRepository<OrderDetail>, DaxkoRepository<OrderDetail>>();
            services.AddScoped<IRepository<ShippedOrder>, DaxkoRepository<ShippedOrder>>();
            services.AddScoped<IHttpContextService, HttpContextService>();
            services.AddScoped(x =>
            {
                var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
                var factory = x.GetRequiredService<IUrlHelperFactory>();
                return factory.GetUrlHelper(actionContext);
            });

            // AutoMapper
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new OrderItemDtoMapper());
            });

            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);

            services.AddHttpContextAccessor();
        }

        public static void AddValidatorServices(this IServiceCollection services)
        {
            // if validation is needed
            //services.AddTransient<IValidator<>, ();
        }

        public static void AddDbContexts(this IServiceCollection services, IConfiguration configuration, IHostingEnvironment environment)
        {
            services.AddDbContext<DaxkoDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Daxko"))
                    .EnableSensitiveDataLogging(environment.IsDevelopment())
                    .UseLazyLoadingProxies());
        }

        public static void AddHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            // where http client information to get to another API
            // example
            //services.AddHttpClient<IUserApiClient, UserApiClient>(client =>
            //{
            //    var serviceProvider = services.BuildServiceProvider();
            //    var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            //    var bearerToken = httpContextAccessor.HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token").Result;
            //    client.BaseAddress = new Uri(configuration["ApiStrings:SomeApiBaseUrl"]);
            //    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
            //});
        }

        public static void AddAuthentication(this IServiceCollection services, IConfiguration configuration, string apiName)
        {
            services.AddAuthentication(o =>
            {
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.Authority = configuration["Identity:Authority"];
                o.Audience = apiName;
                o.RequireHttpsMetadata = false;
                o.SaveToken = true;
            });
        }
    }
}
