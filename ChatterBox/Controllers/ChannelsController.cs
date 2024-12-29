using ChatterBox.Data;
using ChatterBox.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ChatterBox.Controllers
{
	public class ChannelsController : Controller
	{
		private readonly ApplicationDbContext MyDataBase;
		private readonly UserManager<ApplicationUser> MyUserManager;
		private readonly RoleManager<IdentityRole> MyRoleManager;

		public ChannelsController(ApplicationDbContext _MyDataBase, UserManager<ApplicationUser> _MyUserManager, RoleManager<IdentityRole> _MyRoleManager)
		{
			MyDataBase = _MyDataBase;
			MyUserManager = _MyUserManager;
			MyRoleManager = _MyRoleManager;
		}

		[NonAction]
		public void GetUserChannels()
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
					.Where(c => c.Name.ToUpper().Contains(_Search.ToUpper()) || c.Description.ToUpper().Contains(_Search.ToUpper()))
					.Select(c => c.Id)
					.ToList();
			}

			List<Channel> _Channels = MyDataBase.Channels
				.Where(c => _ChannelsIds.Contains(c.Id))
				.ToList();

			ViewBag.SearchString = _Search;

			ViewBag.UserChannels = _Channels;
		}

		[Authorize(Roles = "User,Admin")]
		public IActionResult List()
		{
			GetUserChannels();

			ViewBag.Channels = MyDataBase.Channels.Include("Category").ToList();

			ViewBag.Admin = MyDataBase.BindChannelUserEntries
				.Where(b => b.Role == "Admin")
				.Select(b => b.UserId)
				.First();

			if (TempData.ContainsKey("Message"))
			{
				ViewBag.TempMsg = TempData["Message"];
			}

			return View();
		}

		[Authorize(Roles = "User,Admin")]
		public IActionResult Show(int _Id)
		{
			Channel? _Channel = MyDataBase.Channels.Include("BindChannelUser").Include("Messages").Include("Messages.User").Where(m => m.Id == _Id).First();

			if (_Channel == null || _Channel.BindChannelUser == null || _Channel.Messages == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Could not find the channel!" });
			}

			bool _Found = false;

			foreach (BindChannelUser _Bind in _Channel.BindChannelUser)
			{
				if (_Bind.UserId == MyUserManager.GetUserId(User))
				{
					_Found = true;
					break;
				}
			}

			if (!_Found)
			{
				return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
			}

			GetUserChannels();

			ViewBag.ChatMessages = _Channel.Messages.ToList().OrderBy(m => m.Date);

			return View(_Channel);
		}

		[Authorize(Roles = "User,Admin")]
		public IActionResult Display(int _Id)
		{
			Channel? _Channel = MyDataBase.Channels.Include("BindChannelUser").Include("Category").Where(m => m.Id == _Id).First();

			if (_Channel == null || _Channel.BindChannelUser == null || _Channel.Category == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Could not find the channel!" });
			}

			bool _Found = false;

			foreach (BindChannelUser _Bind in _Channel.BindChannelUser)
			{
				if (_Bind.UserId == MyUserManager.GetUserId(User))
				{
					_Found = true;
					break;
				}
			}

			if (!_Found)
			{
				return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
			}

			GetUserChannels();

			List<BindChannelUser> _AllBinds = new List<BindChannelUser>();

			foreach (BindChannelUser _Bind in _Channel.BindChannelUser)
			{
				ApplicationUser? _Member = MyDataBase.AppUsers.Find(_Bind.UserId);

				if (_Member == null)
				{
					continue;
				}

				_AllBinds .Add(_Bind);
			}

			ViewBag.AllBinds = _AllBinds;

			return View(_Channel);
		}

		[Authorize(Roles = "User,Admin")]
		public IActionResult New()
		{
			GetUserChannels();

			ViewBag.CategoriesList = GetAllCategories();

			return View();
		}

		[HttpPost]
		[Authorize(Roles = "User,Admin")]
		public IActionResult New(Channel _Channel)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.CategoriesList = GetAllCategories();

				return View(_Channel);
			}

			try
			{
				MyDataBase.Channels.Add(_Channel);

				MyDataBase.SaveChanges();

				MyDataBase.BindChannelUserEntries.Add(new BindChannelUser { Role = "Admin", ChannelId = _Channel.Id, UserId = MyUserManager.GetUserId(User) });

				MyDataBase.SaveChanges();

				return Redirect("/Channels/Show/" + _Channel.Id);
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to create a new channel. Please contact the dev team in order to resolve this issue." });
			}
		}

		//[Authorize(Roles = "User,Admin")]
		//[HttpPost]
		//public IActionResult Join()
		//{
		//	if (!ModelState.IsValid)
		//	{
		//		return Redirect("List");
		//	}

		//	try
		//	{

		//	}
		//	catch
		//	{
		//		return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to join the channel. Please contact the dev team in order to resolve this issue." });
		//	}
		//}

		[Authorize(Roles = "User,Admin")]
		public IActionResult Edit(int _Id)
		{
			Channel? _Channel = MyDataBase.Channels.Include("BindChannelUser").Where(m => m.Id == _Id).First();

			if (_Channel == null || _Channel.BindChannelUser == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Edit attempt on non existing channel!" });
			}

			if (User.IsInRole("Moderator"))
			{
				bool _Found = false;

				foreach (BindChannelUser _Bind in _Channel.BindChannelUser)
				{
					if (_Bind.UserId == MyUserManager.GetUserId(User))
					{
						_Found = true;
						break;
					}
				}

				if (!_Found)
				{
					return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
				}
			}
			else if (User.IsInRole("User"))
			{
				foreach (BindChannelUser _Bind in _Channel.BindChannelUser)
				{
					if (_Bind.Role == "Admin")
					{
						if (_Bind.UserId != MyUserManager.GetUserId(User))
						{
							return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
						}
					}
				}
			}

			ViewBag.CategoriesList = GetAllCategories();

			return View(_Channel);
		}

		[HttpPost]
		[Authorize(Roles = "User,Admin")]
		public IActionResult Edit(int _ID, Channel _Channel)
		{
			_Channel.Id = _ID;

			Channel? _OriginalChannel = MyDataBase.Channels.Include("BindChannelUser").Where(m => m.Id == _ID).First();

			if (_OriginalChannel == null || _OriginalChannel.BindChannelUser == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Edit attempt on non existing channel!" });
			}

			if (User.IsInRole("Moderator"))
			{
				bool _Found = false;

				foreach (BindChannelUser _Bind in _Channel.BindChannelUser)
				{
					if (_Bind.UserId == MyUserManager.GetUserId(User))
					{
						_Found = true;
						break;
					}
				}

				if (!_Found)
				{
					return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
				}
			}
			else if (User.IsInRole("User"))
			{
				foreach (BindChannelUser _Bind in _Channel.BindChannelUser)
				{
					if (_Bind.Role == "Admin")
					{
						if (_Bind.UserId != MyUserManager.GetUserId(User))
						{
							return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
						}
					}
				}
			}

			if (!ModelState.IsValid)
			{
				ViewBag.CategoriesList = GetAllCategories();

				return View(_Channel);
			}

			try
			{
				_OriginalChannel.Name = _Channel.Name;
				_OriginalChannel.Description = _Channel.Description;
				_OriginalChannel.CategoryId = _Channel.CategoryId;

				MyDataBase.SaveChanges();

				return Redirect("/Channels/Display/" + _ID);
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to edit the channel. Please contact the dev team in order to resolve this issue." });
			}
		}

		[HttpPost]
		[Authorize(Roles = "User,Admin")]
		public IActionResult Delete(int _ID)
		{
			Channel? _Channel = MyDataBase.Channels.Include("BindChannelUser").Where(m => m.Id == _ID).First();

			if (_Channel == null || _Channel.BindChannelUser == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Delete attempt on non existing channel!" });
			}

			if (User.IsInRole("Moderator"))
			{
				bool _Found = false;

				foreach (BindChannelUser _Bind in _Channel.BindChannelUser)
				{
					if (_Bind.UserId == MyUserManager.GetUserId(User))
					{
						_Found = true;
						break;
					}
				}

				if (!_Found)
				{
					return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
				}
			}
			else if (User.IsInRole("User"))
			{
				foreach (BindChannelUser _Bind in _Channel.BindChannelUser)
				{
					if (_Bind.Role == "Admin")
					{
						if (_Bind.UserId != MyUserManager.GetUserId(User))
						{
							return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
						}
					}
				}
			}

			try
			{
				MyDataBase.Channels.Remove(_Channel);

				MyDataBase.SaveChanges();

				return Redirect("/Users/Show/" + MyUserManager.GetUserId(User));
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to delete the channel. Please contact the dev team in order to resolve this issue." });
			}
		}

		[NonAction]
		public IEnumerable<SelectListItem> GetAllCategories()
		{
			List<SelectListItem> _SelectList = new List<SelectListItem>();

			List<Category> _Categories = MyDataBase.Categories.ToList();

			foreach (Category _Category in _Categories)
			{
				_SelectList.Add(new SelectListItem { Value = _Category.Id.ToString(), Text = _Category.Title});
			}

			return _SelectList;
		}
	}
}
