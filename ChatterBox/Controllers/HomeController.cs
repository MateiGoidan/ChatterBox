using System.Diagnostics;
using ChatterBox.Data;
using ChatterBox.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChatterBox.Controllers
{
	public class HomeController : Controller
	{
		private readonly ApplicationDbContext MyDataBase;
		private readonly UserManager<ApplicationUser> MyUserManager;
		private readonly SignInManager<ApplicationUser> SignInManager;

		public HomeController(ApplicationDbContext _MyDataBase, UserManager<ApplicationUser> _MyUserManager, SignInManager<ApplicationUser> _SignInManager)
		{
			MyDataBase = _MyDataBase;
			MyUserManager = _MyUserManager;
			SignInManager = _SignInManager;
		}

		public IActionResult Index()
		{
			if(!SignInManager.IsSignedIn(User))
			{
				return View();
			}
			else
			{
				return RedirectToAction("Show", "Users", new { id = MyUserManager.GetUserId(User) });
			}
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
