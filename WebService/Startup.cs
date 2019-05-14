using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Data.SqlClient;
using Company.WebService.Policies;
using System;
using Microsoft.AspNetCore.Http;

namespace Company.WebService {

	public class Startup {
		public Startup(ILogger<Startup> logger, IConfiguration configuration) {
			Logger = logger;
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		private ILogger<Startup> Logger { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services) {

			//-- Database
			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(Configuration.GetConnectionString("MAINDB"));

			if (string.IsNullOrEmpty(builder.ConnectionString))
				Logger.Log(LogLevel.Critical, "Missing Connection String 'MAINDB'");
#if DEBUG
			/*
			 * To add your username and password to the insecure secure store, execute these commands with your credentials
            dotnet user-secrets set "MAINDB:ConnectionString:UserId" "YourUserId"
            dotnet user-secrets set "MAINDB:ConnectionString:Password" "YourPassword"
            */

			builder.UserID = Configuration["MAINDB:ConnectionString:UserId"];
			builder.Password = Configuration["MAINDB:ConnectionString:Password"];

			services.AddDbContext<Models.MAINDB.MAINDBContext>(options =>
				options.UseSqlServer(builder.ConnectionString,
									 sqlServerOptionsAction: sqlOptions => {
										 sqlOptions.EnableRetryOnFailure(
											 maxRetryCount: 5,
											 maxRetryDelay: TimeSpan.FromSeconds(1),
											 errorNumbersToAdd: null);
									 })
					   .UseLoggerFactory(Utils.LoggerFactory) // Warning: Do not create a new ILoggerFactory instance each time
			);

			services.AddDbContext<Models.ServiceCenter.ServiceCenterContext>(options =>
				options.UseSqlServer(builder.ConnectionString,
									 sqlServerOptionsAction: sqlOptions => {
										 sqlOptions.EnableRetryOnFailure(
											 maxRetryCount: 5,
											 maxRetryDelay: TimeSpan.FromSeconds(1),
											 errorNumbersToAdd: null);
									 })
					   .UseLoggerFactory(Utils.LoggerFactory) // Warning: Do not create a new ILoggerFactory instance each time
			);
#else

            services.AddDbContext<Models.MAINDB.MAINDBContext>(options =>
				options.UseSqlServer(builder.ConnectionString,
									 sqlServerOptionsAction: sqlOptions => {
										 sqlOptions.EnableRetryOnFailure(
											 maxRetryCount: 5,
											 maxRetryDelay: TimeSpan.FromSeconds(1),
											 errorNumbersToAdd: null);
									 })
            );

            services.AddDbContext<Models.ServiceCenter.ServiceCenterContext>(options =>
				options.UseSqlServer(builder.ConnectionString,
									 sqlServerOptionsAction: sqlOptions => {
										 sqlOptions.EnableRetryOnFailure(
											 maxRetryCount: 5,
											 maxRetryDelay: TimeSpan.FromSeconds(1),
											 errorNumbersToAdd: null);
									 })
            );
#endif

			//-- Configure Windows Authentication
			services.AddAuthentication(Microsoft.AspNetCore.Server.IISIntegration.IISDefaults.AuthenticationScheme);

			//-- Control access to web services
			services.AddAuthorization(options => {
				options.AddPolicy("WebServicesUser", policy => policy.Requirements.Add(new WebServicesUserRequirement()));
			});
			services.AddSingleton<IAuthorizationHandler, WebServicesUserHandler>();

			//--
			services.AddMvc(options =>
				options.Conventions.Add(new Conventions.EnableApiExplorerApplicationConvention())
				).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env) {

			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}
			else {
				app.UseExceptionHandler("/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseMvc();
		}
	}
}
