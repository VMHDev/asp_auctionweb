using ASP_Project_DHS.Filters;
using ASP_Project_DHS.Helpers;
using ASP_Project_DHS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ASP_Project_DHS.Controllers
{
    [CheckLogin(RequiredPermission = true)]
    public class QuanTriNguoiDungController : Controller
    {
        // GET: QuanTriNguoiDung
        public ActionResult Index(int page = 1)
        {
            using (var utx = new QLDauGiaEntities())
            {
                //Lấy danh sách các user:
                var lstUser = utx.TaiKhoan.ToList();

                //Lấy các user chưa bị xóa Exist = true trong lstUser:
                var queryExist = from c in lstUser
                                 where c.TonTai == true
                                 select c;

                //Lấy danh sách các user chưa bị xóa
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
                                  .OrderBy(u => u.ID)
                                  .Skip((page - 1) * recordsPerPage)
                                  .Take(recordsPerPage)
                                  .ToList();

                return View(lstPaging);
            }
        }

        // GET: QuanTriNguoiDung/DeleteUser
        public ActionResult DeleteUser(int? idToDelete)
        {
            if (idToDelete.HasValue == false)
            {
                return RedirectToAction("Index", "QuanTriNguoiDung");
            }

            using (var ctx = new QLDauGiaEntities())
            {
                TaiKhoan tk = ctx.TaiKhoan.Where(t => t.ID == idToDelete).FirstOrDefault();
                return View(tk);
            }
        }

        // POST: QuanTriNguoiDung/DeleteUser
        [HttpPost]
        public ActionResult DeleteUser(TaiKhoan tk)
        {
            Boolean mess;

            try
            {
                using (var ctx = new QLDauGiaEntities())
                {
                    TaiKhoan mtk = ctx.TaiKhoan.Where(t => t.ID == tk.ID).FirstOrDefault();
                    mtk.TonTai = false;

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

        // GET: QuanTriNguoiDung/ResetPass
        public ActionResult ResetPass(int page = 1)
        {
            using (var utx = new QLDauGiaEntities())
            {
                //Lấy danh sách các user:
                var lstUser = utx.TaiKhoan.ToList();

                //Lấy các user chưa bị xóa Exist = true và có ThoiGianXinBan !=null  và chưa được cấp quyền bán trong lstUser:
                var queryExist = from c in lstUser
                                 where c.TonTai == true && c.ResetPass == true
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

        // POST: QuanTriNguoiDung/ResetPass
        [HttpPost]
        public ActionResult ResetPass(int? id)
        {
            string tb = "Reset mật khẩu người dùng thành công!";

            using (var ctx = new QLDauGiaEntities())
            {
                TaiKhoan mtk = ctx.TaiKhoan.Where(t => t.ID == id).FirstOrDefault();
                mtk.ResetPass = false;
                mtk.MatKhau = StringUtils.GetMD5("000000");

                ctx.Entry(mtk).State = System.Data.Entity.EntityState.Modified;
                ctx.SaveChanges();
                var kq = new { tb = tb };

                CurrentContext.SendMail(mtk.Email, "Tài khoản của bạn đã được reset thành công! Mật khẩu mới là: 000000");
                return Json(kq, JsonRequestBehavior.AllowGet);
            }
          
        }
    }
}