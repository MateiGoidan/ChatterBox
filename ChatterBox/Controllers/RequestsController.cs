using ChatterBox.Data;
using ChatterBox.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ChatterBox.Controllers
{
	public class RequestsController : Controller
	{
		private readonly ApplicationDbContext MyDataBase;
		private readonly UserManager<ApplicationUser> MyUserManager;

		public RequestsController(ApplicationDbContext _MyDataBase, UserManager<ApplicationUser> _MyUserManager)
		{
			MyDataBase = _MyDataBase;
			MyUserManager = _MyUserManager;
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
					.Where(c => c.Name.ToUpper().Contains(_Search.ToUpper()) || c.Description.ToUpper().Contains(_Search.ToUpper()))
					.Select(c => c.Id)
					.ToList();
			}

			var _Channels = MyDataBase.Channels
				.Where(c => _ChannelsIds.Contains(c.Id))
				.ToList();

			ViewBag.SearchString = _Search;

			ViewBag.UserChannels = _Channels;
		}

		[Authorize(Roles = "User, Admin")]
		public IActionResult List()
		{
			GetChannels();

			ViewBag.Requests = MyDataBase.ChannelRequests
				.Include(r => r.Channel)
				.Include(r => r.Requester)
				.Include(r => r.TargetUser)
				.ToList();

			if (TempData.ContainsKey("Message"))
			{
				ViewBag.TempMsg = TempData["Message"];
			}

			return View();
		}

		public IActionResult New()
		{
			GetChannels();

			ViewBag.Users = MyDataBase.Users;

			return View();
		}

		[HttpPost]
		public IActionResult New(ChannelRequest _Request)
		{
			if(!ModelState.IsValid)
			{
				ViewBag.Users = MyDataBase.Users;

				return View(_Request);
			}

			try
			{
				ChannelRequest Request = _Request;

				Request.CreatedAt = DateTime.Now;

				Request.Status = "Pending";

				MyDataBase.ChannelRequests.Add(_Request);

				MyDataBase.SaveChanges();

				return RedirectToAction("/Channels/List/");
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to create a request to join a channel. Please contact the dev team in order to resolve this issue." });
			}
		}

		[HttpPost]
		public async Task<IActionResult> Accept(int _Id)
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Reject(int _Id)
		{
			return View();
		}

		[HttpPost]
		public IActionResult Delete(int _Id)
		{
			return View();
		}
	}
}
