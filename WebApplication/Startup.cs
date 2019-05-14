using System.Collections.Generic;
using System.Globalization;
using AspNetCore.RouteAnalyzer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Company.WebApplication.Policies;

namespace Company.WebApplication {
	public class Startup {
		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services) {

			services.AddAuthentication(Microsoft.AspNetCore.Server.IISIntegration.IISDefaults.AuthenticationScheme);

			services.Configure<CookiePolicyOptions>(options => {
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				options.CheckConsentNeeded = context => false;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});

			//-- Add Path to Resources File
			services.AddLocalization(options => options.ResourcesPath = "Resources");

			//-- Caching
			services.AddMemoryCache();

			//-- Configure Windows Authentication
			services.AddAuthentication(Microsoft.AspNetCore.Server.IISIntegration.IISDefaults.AuthenticationScheme);

			//-- Add Policies
			services.AddAuthorization(options => {
				// -- Areas
				options.AddPolicy("EinkaufUser", policy => policy.Requirements.Add(new EinkaufUserRequirement()));
				options.AddPolicy("FeedUser", policy => policy.Requirements.Add(new FeedUserRequirement()));
				options.AddPolicy("MiddlewareUser", policy => policy.Requirements.Add(new MiddlewareUserRequirement()));
				options.AddPolicy("ServiceCenterUser", policy => policy.Requirements.Add(new ServiceCenterUserRequirement()));
				// --
				options.AddPolicy("SpecialUser", policy => policy.Requirements.Add(new SpecialUserRequirement()));
			});

			services.AddSingleton<IAuthorizationHandler, EinkaufUserHandler>();
			services.AddSingleton<IAuthorizationHandler, FeedUserHandler>();
			services.AddSingleton<IAuthorizationHandler, MiddlewareUserHandler>();
			services.AddSingleton<IAuthorizationHandler, ServiceCenterUserHandler>();
			services.AddSingleton<IAuthorizationHandler, SpecialUserHandler>();

#if DEBUG
			services.AddRouteAnalyzer();
#endif
			//-- Add localisation
			services.AddMvc(options => {
				options.CacheProfiles.Add("Never",
					new CacheProfile() {
						Location = ResponseCacheLocation.None,
						NoStore = true
					});
			}).AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
				.AddDataAnnotationsLocalization()
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
			//-- Available and default languages
			CultureInfo en = new CultureInfo("en");
			en.DateTimeFormat.ShortDatePattern = "dd-MM-yyyy";
			en.DateTimeFormat.ShortTimePattern = "HH:mm";
			en.DateTimeFormat.LongTimePattern = "HH:mm:ss";

			IList<CultureInfo> supportedCultures = new List<CultureInfo>
			{
				en,
				new CultureInfo("de"),
			};
			app.UseRequestLocalization(new RequestLocalizationOptions {
				DefaultRequestCulture = new RequestCulture("de"),
				SupportedCultures = supportedCultures,
				SupportedUICultures = supportedCultures
			});

			app.UseStatusCodePagesWithRedirects("~/{0}");

			//--
			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}
			else {
				app.UseExceptionHandler("/Root/Error");
				app.UseHsts();
			}

			//--

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			//app.UseCookiePolicy();

			app.UseMvc(routes => {
#if DEBUG
				routes.MapRouteAnalyzer("/routes");
#endif
				routes.MapRoute(
						name: "areaRoute",
						template: "/{area:exists=ServiceCenter}/{controller=Default}/{action=Index}"
					);
				routes.MapRoute(
						name: "root",
						template: "/{controller}/{action}"
					);
			});
		}
	}
}
