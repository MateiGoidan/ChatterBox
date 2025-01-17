using ChatterBox.Data;
using ChatterBox.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
			GetChannels();

			var categories = MyDataBase.Categories.Include("Channels");

			if (TempData.ContainsKey("Message"))
			{
				ViewBag.TempMsg = TempData["Message"];
			}

			// Search Engine

			var _CategoriesSearch = "";

			if (!string.IsNullOrEmpty(Convert.ToString(HttpContext.Request.Query["categoriesSearch"])))
			{
				_CategoriesSearch = Convert.ToString(HttpContext.Request.Query["categoriesSearch"]).Trim(); // Remove spaces 

				List<int> categoriesIds = MyDataBase.Categories
					.Where(a => a.Title != null && a.Title.Contains(_CategoriesSearch))
					.Select(a => a.Id)
					.ToList();

				categories = MyDataBase.Categories.Where(a => categoriesIds.Contains(a.Id));
			}

			ViewBag.CategoriesSearchString = _CategoriesSearch;

			// Pagination

			int numbPerPage = 9;
			int totalNumb = categories.Count();

			var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

			var offset = 0;
			if (!currentPage.Equals(0))
			{
				offset = (currentPage - 1) * numbPerPage;
			}

			var paginatedCategories = categories.Skip(offset).Take(numbPerPage);

			ViewBag.lastPage = Math.Ceiling((float)totalNumb / (float)numbPerPage);
			ViewBag.Categories = paginatedCategories;

			if (_CategoriesSearch != "")
			{
				ViewBag.PaginationBaseUrl = "/Categories/List/?categoriesSearch=" + _CategoriesSearch+ "&search" + "&page";
			}
			else
			{
				ViewBag.PaginationBaseUrl = "/Categories/List/?page";
			}

			return View();
		}

		public IActionResult New()
		{
			GetChannels();

			return View();
		}

		[HttpPost]
		public IActionResult New(Category _Category)
		{
			GetChannels();

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

		[Authorize(Roles = "Admin")]
		public IActionResult Edit(int Id)
		{
			GetChannels();

			try
			{
				return View(MyDataBase.Categories.Find(Id));
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "Edit attempt on non existing category!" });
			}
		}

		[Authorize(Roles = "Admin")]
		[HttpPost]
		public IActionResult Edit(int Id, Category Category)
		{
			Category.Id = Id;

			Category? _OriginalCategory = MyDataBase.Categories.Find(Id);

			if (_OriginalCategory == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Edit attempt on non existing category!" });
			}

			ViewBag.CategoryTitle = _OriginalCategory.Title;

			GetChannels();

			if (!ModelState.IsValid)
			{
				return View(Category);
			}

			try
			{
				_OriginalCategory.Title = Category.Title;

				MyDataBase.SaveChanges();

				TempData["TempMsg"] = "Edit completed!";

				return RedirectToAction("List");
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to edit the category. Please contact the dev team in order to resolve this issue." });
			}
		}

		[Authorize(Roles = "Admin")]
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

		[NonAction]
		public void GetChannels()
		{
			List<BindChannelUser> _BindChannelUser = MyDataBase.BindChannelUserEntries
				.Where(b => b.UserId == MyUserManager.GetUserId(User))
				.ToList();

			List<int> _ChannelsIds = new List<int>();
			foreach (BindChannelUser _Bind in _BindChannelUser)
			{
				_ChannelsIds.Add(_Bind.ChannelId);
			}

			string _Search = "";

			if (!string.IsNullOrEmpty(Convert.ToString(HttpContext.Request.Query["search"])))
			{
				_Search = Convert.ToString(Request.Query["Search"]);

				_ChannelsIds = MyDataBase.Channels
					.Where(c => (c.Name.ToUpper().Contains(_Search.ToUpper()) ||
					c.Description.ToUpper().Contains(_Search.ToUpper())) && 
					_ChannelsIds.Contains(c.Id))
					.Select(c => c.Id)
					.ToList();
			}

			var _Channels = MyDataBase.Channels
				.Where(c => _ChannelsIds.Contains(c.Id))
				.ToList();

			ViewBag.SearchString = _Search;

			ViewBag.UserChannels = _Channels;
		}
	}
}
