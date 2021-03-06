﻿using System.Linq;
using API.Data;
using API.Data.Contracts;
using API.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using API.Extensions;
using API.Formatters;
using API.TransferModels.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Swashbuckle.AspNetCore.Swagger;
using IdentityServer4.Services;
using System.Security.Claims;

namespace API
{
    public class Startup
    {
        private readonly string CorsPolicy = "DAW-CORS-POLICY";
        private IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // EF Core
            services.AddDbContext<DatabaseContext>(
                opt => opt.UseNpgsql(Configuration["Data:PostgreConnection:ConnectionString"])
            );

            // Configure Cors
            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicy,
                     builder => {
                         var origins = Configuration.GetSection("SafeOrigins")
                            .GetChildren()
                            .Select(x => x.Value)
                            .ToArray();

                         builder.WithOrigins(origins)
                            .WithHeaders("Authorization", "Content-Type")
                            .AllowAnyMethod();
                     });
            });

            // lowercase urls for consistency
            services.AddRouting(options => options.LowercaseUrls = true);

            // Identity Server
            services.AddIdentityServer()
                .AddTemporarySigningCredential()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients());

            // MVC
            services.AddMvc(options =>
            {
                // Accept application/vnd.siren+json media type (json format)
                options.OutputFormatters.Add(new SirenOutputFormatter());
            }).AddJsonOptions(options =>
            {
                // Ignore null fields on json formats by default
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            // needed to inject IUrlHelper
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "DAW API" });
            });

            // Custom Services
            // AddTransient -> services are created each time they are requested.
            // AddScoped    -> services are created once per request.
            // AddSingleton -> services are created only the first time.
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<ITeacherRepository, TeacherRepository>();
            services.AddScoped<IClassRepository, ClassRepository>();
            services.AddScoped<ICourseRepository, CourseRepository>();
            services.AddScoped<IGroupRepository, GroupsRepository>();

            services.AddScoped<StudentsSirenHto>();
            services.AddScoped<ClassStudentsSirenHto>();

            services.AddScoped<TeachersSirenHto>();
            services.AddScoped<ClassTeachersSirenHto>();

            services.AddScoped<CoursesSirenHto>();
            services.AddScoped<TeacherCoursesSirenHto>();

            services.AddScoped<ClassesSirenHto>();
            services.AddScoped<CourseClassesSirenHto>();
            services.AddScoped<TeacherClassesSirenHto>();
            services.AddScoped<StudentClassesSirenHto>();

            services.AddScoped<GroupsSirenHto>();
            services.AddScoped<ClassGroupsSirenHto>();
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            DatabaseContext context)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug(LogLevel.Debug);

            // Seed data if there is none
            if (env.IsDevelopment())
            {
                context.ClearAllData();
                context.EnsureSeedDataForContext();
            }

            app.UseCors(CorsPolicy);

            app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
            {
                Authority = "http://localhost:5000",
                RequireHttpsMetadata = false,
                RoleClaimType=ClaimTypes.Role,
                NameClaimType=ClaimTypes.Name,

                AutomaticChallenge = false,

                ApiName = "daw_api"
            });

            app.UseIdentityServer();

            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseStatusCodePagesWithReExecute("/error/{0}");

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DAW API");
            });
        }
    }
}
