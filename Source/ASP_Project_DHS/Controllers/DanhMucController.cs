using ASP_Project_DHS.Filters;
using ASP_Project_DHS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ASP_Project_DHS.Controllers
{
    public class DanhMucController : Controller
    {

        //Xuất ra danh mục nè
        public ActionResult XuatDanhMuc(int page = 1)
        {
            var ctx = new QLDauGiaEntities();


            //phân trang
            int n = ctx.DanhMucLon.Where(dml => dml.TinhTrang != true).Count();
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

            var list = ctx.DanhMucLon
                .Where(dml => dml.TinhTrang != true)
                .OrderBy(p => p.ID).Skip((page - 1) * recordsPerPage).Take(recordsPerPage).ToList();
            return View(list);
        }

        //Partialview hiển thị danh sách DanhMucLon
        public ActionResult DanhMucLon()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var list = ctx.DanhMucLon.Where(c => c.TinhTrang != true).ToList();
                return PartialView("PVDanhMucLon", list);
            }
        }
        //Partialview hiển thị danh sách DanhMucNho thuộc id
        public ActionResult DanhMucNho(int? id)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var list = ctx.DanhMucNho
                    .Where(dmn => dmn.ID_DanhMucLon == id && dmn.TinhTrang != true)
                    .ToList();
                return PartialView("PVDanhMucNho", list);
            }
        }
        [CheckLogin(RequiredPermission = true)]
        //Hàm thêm danh mục 
        public ActionResult Add()
        {
            return View();

        }
        [CheckLogin(RequiredPermission = true)]
        [HttpPost]
        public ActionResult Add(DanhMucLon model)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                ctx.DanhMucLon.Add(model);
                ctx.SaveChanges();
            }
            return View();

        }
        [CheckLogin(RequiredPermission = true)]
        //Hàm xóa danh mục 
        public ActionResult Delete(int? id)
        {
            if (id.HasValue == false)
            {
                return RedirectToAction("XuatDanhMuc", "DanhMuc");
            }
            ViewBag.ID = id;
            return View();

        }
        [CheckLogin(RequiredPermission = true)]
        [HttpPost]
        public ActionResult Delete(int idToDelete)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                DanhMucLon model = ctx.DanhMucLon
                    .Where(c => c.ID == idToDelete)
                    .FirstOrDefault();
                model.TinhTrang = true;
                ctx.SaveChanges();
            }
            return RedirectToAction("XuatDanhMuc", "DanhMuc");
        }

        [CheckLogin(RequiredPermission = true)]
        //hàm Edit ở đây 
        public ActionResult Edit(int? id)
        {
            if (id.HasValue == false)
            {
                return RedirectToAction("XuatDanhMuc", "DanhMuc");
            }
            using (var ctx = new QLDauGiaEntities())
            {
                DanhMucLon model = ctx.DanhMucLon
                    .Where(c => c.ID == id)
                    .FirstOrDefault();


                return View(model);
            }
        }

        //Update
        [CheckLogin(RequiredPermission = true)]
        [HttpPost]
        public ActionResult Update(DanhMucLon model)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                ctx.Entry(model).State =
                    System.Data.Entity.EntityState.Modified;
                ctx.SaveChanges();
            }
            return RedirectToAction("XuatDanhMuc", "DanhMuc");
        }

    }
}