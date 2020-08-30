using ASP_Project_DHS.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ASP_Project_DHS.Helpers;

namespace ASP_Project_DHS.Controllers
{
    public class DauGiaController : Controller
    {
        // GET: DauGia
        public ActionResult Index(int? page)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var ds = ctx.DauGia
                    .Where(dg => dg.NgayKetThuc.Value > DateTime.Now)
                    .ToList();
                int pageSize = 6;
                int pageNumber = page ?? 1;
                return View(ds.ToPagedList(pageNumber, pageSize));
            }
        }

        // GET: DauGia/ChiTiet
        public ActionResult ChiTiet(int? id)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var ds = ctx.DauGia
                    .Where(dg => dg.ID == id && dg.NgayKetThuc.Value > DateTime.Now)
                    .FirstOrDefault();
                if (ds == null)
                    return RedirectToAction("Index", "ThongBao", new { ThongBao = "Không có sản phẩm này" });
                var ngay = ds.NgayKetThuc.Value - DateTime.Now;
                ViewBag.Gio = ngay.Hours + ngay.Days*24;
                ViewBag.Phut = ngay.Minutes;
                ViewBag.Giay = ngay.Seconds;

                //lấy lượt đấu giá
                ViewBag.Luot = ctx.ChiTietDauGia
                    .Where(ldg => ldg.BiCam != true && ldg.ID_DauGia == id)
                    .Count();
                var nguoiban = ctx.TaiKhoan
                    .Where(tk => tk.ID == ds.ID_NguoiDauGia)
                    .FirstOrDefault();
                var nguoigiugia = ctx.TaiKhoan
                    .Where(tk => tk.ID == ds.ID_NguoiGiuGia)
                    .FirstOrDefault();
                ViewBag.NguoiBan = nguoiban;
                ViewBag.NguoiGiuGia = nguoigiugia;
                return View(ds);
            }
        }

        // GET: DauGia/ChiTiet. partialview
        public ActionResult TopGiaCao()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var ds = ctx.DauGia
                    .Where(dg => dg.NgayKetThuc.Value > DateTime.Now)
                    .OrderByDescending(dg => dg.GiaKhoiDiem)
                    .Take(5)
                    .ToList();
                return PartialView("PVTopGiaCao", ds);
            }
        }

        // GET: DauGia/ChiTiet. partialview
        public ActionResult TopGiaThap()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var ds = ctx.DauGia
                    .Where(dg => dg.NgayKetThuc.Value > DateTime.Now)
                    .OrderBy(dg => dg.GiaKhoiDiem)
                    .Take(5)
                    .ToList();
                return PartialView("PVTopGiaThap", ds);
            }
        }

        // GET: DauGia/ChiTiet. partialview
        public ActionResult CungChuDe(int? id) //id của danhmuc
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var ds = ctx.DauGia
                    .Where(dg => dg.ID_DanhMucNho == id && dg.NgayKetThuc.Value > DateTime.Now)
                    .Take(4)
                    .ToList();
                return PartialView("PVCungChuDe", ds);
            }
        }

        // GET: DauGia/TheoDanhMuc
        public ActionResult TheoDanhMuc(int? id, int? page) //id của danhmuc
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var ds = ctx.DauGia
                    .Where(dg => dg.ID_DanhMucNho == id && dg.NgayKetThuc.Value > DateTime.Now)
                    .ToList();
                var danhmuc = ctx.DanhMucNho
                    .Where(dmn => dmn.ID == id)
                    .FirstOrDefault();
                ViewBag.TenDanhMucNho = danhmuc.Ten;
                ViewBag.IDDanhMucNho = danhmuc.ID;
                int pageSize = 6;
                int pageNumber = page ?? 1;
                return View(ds.ToPagedList(pageNumber, pageSize));
            }
        }

        // GET: DauGia/ChiTiet. partialview
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

        // GET: DauGia/ChiTiet. partialview
        public ActionResult DanhMucNho(int? id) // id DanhMucLon
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var ds = ctx.DanhMucNho
                    .Where(dmn => dmn.ID_DanhMucLon == id && dmn.TinhTrang != true)
                    .ToList();
                return PartialView("PVDanhMucNho", ds);
            }
        }

        //GET: partialview lịch sử đấu giá, nhận vào id của sản phẩm
        public ActionResult LichSu(int? id) // id DanhMucLon
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var ds = ctx.ChiTietDauGia
                    .Where(ctdg => ctdg.ID_DauGia == id && ctdg.BiCam != true)
                    .OrderByDescending(ctdg => ctdg.NgayThamGiaDauGiaGanNhat.Value)
                    .ToList();
                var dstk = new List<TaiKhoan>();
                foreach (var item in ds)
                {
                    var temp = ctx.TaiKhoan
                        .Where(tk => tk.ID == item.ID_TaiKhoanThamGiaDauGia)
                        .FirstOrDefault();
                    temp.Email = CurrentContext.MaHoaEmail(temp.Email);
                    dstk.Add(temp);

                }

                ViewBag.ID_NguoiDang = ctx.DauGia
                    .Where(dg => dg.ID == id)
                    .FirstOrDefault()
                    .ID_NguoiDauGia;

                ViewBag.DSTK = dstk;
                return PartialView("PVLichSu", ds);
            }
        }
    }
}