using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Company.WebService {
	public sealed class Program {
		public static void Main(string[] args) {
			CreateWebHostBuilder(args).Build().Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
					.ConfigureLogging((hostingContext, logging) => {
#if DEBUG
						logging.AddConsole();
#else
						logging.AddEventSourceLogger();
#endif
					})
			.UseStartup<Startup>();
	}
}
