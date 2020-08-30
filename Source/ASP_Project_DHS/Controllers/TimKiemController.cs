using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ASP_Project_DHS.Models;
using PagedList;
using ASP_Project_DHS.Helpers;

namespace ASP_Project_DHS.Controllers
{
    public class TimKiemController : Controller
    {
        // GET: TimKiem
        public ActionResult Index(bool? TG_Tang, bool? Gia_Tang, string ten_SP, int? id_DM, int? page)
        {
            if (ten_SP == null)
                ten_SP = "";
            using (var ctx = new QLDauGiaEntities())
            {
                var ds = new List<DauGia>();
                if (TG_Tang != null)
                {
                    if (TG_Tang == true)
                    {
                        ds = ctx.DauGia
                            .Where(dg => (id_DM == 0 ? true : dg.ID_DanhMucNho == id_DM) && dg.NgayKetThuc.Value > DateTime.Now && (ten_SP == "" ? dg.Ten.ToLower().Contains("") : dg.Ten.ToLower().Contains(ten_SP.ToLower())))
                            .OrderBy(dg => dg.NgayKetThuc.Value)
                            .ToList();
                    }
                    else
                    {
                        ds = ctx.DauGia
                            .Where(dg => (id_DM == 0 ? true : dg.ID_DanhMucNho == id_DM) && dg.NgayKetThuc.Value > DateTime.Now && (ten_SP == "" ? dg.Ten.ToLower().Contains("") : dg.Ten.ToLower().Contains(ten_SP.ToLower())))
                            .OrderByDescending(dg => dg.NgayKetThuc.Value)
                            .ToList();
                    }
                }
                else if (Gia_Tang != null)
                {
                    if (Gia_Tang == true)
                    {
                        ds = ctx.DauGia
                            .Where(dg => (id_DM == 0 ? true : dg.ID_DanhMucNho == id_DM) && dg.NgayKetThuc.Value > DateTime.Now && (ten_SP == "" ? dg.Ten.ToLower().Contains("") : dg.Ten.ToLower().Contains(ten_SP.ToLower())))
                            .OrderBy(dg => dg.GiaCaoNhatHienTai == null ? dg.GiaKhoiDiem : dg.GiaCaoNhatHienTai)
                            .ToList();
                    }
                    else
                    {
                        ds = ctx.DauGia
                            .Where(dg => (id_DM == 0 ? true : dg.ID_DanhMucNho == id_DM) && dg.NgayKetThuc.Value > DateTime.Now && (ten_SP == "" ? dg.Ten.ToLower().Contains("") : dg.Ten.ToLower().Contains(ten_SP.ToLower())))
                            .OrderByDescending(dg => dg.GiaCaoNhatHienTai == null ? dg.GiaKhoiDiem : dg.GiaCaoNhatHienTai)
                            .ToList();
                    }
                }

                //loại bỏ những sản phẩm nằm trong những danh mục bị xóa

                var dml = ctx.DanhMucLon
                    .Where(dmll => dmll.TinhTrang == true)
                    .ToList();
                foreach (var item in dml)
                {
                    var dmn = ctx.DanhMucNho
                        .Where(dmnn => dmnn.ID_DanhMucLon == item.ID)
                        .ToList();
                    foreach (var item_n in dmn)
                    {
                        ds.RemoveAll(rm => rm.ID_DanhMucNho == item_n.ID);
                    }
                }

                var dmn_x = ctx.DanhMucNho
                    .Where(dmnn => dmnn.TinhTrang == true)
                    .ToList();

                foreach (var item in dmn_x)
                {
                    ds.RemoveAll(re => re.ID_DanhMucNho == item.ID);
                }


                ViewBag.TG_Tang = TG_Tang;
                ViewBag.Gia_Tang = Gia_Tang;
                ViewBag.ID_DM = id_DM;
                ViewBag.Ten_SP = ten_SP;
                int pageSize = 6;
                int pageNumber = page ?? 1;
                return View(ds.ToPagedList(pageNumber, pageSize));
            }
        }

        public ActionResult Home()
        {
            return View();
        }

        //Partialview combobox danh mục lớn
        public ActionResult DanhMucLon()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var ds = ctx.DanhMucLon
                    .Where(dml => dml.TinhTrang != true)
                    .ToList();
                return PartialView("PVDanhMucLon", ds);
            }
        }

        //Partialview combobox danh mục nhỏ
        public ActionResult DanhMucNho(int? id)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var ds = ctx.DanhMucNho
                    .Where(dmn => dmn.ID_DanhMucLon == id && dmn.TinhTrang != true)
                    .ToList();
                return PartialView("PVDanhMucNho", ds);
            }
        }

        public ActionResult LuotDauGia(int? id)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                ViewBag.SL = ctx.ChiTietDauGia
                    .Where(ctdg => ctdg.ID_DauGia == id && ctdg.BiCam != true)
                    .Count();
                return PartialView("PVLuotDauGia");
            }
        }

        public ActionResult NguoiGiuGia(int? id)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                string ngg = null;
                var tk = ctx.DauGia
                    .Where(dg => dg.ID == id)
                    .FirstOrDefault();

                if (tk.ID_NguoiGiuGia == null)
                {
                    ngg = ctx.TaiKhoan
                        .Where(tt => tt.ID == tk.ID_NguoiDauGia)
                        .FirstOrDefault()
                        .HoTen;
                }
                else
                {
                    ngg = ctx.TaiKhoan
                        .Where(tt => tt.ID == tk.ID_NguoiGiuGia)
                        .FirstOrDefault()
                        .HoTen;
                }
                ngg = CurrentContext.MaHoaTen(ngg);
                ViewBag.NguoiGiuGia = ngg;
                return PartialView("PVNguoiGiuGia");
            }
        }
    }
}