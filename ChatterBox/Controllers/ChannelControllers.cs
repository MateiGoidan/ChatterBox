using ChatterBox.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatterBox.Controllers
{
    public class ChannelController : Controller
    {
        private readonly AppDbContext _db;

        public ChannelController(AppDbContext context)
        {
            _db = context;
        }

        public IActionResult Index()
        {
            var channels = from channel in _db.Articles
                           select channel;

            ViewBag.Articles = articles;

            return View();
        }

        public ActionResult Show(int id)
        {
            Article? channel = _db.Channel.Find(id);

            ViewBag.Channel = channel;

            return View();
        }

        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        public IActionResult New(Channel channel)
        {
            channel.Date = DateTime.Now;

            try
            {
                _db.Channel.Add(channel);

                _db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return View();
            }

        }

        public IActionResult Edit(int id)
        {
            Channel? channel = _db.Channels.Find(id);

            ViewBag.Channel = channel;

            return View();
        }

        [HttpPost]
        public ActionResult Edit(int id, Channel requestChannel)
        {
            Channel? channel = _db.Channel.Find(id);

            try
            {
                channel.Title = requestChannel.Title;
                channel.Content = requestChannel.Content;
                channel.Date = requestChannel.Date;

                _db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return RedirectToAction("Edit", channel.Id);
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            Channel? channel = _db.Channels.Find(id);

            if(channel != null)
            {
                _db.Channels.Remove(channel);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            else
            {
                return StatusCode(StatusCodes.Status404NotFound);
            } 
        }
    }
}
