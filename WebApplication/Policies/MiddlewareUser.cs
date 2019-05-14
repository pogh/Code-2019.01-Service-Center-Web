using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Company.WebApplication.Policies {
	public class MiddlewareUserRequirement : IAuthorizationRequirement {
	}

	public class MiddlewareUserHandler : AuthorizationHandler<MiddlewareUserRequirement> {
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MiddlewareUserRequirement requirement) {

			if (context.User.IsInRole("APO-SG-Entwicklung")) {
				context.Succeed(requirement);
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
