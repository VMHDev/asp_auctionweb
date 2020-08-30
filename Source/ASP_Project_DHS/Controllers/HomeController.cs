using ASP_Project_DHS.Helpers;
using ASP_Project_DHS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ASP_Project_DHS.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        //top hot
        public ActionResult TopHot()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                //lấy 5 sản phẩm có nhiều lượt đấu giá nhất
                var ds = ctx.DauGia
                    .Join(ctx.ChiTietDauGia,
                        dg => dg.ID,
                        ctdg => ctdg.ID_DauGia,
                        (dg, ctdg) => new { dg = dg, ctdg = ctdg })
                    .Where(wdg => wdg.dg.NgayKetThuc.Value >= DateTime.Now && wdg.ctdg.BiCam != true)
                    .GroupBy(wdg => wdg.dg.ID)
                    .OrderByDescending(wdg => wdg.Count())
                    .Take(5);
                var dsdaugia = new List<DauGia>();
                var dsluotdg = new List<int>();
                foreach (var item in ds)
                {
                    dsluotdg.Add(item.Count());
                    dsdaugia.Add(item.FirstOrDefault().dg);
                }
                ViewBag.LuotDauGia = dsluotdg;
                return PartialView("PVTopHot", dsdaugia);
            }
        }

        //top giá cao
        public ActionResult TopGiaCao()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var ds = ctx.DauGia
                    .Where(dg => dg.NgayKetThuc.Value > DateTime.Now)
                    .OrderByDescending(dg => dg.GiaCaoNhatHienTai == null ? dg.GiaKhoiDiem : dg.GiaCaoNhatHienTai)
                    .Take(6)
                    .ToList();
                return PartialView("PVTopGiaCao", ds);
            }
        }

        //top sắp hết thời gian
        public ActionResult TopThoiGian()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var ds = new List<DauGia>();
                ds = ctx.DauGia
                    .Where(dg => dg.NgayKetThuc.Value > DateTime.Now)
                    .OrderBy(dg => SqlFunctions.DateDiff("ss", DateTime.Now, dg.NgayKetThuc.Value))
                    .Take(5)
                    .ToList();

                ds = ds.OrderByDescending(t => t.NgayKetThuc.Value).ToList();
                return PartialView("PVTopThoiGian", ds);
            }
        }

        public ActionResult KiemTraHetHan()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                string tb = "";
                //sản phẩm không có người đấu giá
                var dsk = ctx.DauGia
                    .Where(dg => dg.NgayKetThuc.Value < DateTime.Now && dg.GuiMailThatBai != true && dg.ID_NguoiGiuGia == null)
                    .ToList();
                if (dsk != null)
                {
                    foreach (var item in dsk)
                    {
                        var tk = ctx.TaiKhoan
                            .Where(tkk => tkk.ID == item.ID_NguoiDauGia)
                            .FirstOrDefault();

                        item.GuiMailThatBai = true;
                        //gửi mail
                        CurrentContext.SendMail(tk.Email, $"Sản phẩm của bạn không có người tham gia đấu giá! Sản phẩm: {item.Ten}");
                    }
                }

                //sản phẩm có người đấu giá
                var dsc = ctx.DauGia
                    .Where(dg => dg.NgayKetThuc.Value < DateTime.Now && dg.GuiMailThatBai != true && dg.ID_NguoiGiuGia != null)
                    .ToList();

                if (dsc != null)
                {
                    //gửi mail
                    foreach (var item in dsc)
                    {
                        //người bán
                        var tk = ctx.TaiKhoan
                            .Where(tkk => tkk.ID == item.ID_NguoiDauGia)
                            .FirstOrDefault();

                       
                        //người trúng
                        var tkt = ctx.TaiKhoan
                            .Where(tkk => tkk.ID == item.ID_NguoiGiuGia)
                            .FirstOrDefault();

                        item.GuiMailThatBai = true;
                        //gửi mail
                            CurrentContext.SendMail(tk.Email, $"Sản phẩm của bạn có người đấu giá thành công! Sản phẩm: {item.Ten}");
                            CurrentContext.SendMail(tkt.Email, $"Bạn đã là người chiến thắng! Sản phẩm: {item.Ten}");
                    }
                }
                return Json(tb, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult LienHe()
        {
            return View();
        }
        public ActionResult GioiThieu()
        {
            return View();

        }
    }
}