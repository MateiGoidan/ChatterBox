using ChatterBox.Data;
using ChatterBox.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;


namespace ChatterBox.Controllers
{
	[Authorize(Roles = "Admin")]
	public class CategoriesController : Controller
	{
		private readonly ApplicationDbContext MyDataBase;
		private readonly UserManager<ApplicationUser> MyUserManager;
		private readonly RoleManager<IdentityRole> MyRoleManager;
		public CategoriesController(ApplicationDbContext _MyDataBase, UserManager<ApplicationUser> _MyUserManager, RoleManager<IdentityRole> _MyRoleManager)
		{
			MyDataBase = _MyDataBase;
			MyUserManager = _MyUserManager;
			MyRoleManager = _MyRoleManager;
		}

		public IActionResult List()
		{
			ViewBag.Categories = MyDataBase.Categories;

			if (TempData.ContainsKey("Message"))
			{
				ViewBag.TempMsg = TempData["Message"];
			}

			return View();
		}

		public IActionResult New()
		{
			return View();
		}

		[HttpPost]
		public IActionResult New(Category _Category)
		{
			if (!ModelState.IsValid)
			{
				return View(_Category);
			}

			try
			{
				MyDataBase.Categories.Add(_Category);

				MyDataBase.SaveChanges();

				TempData["TempMsg"] = "New category added!";

				return RedirectToAction("List");
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to add the category. Please contact the dev team in order to resolve this issue." });
			}
		}

		public IActionResult Edit(int _Id)
		{
			try
			{
				return View(MyDataBase.Categories.Find(_Id));
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "Edit attempt on non existing category!" });
			}
		}

		[HttpPost]
		public IActionResult Edit(int _Id, Category _Category)
		{
			_Category.Id = _Id;

			Category? _OriginalCategory = MyDataBase.Categories.Find(_Id);

			if (_OriginalCategory == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Edit attempt on non existing category!" });
			}

			if (!ModelState.IsValid)
			{
				return View(_Category);
			}

			try
			{
				_OriginalCategory.Title = _Category.Title;

				MyDataBase.SaveChanges();

				TempData["TempMsg"] = "Edit completed!";

				return RedirectToAction("List");
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to edit the category. Please contact the dev team in order to resolve this issue." });
			}
		}

		[HttpPost]
		public IActionResult Delete(int _Id)
		{
			Category? _Category = MyDataBase.Categories.Find(_Id);

			if (_Category == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Delete attempt on non existing category!" });
			}

			try
			{
				MyDataBase.Categories.Remove(_Category);

				MyDataBase.SaveChanges();

				return RedirectToAction("List");
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to delete the category. Please contact the dev team in order to resolve this issue." });
			}
		}
	}
}
