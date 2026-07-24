using Models.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BBEcom.Controllers
{
    public class NewsController : Controller
    {
        // GET: News
        public ActionResult Index()
        {
            return View(new NewsDAO().GetNews());
        }


        public ActionResult View(int id)
        {
            var news = new NewsDAO().GetNewsById(id);
            if (news == null)
            {
                return HttpNotFound();
            }
            ViewBag.RelatedNews = new NewsDAO().GetNews().Where(x => x.Id != id).Take(4).ToList();
            return View(news);
        }
    }
}
