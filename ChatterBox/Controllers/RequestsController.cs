using ChatterBox.Data;
using ChatterBox.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

		[Authorize(Roles = "User, Admin")]
		[HttpPost]
		public IActionResult New(Request _Request, int _ChannelId, string _UserId)
		{
			Channel? _Channel = MyDataBase.Channels.Find(_ChannelId);

			if (_Channel == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "The channel you are trying to join does not exist." });
			}

			ApplicationUser? _User = MyUserManager.FindByIdAsync(_UserId).Result;

			if (_User == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "User not found." });
			}

			if (!ModelState.IsValid)
			{
				return Redirect("/Channels/List/");
			}

			try
			{
				_Request.CreatedAt = DateTime.Now;

				MyDataBase.Requests.Add(_Request);

				MyDataBase.SaveChanges();

				BindRequestChannelUser _BindRequestChannelUser = new BindRequestChannelUser
				{
					ChannelId = _ChannelId,
					UserId = _UserId,
					RequestId = _Request.Id,
					Status = "Pending"
				};

				MyDataBase.BindRequestChannelUserEntries.Add(_BindRequestChannelUser);

				MyDataBase.SaveChanges();
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to join the channel. Please contact the dev team in order to resolve this issue." });
			}

			if(_User.Id == MyUserManager.GetUserId(User))
			{
				return Redirect("/Channels/List/");
			}
			else
			{
				return Redirect("/Channels/Info/" + _ChannelId);
			}
		}

		[Authorize(Roles = "User, Admin")]
		[HttpPost]
		public IActionResult Accept(int Id)
		{
			Request? _Request = MyDataBase.Requests.Find(Id);

			if (_Request == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "The request you are trying to accept does not exist." });
			}

			BindRequestChannelUser? _BindRequestChannelUser = MyDataBase.BindRequestChannelUserEntries
				.FirstOrDefault(b => b.RequestId == Id);

			if (_BindRequestChannelUser == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "The request you are trying to accept does not exist." });
			}

			try
			{
				_Request.ProcessedAt = DateTime.Now;
				_BindRequestChannelUser.Status = "Accepted";

				BindChannelUser _BindChanelUser = new BindChannelUser
				{
					ChannelId = _BindRequestChannelUser.ChannelId,
					UserId = _BindRequestChannelUser.UserId,
					Role = "Member"
				};

				MyDataBase.BindChannelUserEntries.Add(_BindChanelUser);
				MyDataBase.SaveChanges();

			}
			catch (Exception ex)
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occurred: " + ex.Message });
			}

			if(_BindRequestChannelUser.UserId == MyUserManager.GetUserId(User))
			{
				return Redirect("/Users/Inbox/" + _BindRequestChannelUser.UserId);
			}
			else
			{
				return Redirect("/Channels/Info/" + _BindRequestChannelUser.ChannelId);
			}
		}

		[Authorize(Roles = "User, Admin")]
		[HttpPost]
		public IActionResult Decline(int Id)
		{
			Request? _Request = MyDataBase.Requests.Find(Id);

			if (_Request == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "The request you are trying to accept does not exist." });
			}

			BindRequestChannelUser? _BindRequestChannelUser = MyDataBase.BindRequestChannelUserEntries
				.FirstOrDefault(b => b.RequestId == Id);

			if (_BindRequestChannelUser == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "The request you are trying to accept does not exist." });
			}

			try
			{
				MyDataBase.Requests.Remove(_Request);

				MyDataBase.SaveChanges();
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to decline a request. Please contact the dev team in order to resolve this issue." });
			}

			if(_BindRequestChannelUser.UserId == MyUserManager.GetUserId(User))
			{
				return Redirect("/Users/Inbox/" + _BindRequestChannelUser.UserId);
			}
			else
			{
				return Redirect("/Channels/Info/" + _BindRequestChannelUser.ChannelId);
			}
		}

		[Authorize(Roles = "User, Admin")]
		[HttpPost]
		public IActionResult Delete(int Id)
		{
			Request? _Request = MyDataBase.Requests.Find(Id);

			if(_Request == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "The request you are trying to delete does not exist." });
			}

			try
			{
				MyDataBase.Requests.Remove(_Request);

				MyDataBase.SaveChanges();

				return Redirect("/Users/Inbox/" + MyUserManager.GetUserId(User));
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to delete a request. Please contact the dev team in order to resolve this issue." });
			}
		}
	}
}
