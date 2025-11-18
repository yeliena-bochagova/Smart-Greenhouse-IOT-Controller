using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartGreenhouse.Web.Models;

namespace SmartGreenhouse.Web.Controllers
{
    [Authorize]
    public class Subroutine3Controller : Controller
    {
        [HttpGet]
        public IActionResult Index() => View(new SubroutineXModel());

        [HttpPost]
        public IActionResult Index(SubroutineXModel model)
        {
            model.Output = model.Input?.Length.ToString() ?? "0";
            return View(model);
        }

        [HttpGet]
        public IActionResult Description() => View();
    }
}
