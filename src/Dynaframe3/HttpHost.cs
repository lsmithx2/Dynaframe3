﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Dynaframe3
{
    public static class HttpHost
    {
        public static WebApplication CreateHostBuilder(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((ctx, log) =>
            {
                log
                    .WriteTo.Console()
                    .WriteTo.Debug()
                    ;
            });

            builder.Services.AddControllersWithViews()
                .AddNewtonsoftJson();

            builder.Services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.MapControllers();

            return app;
        }
    }
}