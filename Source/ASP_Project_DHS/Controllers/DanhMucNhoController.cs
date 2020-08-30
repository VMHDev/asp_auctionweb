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
    public class DanhMucNhoController : Controller
    {
        //Partialview hiển thị danh sách DanhMucNho
        public ActionResult DanhMucNho()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var list = ctx.DanhMucNho.Where(c => c.TinhTrang != true).ToList();
                return PartialView("PVDanhMucNho", list);
            }
        }

        //Hàm thêm danh mục ở đây nè 
        public ActionResult Add()
        {
            return View();

        }

        [HttpPost]
        public ActionResult Add(DanhMucNho model)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                ctx.DanhMucNho.Add(model);
                ctx.SaveChanges();
            }
            return View();

        }
        //Hàm xóa danh mục nè
        public ActionResult Delete(int? id)
        {
            if (id.HasValue == false)
            {
                return RedirectToAction("XuatDanhMucNho", "DanhMucNho");
            }
            ViewBag.ID = id;
            return View();

        }
        [HttpPost]
        public ActionResult Delete(int idToDelete)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                DanhMucNho model = ctx.DanhMucNho
                    .Where(c => c.ID == idToDelete)
                    .FirstOrDefault();
                model.TinhTrang = true;
                ctx.SaveChanges();
            }
            return RedirectToAction("XuatDanhMucNho", "DanhMucNho");
        }

        //hàm Edit ở đây 
        public ActionResult Edit(int? id)
        {
            if (id.HasValue == false)
            {
                return RedirectToAction("XuatDanhMucNho", "DanhMucNho");
            }
            using (var ctx = new QLDauGiaEntities())
            {
                DanhMucNho model = ctx.DanhMucNho
                    .Where(c => c.ID == id)
                    .FirstOrDefault();
                return View(model);
            }
        }
        //Update nè
        [HttpPost]
        public ActionResult Update(DanhMucNho model)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                ctx.Entry(model).State =
                    System.Data.Entity.EntityState.Modified;
                ctx.SaveChanges();
            }
            return RedirectToAction("XuatDanhMucNho", "DanhMucNho");
        }

    }
}