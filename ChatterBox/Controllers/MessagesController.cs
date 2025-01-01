using ChatterBox.Data;
using ChatterBox.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatterBox.Controllers
{
	public class MessagesController : Controller
	{
		private readonly ApplicationDbContext MyDataBase;
		private readonly UserManager<ApplicationUser> MyUserManager;
		private readonly IWebHostEnvironment MyHostingEnvironment;

		public MessagesController(ApplicationDbContext _MyDataBase, UserManager<ApplicationUser> _MyUserManager, IWebHostEnvironment _MyHostingEnvironment)
		{
			MyDataBase = _MyDataBase;
			MyUserManager = _MyUserManager;
			MyHostingEnvironment = _MyHostingEnvironment;
		}

		[Authorize(Roles = "Admin, User")]
		[HttpPost]
		public IActionResult New(Message _Message, IFormFile? _File)
		{
			Channel? _Channel = MyDataBase.Channels.Include(c => c.BindChannelUser)
				.Where(c => c.Id == _Message.ChannelId).First();

			if (_Channel == null || _Channel.BindChannelUser == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "The channel does not exist" });
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
				return View("Error", new ErrorViewModel { RequestId = "Access denied" });
			}

			_Message.Date = DateTime.Now;

			_Message.UserId = MyUserManager.GetUserId(User);

			if (_File != null)
			{
				if (_File.Length == 0)
				{
					return View("Error", new ErrorViewModel { RequestId = "No empty files allowed here!" });
				}

				try
				{
					if (!Directory.Exists(Path.Combine(MyHostingEnvironment.WebRootPath, "media")))
					{
						Directory.CreateDirectory(Path.Combine(MyHostingEnvironment.WebRootPath, "media"));
					}

					string _FileName = BitConverter.ToString(System.Security.Cryptography.SHA256.Create().ComputeHash(_File.OpenReadStream())).Replace("-", "").ToLowerInvariant() + Path.GetExtension(_File.FileName);

					FileStream _FileStream = new FileStream(Path.Combine(MyHostingEnvironment.WebRootPath, "media", _FileName), FileMode.Create);

					_File.CopyTo(_FileStream);

					_FileStream.Close();

					if (_File.ContentType.Contains("image"))
					{
						_Message.FilePath = "/media/" + _FileName;
						_Message.FileType = "image/" + Path.GetExtension(_FileName).Remove(0, 1);
					}
					else if (_File.ContentType.Contains("video"))
					{
						_Message.FilePath = "/media/" + _FileName;
						_Message.FileType = "video/" + Path.GetExtension(_FileName).Remove(0, 1);
					}
					else
					{
						return View("Error", new ErrorViewModel { RequestId = "Only images and videos allowed" });
					}
				}
				catch
				{
					return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to add the message. Please contact the dev team in order to resolve this issue." });
				}
			}

			ModelState.Clear();
			TryValidateModel(_Message);

			if (!ModelState.IsValid || (_Message.Content == null && _File == null))
			{
				return Redirect("/Channels/Show/" + _Message.ChannelId);
			}

			try
			{
				MyDataBase.Messages.Add(_Message);

				MyDataBase.SaveChanges();

				return Redirect("/Channels/Show/" + _Message.ChannelId);
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to write the message. Please contact the dev team in order to resolve this issue." });
			}
		}

		[Authorize(Roles = "Admin, User")]
		public IActionResult Edit(int _Id)
		{ 
			Message? _Message = MyDataBase.Messages.Find(_Id);

			if (_Message == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Edit attempt on none existing message."});
			}

			if (_Message.UserId != MyUserManager.GetUserId(User))
			{
				return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
			}

			return View(_Message);
		}

		[Authorize(Roles = "Admin, User")]
		[HttpPost]
		public IActionResult Edit(int _Id, Message _Message, IFormFile? _File)
		{
			Message _OriginalMesage = MyDataBase.Messages.Find(_Id);

			if(_OriginalMesage == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Edit attempt on none existing message." });
			}

			if(_OriginalMesage.UserId != MyUserManager.GetUserId(User))
			{
				return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
			}

			_Message.UserId = _OriginalMesage.UserId;

			if(_File != null)
			{
				if (_File.Length == 0)
				{
					return View("Error", new ErrorViewModel { RequestId = "No empty files allowed here!" });
				}

				try
				{
					if (!Directory.Exists(Path.Combine(MyHostingEnvironment.WebRootPath, "media")))
					{
						Directory.CreateDirectory(Path.Combine(MyHostingEnvironment.WebRootPath, "media"));
					}

					string _FileName = BitConverter.ToString(System.Security.Cryptography.SHA256.Create().ComputeHash(_File.OpenReadStream())).Replace("-", "").ToLowerInvariant() + Path.GetExtension(_File.FileName);

					FileStream _FileStream = new FileStream(Path.Combine(MyHostingEnvironment.WebRootPath, "media", _FileName), FileMode.Create);

					_File.CopyTo(_FileStream);

					_FileStream.Close();

					if (_File.ContentType.Contains("image"))
					{
						_Message.FilePath = "/media/" + _FileName;
						_Message.FileType = "image/" + Path.GetExtension(_FileName).Remove(0, 1);
					}
					else if (_File.ContentType.Contains("video"))
					{
						_Message.FilePath = "/media/" + _FileName;
						_Message.FileType = "video/" + Path.GetExtension(_FileName).Remove(0, 1);
					}
					else
					{
						return View("Error", new ErrorViewModel { RequestId = "Only images and videos allowed" });
					}
				}
				catch
				{
					return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to add the message. Please contact the dev team in order to resolve this issue." });
				}
			}

			ModelState.Clear();
			TryValidateModel(_Message);
			if (!ModelState.IsValid || (_Message.Content == null && _File == null))
			{
				return Redirect("/Channels/Show/" + _Message.ChannelId);
			}

			try
			{
				MyDataBase.Messages.Add(_Message);

				MyDataBase.SaveChanges();

				return Redirect("/Channels/Show/" + _Message.ChannelId);
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to add a message. Please contact the dev team in order to resolve this issue." });
			}
		}

		[Authorize(Roles = "Admin, User")]
		[HttpPost]
		public IActionResult Delete(int _Id)
		{
			Message? _Message = MyDataBase.Messages.Find(_Id);

			if(_Message == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Delete attempt on none existing message." });
			}

			int _ChannelId = _Message.ChannelId;

			string? _Role = MyDataBase.BindChannelUserEntries
				.Where(b => b.ChannelId == _ChannelId
				&& b.UserId == MyUserManager.GetUserId(User))
				.ToQueryString();

			if (_Message.UserId != MyUserManager.GetUserId(User) || _Role == "Admin" || _Role == "Moderator")
			{
				return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
			}

			try
			{
				MyDataBase.Messages.Remove(_Message);

				MyDataBase.SaveChanges();

				return Redirect("/Channels/Show/" + _ChannelId);
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to delete the message. Please contact the dev team in order to resolve this issue." });
			}
		}
	}
}
