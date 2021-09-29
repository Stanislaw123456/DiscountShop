using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DiscountStore.Modules.Discounts.Enums;
using DiscountStore.Modules.Discounts.Models;
using DiscountStore.Modules.Discounts.Providers;
using DiscountStore.Modules.Products.Models;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DiscountStore.Infrastructure
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddMediatR(typeof(Startup));
            services.AddSession();

            ConfigureInMemoryDatabase(services);

            ConfigureAppSpecificServices(services);
        }

        private static void ConfigureAppSpecificServices(IServiceCollection services)
        {
            services.AddScoped<IDiscountProvider, TwoForXDiscountTypeProvider>();
            services.AddScoped<IDiscountProvider, ThreeForXDiscountTypeProvider>();
        }

        private static void ConfigureInMemoryDatabase(IServiceCollection services)
        {
#pragma warning disable ASP0000
            var entityFrameworkServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();
#pragma warning restore ASP0000

            services.AddDbContext<DiscountStoreDbContext>(options =>
            {
                options.UseInMemoryDatabase("Database");
                options.UseInternalServiceProvider(entityFrameworkServiceProvider);
                options.ConfigureWarnings(
                    builder => builder.Ignore(new[] { InMemoryEventId.TransactionIgnoredWarning }));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DiscountStoreDbContext dbContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();

            app.UseRouting();

            app.UseAuthorization();

            ConfigureDefaultRequestCulture(app);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            SeedProducts(dbContext);
            SeedDiscounts(dbContext);
        }

        private static void ConfigureDefaultRequestCulture(IApplicationBuilder app)
        {
            var cultureInfo = new CultureInfo("en-US")
            {
                NumberFormat =
                {
                    CurrencySymbol = "$"
                }
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(cultureInfo),
                SupportedCultures = new List<CultureInfo>
                {
                    cultureInfo,
                },
                SupportedUICultures = new List<CultureInfo>
                {
                    cultureInfo,
                }
            });
        }

        private static void SeedProducts(DiscountStoreDbContext dbContext)
        {
            var isProductsExist = dbContext.Products.Any();
            if (isProductsExist)
            {
                return;
            }

            var products = new List<Product>
            {
                new Product { Name = "Vase", Price = 1.2 },
                new Product { Name = "Big mug", Price = 1.0 },
                new Product { Name = "Napkins pack", Price = 0.45 }
            };

            dbContext.Products.AddRange(products);
            dbContext.SaveChanges();
        }

        private static void SeedDiscounts(DiscountStoreDbContext dbContext)
        {
            var isDiscountsExist = dbContext.Discounts.Any();
            if (isDiscountsExist)
            {
                return;
            }

            var bigMugProduct = dbContext.Products.First(product =>
                string.Equals(product.Name, "Big mug", StringComparison.InvariantCultureIgnoreCase));

            var napkinsPackProduct = dbContext.Products.First(
                product => string.Equals(product.Name, "Napkins pack", StringComparison.InvariantCultureIgnoreCase));

            var discounts = new List<Discount>
            {
                new Discount
                    { DiscountType = DiscountType.TwoForX, Product = bigMugProduct, DiscountedUnitPrice = 0.75 },
                new Discount
                    { DiscountType = DiscountType.ThreeForX, Product = napkinsPackProduct, DiscountedUnitPrice = 0.30 }
            };

            dbContext.Discounts.AddRange(discounts);
            dbContext.SaveChanges();
        }
    }
}