using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Company.WebApplication.Policies {
	public class EinkaufUserRequirement : IAuthorizationRequirement {
	}

	public class EinkaufUserHandler : AuthorizationHandler<EinkaufUserRequirement> {
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EinkaufUserRequirement requirement) {

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
