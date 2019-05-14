using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Company.WebApplication.Policies {
	public class ServiceCenterUserRequirement : IAuthorizationRequirement {
	}

	public class ServiceCenterUserHandler : AuthorizationHandler<ServiceCenterUserRequirement> {
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ServiceCenterUserRequirement requirement) {

			if (context.User.IsInRole("APO-APP-SCWC-Access")) {
				context.Succeed(requirement);
			}
			else if (context.User.IsInRole("APO-APP-SCWC-Access-Getaline")) {
				context.Succeed(requirement);
			}
			else if (context.User.IsInRole("APO-SG-Entwicklung")) {
				context.Succeed(requirement);
			}
			else {

				List<string> users = new List<string>() {
					"MAINDB-2030\\jknauer",
					"MAINDB-2030\\awolfram",
					"MAINDB-2030\\mwinter",
					"MAINDB-2030\\tedelmann"
				};

				if (users.Contains(context.User.Identity.Name)) {
					context.Succeed(requirement);
				}
			}

			/*
			if (context.User.Identity.Name.Equals("")) {
				context.Succeed(requirement);
			}
			else if (context.User.IsInRole("")) {
				context.Succeed(requirement);
			}
			*/

			return Task.CompletedTask;
		}
	}

}
