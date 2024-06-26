﻿using Catalog.API;
using Catalog.API.Middlewares;
using Catalog.Domain;
using Catalog.gRPCService;
using Ecommerce.Repository;
using Ecommerce.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("MSSQLTest")));
        services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<CatalogDbContext>()
                .AddDefaultTokenProviders();
        services.AddGrpc();
        services.AddAplicationServices();
        services.ConfigureCookies();
        services.ConfigureCors();
        services.AddFixedWindowRateLimiter();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Catalog API", Version = "v1" });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        SerilogConfiguration.ConfigureSerilog();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors("SpecificOrigins");
        app.UseRateLimiter();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<CategorygRPCService>();
            endpoints.MapControllers().RequireRateLimiting("fixed");
        });
    }
}