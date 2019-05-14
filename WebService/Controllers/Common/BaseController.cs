using Microsoft.AspNetCore.Mvc;
using Company.WebService.Models.MAINDB;

namespace Company.WebService.Controllers.Common
{
	[Route("api/common/[controller]/[action]")]
    public abstract class BaseController : WebService.Controllers.BaseController
    {
		public BaseController(MAINDBContext context) : base(context) {
		}
	}
}
