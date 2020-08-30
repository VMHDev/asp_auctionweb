using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ASP_Project_DHS.Controllers
{
    public class ThongBaoController : Controller
    {
        // GET: ThongBao
        public ActionResult Index(string ThongBao)
        {
            ViewBag.ThongBao = ThongBao;
            return View();
        }

        // GET: ThongBao
        public ActionResult Permission()
        {
            var tb = "Bạn không có quyền thực hiện chức năng này!";
            return RedirectToAction("Index","ThongBao", new { ThongBao = tb});
        }
    }
}