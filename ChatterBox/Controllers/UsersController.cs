using ChatterBox.Data;
using ChatterBox.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChatterBox.Controllers
{
	public class UsersController : Controller
	{
		private readonly ApplicationDbContext MyDataBase;
		private readonly UserManager<ApplicationUser> MyUserManager;
		public UsersController(ApplicationDbContext _MyDataBase, UserManager<ApplicationUser> _MyUserManager)
		{
			MyDataBase = _MyDataBase;
			MyUserManager = _MyUserManager;
		}

		public IActionResult Show(string id)
		{
			ApplicationUser user = MyDataBase.Users.Find(id);

			return View(user);
		}

		[Authorize(Roles = "Admin")]
		public IActionResult List()
		{
			return View();
		}
	}
}
