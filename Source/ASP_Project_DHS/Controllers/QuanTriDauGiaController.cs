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
    public class QuanTriDauGiaController : Controller
    {
        // GET: QuanTriDauGia
        public ActionResult Index(int page = 1)
        {
            using (var utx = new QLDauGiaEntities())
            {
                //Lấy danh sách các user:
                var lstUser = utx.TaiKhoan.ToList();

                //Lấy các user chưa bị xóa Exist = true và có ThoiGianXinBan !=null  và chưa được cấp quyền bán trong lstUser:
                var queryExist = from c in lstUser
                                 where c.TonTai == true && c.ThoiGianXinBan !=null && c.Vip == false
                                 select c;

                //Lấy danh sách các user chưa bị xóa và có xin bán
                var lstUserExist = queryExist.ToList();

                //Số lượng các user chưa bị xóa:
                int totalRecords = queryExist.ToList().Count();

                //Số lượng user mỗi trang:
                int recordsPerPage = 6;

                //Tổng số trang:
                int totalPages = totalRecords / recordsPerPage;
                int m = totalRecords % recordsPerPage;
                if (m > 0)
                {
                    totalPages++;
                }

                //Truyền dữ liệu ra view:
                ViewBag.Pages = totalPages;
                ViewBag.CurPage = page;

                //Danh sách các user mỗi trang:
                var lstPaging = lstUserExist
                                  .OrderBy(u => u.ThoiGianXinBan)
                                  .Skip((page - 1) * recordsPerPage)
                                  .Take(recordsPerPage)
                                  .ToList();

                return View(lstPaging);
            }
        }

        // GET: QuanTriDauGia/AcceptSell
        public ActionResult AcceptSell(int? idToAccept)
        {
            if (idToAccept.HasValue == false)
            {
                return RedirectToAction("Index", "QuanTriDauGia");
            }

            using (var ctx = new QLDauGiaEntities())
            {
                TaiKhoan tk = ctx.TaiKhoan.Where(t => t.ID == idToAccept).FirstOrDefault();
                return View(tk);
            }
        }

        // POST: QuanTriDauGia/AcceptSell
        [HttpPost]
        public ActionResult AcceptSell(TaiKhoan tk)
        {
            Boolean mess;

            try
            {
                using (var ctx = new QLDauGiaEntities())
                {
                    TaiKhoan mtk = ctx.TaiKhoan.Where(t => t.ID == tk.ID).FirstOrDefault();
                    mtk.Vip = true;

                    ctx.Entry(mtk).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                }
                mess = true;
            }
            catch (Exception)
            {
                mess = false;
            }

            ViewBag.Mess = mess;
            return View(tk);
        }
    }
}