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
					.Where(c => (c.Name.ToUpper().Contains(_Search.ToUpper()) ||
					c.Description.ToUpper().Contains(_Search.ToUpper())) && 
					_ChannelsIds.Contains(c.Id))
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

			List<Channel?> _PendingChannels = MyDataBase.BindRequestChannelUserEntries
				.Where(b => b.UserId == MyUserManager.GetUserId(User) &&
				b.Status == "Pending")
				.Select(b => b.Channel)
				.ToList();

			ViewBag.PendingChannels = _PendingChannels;

			ViewBag.Channels = MyDataBase.Channels.Include("Category").ToList();

			List<string?> _Admins = MyDataBase.BindChannelUserEntries
				.Where(b => b.Role == "Admin" && 
				b.UserId == MyUserManager.GetUserId(User))
				.Select(b => b.UserId).ToList();

			ViewBag.Admins = _Admins;

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

			GetRole(_Channel.Id);

			ViewBag.ChatMessages = _Channel.Messages.ToList().OrderBy(m => m.Date);

			return View(_Channel);
		}

		[Authorize(Roles = "User,Admin")]
		public IActionResult Display(int _Id)
		{
			Channel? _Channel = MyDataBase.Channels.Include("BindChannelUser")
				.Include("Category").Include("BindRequestChannelUser")
				.Where(m => m.Id == _Id).First();

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

			GetRole(_Channel.Id);

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

			List<BindRequestChannelUser> _ChannelRequests = new List<BindRequestChannelUser>();

			if(_Channel.BindRequestChannelUser != null)
			{
				foreach (BindRequestChannelUser _BindRequest in _Channel.BindRequestChannelUser)
				{
					ApplicationUser? _RequestUser = MyDataBase.AppUsers.Find(_BindRequest.UserId);

					if (_RequestUser == null)
					{
						continue;
					}

					Request? _Request = MyDataBase.Requests.Find(_BindRequest.RequestId);

					if (_Request == null && _Request.RequestType == null)
					{
						continue;
					}

					if (_BindRequest.Status == "Pending" && _Request.RequestType == "Join")
					{
						_ChannelRequests.Add(_BindRequest);
					}
				}
			}

			ViewBag.ChannelRequests = _ChannelRequests;

			if(_Channel.BindRequestChannelUser != null)
			{
				List<string> NonMembersIds = MyDataBase.Users
					.Where(u => !_Channel.BindChannelUser.Select(b => b.UserId).Contains(u.Id))
					.Select(u => u.Id).ToList();

				foreach (BindRequestChannelUser _BindRequest in _Channel.BindRequestChannelUser)
				{
					if(_BindRequest.Status == "Pending" && _BindRequest.UserId != null)
					{
						NonMembersIds.Remove(_BindRequest.UserId);
					}
				}

				ViewBag.NonMembersIds = NonMembersIds;
			}

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

		[Authorize(Roles = "User,Admin")]
		[HttpPost]
		public IActionResult RemoveMember(int Id, string UserId)
		{
			if (UserId == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "No user ID supplied for kick operation!" });
			}

			Channel? _Channel = MyDataBase.Channels.Include("BindChannelUser")
				.Where(m => m.Id == Id).First();

			if (_Channel == null || _Channel.BindChannelUser == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "No channel found for the ID!" });
			}

			BindChannelUser? _OriginalBind = MyDataBase.BindChannelUserEntries
				.Where(b => b.ChannelId == Id && b.UserId == UserId).First();

			if (_OriginalBind == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Remove attempt on non existing member." });
			}

			int _Admins = 0;

			foreach (BindChannelUser _Bind in _Channel.BindChannelUser)
			{
				if (_Bind.Role == "Admin")
				{
					_Admins++;
				}
			}

			if (_Admins == 1 && _OriginalBind.Role == "Admin")
			{
				return View("Error", new ErrorViewModel { RequestId = "Cannot remove the last admin from the channel!" });
			}

			try
			{
				MyDataBase.BindChannelUserEntries.Remove(_OriginalBind);

				MyDataBase.SaveChanges();
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to remove a member from the chat. Please contact the dev team in order to resolve this issue." });
			}

			if (UserId == MyUserManager.GetUserId(User))
			{
				return Redirect("/Users/Show/" + UserId);
			}
			else
			{
				return Redirect("/Channels/Info/" + Id);
			}
		}

		[Authorize(Roles ="User,Admin")]
		[HttpPost]
		public IActionResult Promote(int Id, string UserId)
		{
			BindChannelUser _Bind = MyDataBase.BindChannelUserEntries
				.Where(b => b.ChannelId == Id && b.UserId == UserId).First();

			if(_Bind == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Promote attempt on non existing member!" });
			}

			string? _OldRole = _Bind.Role;
			string _NewRole = "";

			if (_OldRole == "Member")
			{
				_NewRole = "Moderator";
			}
			else if (_OldRole == "Moderator")
			{
				_NewRole = "Admin";
			}
			else
			{
				return View("Error", new ErrorViewModel { RequestId = "User is already an admin!" });
			}
			
			try
			{
				_Bind.Role = _NewRole;

				MyDataBase.SaveChanges();

				return Redirect("/Channels/Info/" + Id);
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to promote a user. Please contact the dev team in order to resolve this issue." });
			}
		}

		[Authorize(Roles ="User,Admin")]
		[HttpPost]
		public IActionResult Demote(int Id, string UserId)
		{
			BindChannelUser _Bind = MyDataBase.BindChannelUserEntries
				.Where(b => b.ChannelId == Id && b.UserId == UserId).First();

			if(_Bind == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Demote attempt on non existing member!" });
			}

			string? _OldRole = _Bind.Role;
			string _NewRole = "";

			if (_OldRole == "Admin")
			{
				_NewRole = "Moderator";
			}
			else if (_OldRole == "Moderator")
			{
				_NewRole = "Member";
			}
			else
			{
				return View("Error", new ErrorViewModel { RequestId = "User is already a member!" });
			}
			
			try
			{
				_Bind.Role = _NewRole;

				MyDataBase.SaveChanges();

				return Redirect("/Channels/Info/" + Id);
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to demote a user. Please contact the dev team in order to resolve this issue." });
			}
		}

		[Authorize(Roles = "User,Admin")]
		[HttpPost]
		public IActionResult Delete(int _ID)
		{
			Channel? _Channel = MyDataBase.Channels.Include("BindChannelUser")
				.Where(m => m.Id == _ID).First();

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
		public void GetRole(int Id)
		{
			string? _UserId = MyUserManager.GetUserId(User);

			string? _Role = MyDataBase.BindChannelUserEntries
				.Where(b => b.ChannelId == Id && b.UserId == _UserId).Select(b => b.Role).First()
				.ToString();

			ViewBag.UserRole = _Role;
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
