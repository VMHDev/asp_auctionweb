using ASP_Project_DHS.Filters;
using ASP_Project_DHS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ASP_Project_DHS.Controllers
{
    [CheckLogin(RequiredPermission = true)]
    public class QuanTriController : Controller
    {
        // GET: QuanTri
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DanhMucNho(int page = 1)
        {
            var ctx = new QLDauGiaEntities();


            //phân trang
            int n = ctx.DanhMucLon.Count();
            int recordsPerPage = 4;
            int nPages = n / recordsPerPage;
            int m = n % recordsPerPage;
            if (m > 0)
            {
                nPages++;
            }
            ViewBag.Pages = nPages;
            ViewBag.CurPage = page;



            //xuất danh sách danh mục
            //var list = ctx.DanhMucLon.ToList();

            var list = ctx.DanhMucLon.OrderBy(p => p.ID).Skip((page - 1) * recordsPerPage).Take(recordsPerPage).ToList();

            return PartialView("PVDanhMucNho", list);
        }

        //Xuất ra danh mục nè
        public ActionResult XuatDanhMucNho(int page = 1)
        {
            var ctx = new QLDauGiaEntities();


            //phân trang
            int n = ctx.DanhMucNho.Where(dml => dml.TinhTrang != true).Count();
            int recordsPerPage = 10;
            int nPages = n / recordsPerPage;
            int m = n % recordsPerPage;
            if (m > 0)
            {
                nPages++;
            }
            ViewBag.Pages = nPages;
            ViewBag.CurPage = page;



            //xuất danh sách danh mục
            //var list = ctx.DanhMucNho.ToList();

            var list = ctx.DanhMucNho
                .Where(dmn => dmn.TinhTrang != true)
                .OrderBy(p => p.ID).Skip((page - 1) * recordsPerPage).Take(recordsPerPage).ToList();
            return PartialView("XuatDanhMucNho", list);
        }
    }
}