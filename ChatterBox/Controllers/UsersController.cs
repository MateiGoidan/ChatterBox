using ChatterBox.Data;
using ChatterBox.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace ChatterBox.Controllers
{
	public class UsersController : Controller
	{
		private readonly ApplicationDbContext MyDataBase;
		private readonly UserManager<ApplicationUser> MyUserManager;
		private readonly RoleManager<IdentityRole> MyRoleManager;
		public UsersController(ApplicationDbContext _MyDataBase, UserManager<ApplicationUser> _MyUserManager, RoleManager<IdentityRole> _MyRoleManager)
		{
			MyDataBase = _MyDataBase;
			MyUserManager = _MyUserManager;
			MyRoleManager = _MyRoleManager;
		}

		[Authorize(Roles = "User, Admin")]
		public IActionResult Show(string id)
		{
			ApplicationUser user = MyDataBase.Users.Find(id);

			try
			{
				return View(user);
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "Could not find the user that you are looking for!" });
			}
		}

		[Authorize(Roles = "Admin")]
		public IActionResult List()
		{
			ViewBag.Users = MyUserManager.Users.ToList();

			if (TempData.ContainsKey("Message"))
			{
				ViewBag.TempMsg = TempData["Message"];
			}

			return View();
		}
		[Authorize(Roles = "User, Admin")]
		[HttpPost]
		public IActionResult Delete(string _ID)
		{
			ApplicationUser? _DeleteUser = MyDataBase.AppUsers.Find(_ID);

			string _RedirectAction = "List";

			if (_DeleteUser == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Delete attempt on non existing user!" });
			}

			try
			{
				MyDataBase.AppUsers.Remove(_DeleteUser);

				MyDataBase.SaveChanges();

				TempData["TempMsg"] = "User deleted!";

				return RedirectToAction("List");
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to delete the user. Please contact the dev team in order to resolve this issue." });
			}
		}

		[Authorize(Roles = "Admin")]
		[HttpPost]
		public async Task<IActionResult> Promote(string _ID)
		{
			var _User = await MyUserManager.FindByIdAsync(_ID);

			var _OldRole = await MyUserManager.GetRolesAsync(_User);

			string _NewRole = "";
			if (_OldRole.Contains("User"))
			{
				_NewRole = "Admin";
			}
			else
			{
				return View("Error", new ErrorViewModel { RequestId = "User is already an admin!" });
			}

			var _Role = await MyRoleManager.FindByNameAsync(_NewRole);


			if (_Role == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Role not found" });
			}

			try
			{
				await MyUserManager.RemoveFromRolesAsync(_User, _OldRole);

				await MyUserManager.AddToRoleAsync(_User, _Role.Name);

				return RedirectToAction("List");
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to promote a user. Please contact the dev team in order to resolve this issue." });
			}
		}

		[Authorize(Roles = "Admin")]
		[HttpPost]
		public async Task<IActionResult> Demote(string _ID)
		{
			var _User = await MyUserManager.FindByIdAsync(_ID);

			var _OldRole = await MyUserManager.GetRolesAsync(_User);

			string _NewRole = "";
			if (_OldRole.Contains("Admin"))
			{
				_NewRole = "User";
			}
			else
			{
				return View("Error", new ErrorViewModel { RequestId = "Already a user, cannot demote it no more!" });
			}

			var _Role = await MyRoleManager.FindByNameAsync(_NewRole);

			if (_Role == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Role not found" });
			}

			try
			{
				await MyUserManager.RemoveFromRolesAsync(_User, _OldRole);

				await MyUserManager.AddToRoleAsync(_User, _Role.Name);

				return RedirectToAction("List");
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to demote a user. Please contact the dev team in order to resolve this issue." });
			}
		}

	}
}
