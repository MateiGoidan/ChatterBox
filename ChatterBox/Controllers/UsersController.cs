using ChatterBox.Data;
using ChatterBox.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

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
		public IActionResult Show(string _Id)
		{
			GetChannels();

			ApplicationUser? _User = MyDataBase.Users.Find(_Id);

			try
			{
				return View(_User);
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "Could not find the user that you are looking for!" });
			}
		}

		[Authorize(Roles = "Admin")]
		public IActionResult List()
		{
			GetChannels();

			ViewBag.Users = MyUserManager.Users.ToList();

			if (TempData.ContainsKey("Message"))
			{
				ViewBag.TempMsg = TempData["Message"];
			}

			return View();
		}

		[Authorize(Roles = "User, Admin")]
		public IActionResult Inbox(string Id)
		{
			var _User = MyDataBase.Users
					.Include(u => u.BindRequestChannelUsers)
					.ThenInclude(b => b.Request) // Include Request navigation property
					.Include(u => u.BindRequestChannelUsers)
					.ThenInclude(b => b.Channel) // Include Channel navigation property
					.FirstOrDefault(u => u.Id == Id);

			if (_User == null || _User.BindRequestChannelUsers == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "User not found!" });
			}

			GetChannels();

			List<BindRequestChannelUser> _BindList = new List<BindRequestChannelUser>();

			foreach(BindRequestChannelUser _Bind in _User.BindRequestChannelUsers)
			{
				_BindList.Add(_Bind);
			}

			ViewBag.Requests = _BindList;

			try
			{
				return View(_User);
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "Could not find the inbox of the user that you are looking for!" });
			}
		}

		[Authorize(Roles = "Admin")]
		[HttpPost]
		public IActionResult Delete(string Id)
		{
			ApplicationUser? _DeleteUser = MyDataBase.AppUsers
				.Include(u => u.BindChannelUsers)
				.Include(u => u.BindRequestChannelUsers)
				.FirstOrDefault(u => u.Id == Id);

			if (_DeleteUser == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Delete attempt on non existing user!" });
			}

			// Get all the Channels where the user is the only Admin
			List<Channel> _RemoveChannels = MyDataBase.Channels
				.Include(c => c.BindChannelUser)
				.Where(c => c.BindChannelUser.Any(b => b.UserId == Id && b.Role == "Admin")
				&& c.BindChannelUser.Count(b => b.Role == "Admin") == 1)
				.ToList();

			// Get all the request of the User
			List<Request> _RemoveRequests = MyDataBase.BindRequestChannelUserEntries
				.Where(b => b.UserId == Id)
				.Select(b => b.Request!)
				.Where(r => r != null)
				.ToList();

			try
			{
				MyDataBase.AppUsers.Remove(_DeleteUser);

				foreach (Channel _Channel in _RemoveChannels)
				{
					MyDataBase.Channels.Remove(_Channel);
				}

				foreach (Request _Request in _RemoveRequests)
				{
					MyDataBase.Requests.Remove(_Request);
				}

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
		public async Task<IActionResult> Promote(string _Id)
		{
			var _User = await MyUserManager.FindByIdAsync(_Id);

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
		public async Task<IActionResult> Demote(string _Id)
		{
			var _User = await MyUserManager.FindByIdAsync(_Id);

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
