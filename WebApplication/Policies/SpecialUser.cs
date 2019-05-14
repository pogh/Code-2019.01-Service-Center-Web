using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Company.WebApplication.Policies {
	public class SpecialUserRequirement : IAuthorizationRequirement {
	}

	public class SpecialUserHandler : AuthorizationHandler<SpecialUserRequirement> {
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SpecialUserRequirement requirement) {

			if (context.User.Identity.Name.Equals("MAINDB-2030\\pspellman", StringComparison.InvariantCulture)) {
				context.Succeed(requirement);
			}
			if (context.User.Identity.Name.Equals("MAINDB-2030\\tvoigt", StringComparison.InvariantCulture)) {
				context.Succeed(requirement);
			}
			if (context.User.Identity.Name.Equals("MAINDB-2030\\pspellman", StringComparison.InvariantCulture)) {
				context.Succeed(requirement);
			}
			if (context.User.Identity.Name.Equals("MAINDB-2030\\mbroeskiewicz", StringComparison.InvariantCulture)) {
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
