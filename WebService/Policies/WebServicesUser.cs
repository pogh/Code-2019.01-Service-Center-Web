using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Company.WebService.Policies {
	public class WebServicesUserRequirement : IAuthorizationRequirement {
	}

	/// <summary>
	/// Decides who has access to the web service
	/// </summary>
	public class WebServicesUserHandler : AuthorizationHandler<WebServicesUserRequirement> {
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, WebServicesUserRequirement requirement) {

			if (context.User.Identity.Name.Equals("MAINDB-2030\\ccwebapp", StringComparison.InvariantCulture)) {
				context.Succeed(requirement);
			}
			else if (context.User.IsInRole("APO-SG-Entwicklung")) {
				context.Succeed(requirement);
			}
			else {
				context.Fail();
			}

			return Task.CompletedTask;
		}
	}

}
