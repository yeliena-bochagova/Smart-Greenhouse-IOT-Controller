using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartGreenhouse.Web.Models;
using System.Linq;

namespace SmartGreenhouse.Web.Controllers
{
    [Authorize]
    public class Subroutine1Controller : Controller
    {
        [HttpGet]
        public IActionResult Index() => View(new SubroutineXModel());

        [HttpPost]
        public IActionResult Index(SubroutineXModel model)
        {
            model.Output = model.Input is null ? "" : new string(model.Input.Reverse().ToArray());
            return View(model);
        }

        [HttpGet]
        public IActionResult Description() => View();
    }
}
