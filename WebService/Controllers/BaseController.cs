using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Company.WebService.Models.MAINDB;

namespace Company.WebService.Controllers {

	[Authorize(Policy = "WebServicesUser")]

	public abstract class BaseController : ControllerBase {
		public BaseController(MAINDBContext context) : base() {
			_MAINDBContext = context;
			_MAINDBContext.Database.SetCommandTimeout(180);
		}

		private readonly MAINDBContext _MAINDBContext;
		protected MAINDBContext MAINDBContext {
			get { return _MAINDBContext; }
		}

		[ApiExplorerSettings(IgnoreApi = true)]
		[HttpGet]
		public virtual ActionResult<string> Hello() {

			string userName = string.Empty;

			try {
				userName = Request.HttpContext.User.Identity.Name;
			}
			catch {
			}

			if (string.IsNullOrEmpty(userName))
				return Ok(string.Concat(this.GetType().Name, " said, I'm here, but I don't know who you are."));
			else
				return Ok(string.Concat("Hello, ", Request.HttpContext.User.Identity.Name, ". ", GetType().Name, " said, I'm here."));
		}
	}


	public class RootController : BaseController {

		public RootController(MAINDBContext context, IApiDescriptionGroupCollectionProvider apiExplorer) : base(context) {
			_apiExplorer = apiExplorer;
		}

		private readonly IApiDescriptionGroupCollectionProvider _apiExplorer;

		[ApiExplorerSettings(IgnoreApi = true)]
		[HttpGet("/")]
		public ActionResult<string> Get() {

			string userName = string.Empty;

			try {
				userName = Request.HttpContext.User.Identity.Name;
			}
			catch {
			}

			if (string.IsNullOrEmpty(userName))
				return Ok("I'm here, but I don't know who you are.");
			else
				return Ok(string.Concat("Hello, ", Request.HttpContext.User.Identity.Name, ", I'm here."));
		}

		[ApiExplorerSettings(IgnoreApi = true)]
		[HttpGet("/api")]
		[Authorize]
		public ActionResult<string> Api() {

			Dictionary<string, string> apis = new Dictionary<string, string>();

			foreach (ApiDescriptionGroup apiDescriptionGroup in _apiExplorer.ApiDescriptionGroups.Items) {
				foreach (ApiDescription apiDescription in apiDescriptionGroup.Items) {

					StringBuilder api = new StringBuilder();

					if (apiDescription.SupportedResponseTypes.Count > 0) {
						if (apiDescription.SupportedResponseTypes[0].Type.IsGenericType) {
							api.Append(apiDescription.SupportedResponseTypes[0].Type.Name).Replace("`1", "");
							api.Append("<");
							api.Append(apiDescription.SupportedResponseTypes[0].Type.GetGenericArguments()[0].Name);
							api.Append(">");
						}
						else {
							api.Append(apiDescription.SupportedResponseTypes[0].Type.Name);
						}
						api.Append(" ");
					}

					api.Append(apiDescription.RelativePath);

					api.Append(" ");
					api.AppendLine(apiDescription.HttpMethod);

					foreach (ApiParameterDescription apiParameterDescription in apiDescription.ParameterDescriptions) {
						api.AppendLine(string.Concat("\t\t", Nullable.GetUnderlyingType(apiParameterDescription.Type) == null ? apiParameterDescription.Type.Name : string.Concat(Nullable.GetUnderlyingType(apiParameterDescription.Type).Name, "?"), " ", apiParameterDescription.Name));
					}

					apis.Add(string.Concat(apiDescription.RelativePath, " ", apiDescription.HttpMethod), api.ToString());
				}
			}

			StringBuilder returnValue = new StringBuilder();

			foreach (string key in apis.Keys.OrderBy(x => x)) {
				returnValue.AppendLine(apis[key]);
			}

			return Ok(returnValue.ToString());
		}
	}
}

namespace Company.WebService.Conventions {
	public class EnableApiExplorerApplicationConvention : IApplicationModelConvention {
		public void Apply(ApplicationModel application) {
			application.ApiExplorer.IsVisible = true;
		}
	}
}

