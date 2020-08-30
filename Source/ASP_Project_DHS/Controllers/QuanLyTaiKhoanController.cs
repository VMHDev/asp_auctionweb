using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ASP_Project_DHS.Models;
using ASP_Project_DHS.Filters;
using ASP_Project_DHS.Helpers;

namespace ASP_Project_DHS.Controllers
{
    [CheckLogin(RequiredPermission = false)]
    public class QuanLyTaiKhoanController : Controller
    {
        // GET: QuanLyTaiKhoan
        public ActionResult Index()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var tk = CurrentContext.GetCurUser();
                ViewBag.bam = false;
                return View(tk);
            }
        }
        [HttpPost]
        public ActionResult Index(bool? bam)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var tk = CurrentContext.GetCurUser();
                ViewBag.bam = true;
                return View(tk);
            }
        }

        // PartialView ThongTin
        public ActionResult ThongTin()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                //Lấy thông tin của user đang đăng nhập
                var tk = CurrentContext.GetCurUser();

                return PartialView("PVThongTin", tk);
            }
        }

        // GET: QuanTriNguoiDung/EditThongTin
        public ActionResult EditThongTin(int? idToEdit)
        {
            if (idToEdit.HasValue == false)
            {
                return RedirectToAction("Index", "QuanTriNguoiDung");
            }

            using (var ctx = new QLDauGiaEntities())
            {
                TaiKhoan tk = ctx.TaiKhoan.Where(t => t.ID == idToEdit).FirstOrDefault();
                return View(tk);
            }
        }

        // POST: QuanTriNguoiDung/EditThongTin
        [HttpPost]
        public ActionResult EditThongTin(EditUserVM tk)
        {
            try
            {
                using (var ctx = new QLDauGiaEntities())
                {
                    TaiKhoan mtk = ctx.TaiKhoan.Where(t => t.ID == tk.ID).FirstOrDefault();
                    mtk.TenDangNhap = tk.TenDangNhap;
                    mtk.HoTen = tk.HoTen;
                    mtk.SoDienThoai = tk.SoDienThoai;
                    mtk.Email = tk.Email;
                    mtk.NgaySinh = DateTime.ParseExact(tk.NgaySinh, "dd/MM/yyyy", null);
                    mtk.DiaChi = tk.DiaChi;

                    ctx.Entry(mtk).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "ThongBao", new { ThongBao = "Chỉnh sửa thông tin không thành công! Vui lòng thử lại!" });
            }

            return RedirectToAction("Index", "ThongBao", new { ThongBao = "Vui lòng đăng nhập lại để cập nhật sự thay đổi!" });
        }

        // PartialView YeuThich
        public ActionResult YeuThich()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                //Lấy thông tin của user đang đăng nhập
                var tk = CurrentContext.GetCurUser();

                var dsyt = ctx.YeuThich
                    .Where(yt => yt.ID_NguoiThich == tk.ID)
                    .ToList();

                var dsdg = new List<DauGia>();

                foreach (var item in dsyt)
                {
                    dsdg.Add(ctx.DauGia
                        .Where(dg => dg.ID == item.ID_DauGia)
                        .FirstOrDefault());
                }

                return PartialView("PVYeuThich", dsdg);
            }
        }

        // PartialView DanhGia
        public ActionResult DanhGia()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                //Lấy thông tin của user đang đăng nhập
                var tk = CurrentContext.GetCurUser();

                var dsnx = ctx.NhanXet
                    .Where(nx => nx.ID_NguoiDuocDanhGia == tk.ID)
                    .ToList();

                //chứa thông tin sản phẩm
                var dsdg = new List<DauGia>();

                //chứa thông tin tài khoản đánh giá
                var dstk = new List<TaiKhoan>();

                foreach (var item in dsnx)
                {
                    dsdg.Add(ctx.DauGia
                        .Where(dg => dg.ID == item.ID_DauGia)
                        .FirstOrDefault());

                    dstk.Add(ctx.TaiKhoan
                        .Where(tkj => tkj.ID == item.ID_NguoiDanhGia)
                        .FirstOrDefault());
                }

                ViewBag.DSDG = dsdg;
                ViewBag.DSTK = dstk;
                ViewBag.User = tk;

                return PartialView("PVDanhGia", dsnx);
            }
        }

        // PartialView Danh sách sản phẩm người dùng tham gia đấu giá:
        public ActionResult ThamGia()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                //Lấy thông tin của user đang đăng nhập
                var tk = CurrentContext.GetCurUser();

                //danh sách mã của sản phẩm đấu giá user này tham gia
                var ds = ctx.ChiTietDauGia
                    .Where(ctdg => ctdg.ID_TaiKhoanThamGiaDauGia == tk.ID)
                    .Select(ctdg => ctdg.ID_DauGia)
                    .Distinct()
                    .ToList();

                //danh sách sản phẩm user tham gia
                var dstg = new List<ChiTietDauGia>();

                //danh sách tên của sản phẩm
                var dsdg = new List<DauGia>();

                foreach (var item in ds)
                {
                    dstg.Add(ctx.ChiTietDauGia
                        .Where(ctdg => ctdg.ID_DauGia == item && ctdg.ID_TaiKhoanThamGiaDauGia == tk.ID)
                        .OrderByDescending(ctdg => ctdg.NgayThamGiaDauGiaGanNhat)
                        .FirstOrDefault());
                }
                dstg.Sort((a, b) => b.NgayThamGiaDauGiaGanNhat.Value.CompareTo(a.NgayThamGiaDauGiaGanNhat.Value));

                foreach (var item in dstg)
                {
                    dsdg.Add(ctx.DauGia
                        .Where(dg => dg.ID == item.ID_DauGia)
                        .FirstOrDefault());
                }

                ViewBag.DSDG = dsdg;
                return PartialView("PVThamGia", dstg);
            }
        }

        // PartialView Danh sách sản phẩm người dùng đã thắng:
        public ActionResult ThangCuoc()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                //Lấy thông tin của user đang đăng nhập
                var tk = CurrentContext.GetCurUser();

                //danh sách sản phẩm đấu giá user này trúng
                var ds = ctx.DauGia
                    .Where(dg => dg.ID_NguoiGiuGia.HasValue == false ? false : dg.ID_NguoiGiuGia == tk.ID && dg.NgayKetThuc.Value <= DateTime.Now)
                    .ToList();

                //danh sách tài khoản của đấu giá ở trên
                var dstk = new List<TaiKhoan>();

                foreach (var item in ds)
                {
                    dstk.Add(ctx.TaiKhoan
                        .Where(tt => tt.ID == item.ID_NguoiDauGia)
                        .FirstOrDefault());
                }

                ViewBag.DSND = dstk;
                return PartialView("PVThangCuoc", ds);
            }
        }

        // Người thắng sản phẩm đánh giá người bán:
        [HttpPost]
        public ActionResult ThangCuoc_DanhGia(int idnguoidang, bool like, int iddaugia, string nhanxet)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                string thongbao = "";

                if (ctx.DauGia
                    .Where(dg => dg.ID == iddaugia)
                    .FirstOrDefault()
                    .DanhGia == true)
                {
                    thongbao = "Đánh giá thất bại! Sản phẩm đã được đánh giá";
                    return RedirectToAction("Index", "ThongBao", new { ThongBao = thongbao });
                }
                //thông tin người đăng
                var ds = ctx.TaiKhoan
                    .Where(dg => dg.ID == idnguoidang)
                    .FirstOrDefault();

                if (like == true)
                {
                    if (ds.DanhGiaTot != null)
                        ds.DanhGiaTot++;
                    else
                        ds.DanhGiaTot = 1;
                }
                else
                {
                    if (ds.DanhGiaXau != null)
                        ds.DanhGiaXau++;
                    else
                        ds.DanhGiaXau = 1;
                }

                thongbao = "Đánh giá thành công!";
                ctx.SaveChanges();

                ctx.DauGia
                    .Where(dg => dg.ID == iddaugia)
                    .FirstOrDefault()
                    .DanhGia = true;
                ctx.SaveChanges();

                //thêm vào bảng nhận xét;

                var nx = ctx.NhanXet.Add(new NhanXet()
                {
                    ID_DauGia = iddaugia,
                    ID_NguoiDanhGia = CurrentContext.GetCurUser().ID,
                    ID_NguoiDuocDanhGia = idnguoidang,
                    Like = like,
                    LoiNhanXet = nhanxet,
                    Ngay = DateTime.Now,
                    LaNguoiBan = false
                });
                ctx.SaveChanges();
                return RedirectToAction("Index", "ThongBao", new { ThongBao = thongbao });
            }
        }

        // PartialView Danh sách sản phẩm người dùng đang đăng và còn hạn:
        public ActionResult DangBan()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                //lấy thông tin đang đăng nhập
                var tk = CurrentContext.GetCurUser();

                //lấy danh sách đang bán
                var dsdb = ctx.DauGia
                    .Where(dg => dg.ID_NguoiDauGia == tk.ID && dg.NgayKetThuc.Value >= DateTime.Now)
                    .ToList();
                return PartialView("PVDangBan", dsdb);
            }
        }

        // PartialView Danh sách sản phẩm hết hạn:
        public ActionResult HetHan()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                //lấy thông tin đang đăng nhập
                var tk = CurrentContext.GetCurUser();

                //lấy danh sách đã bán hết hạn
                var dshh = ctx.DauGia
                    .Where(dg => dg.ID_NguoiDauGia == tk.ID && dg.NgayKetThuc.Value < DateTime.Now)
                    .ToList();

                //danh sách người mua
                var dsnm = new List<TaiKhoan>();
                foreach (var item in dshh)
                {
                    dsnm.Add(ctx.TaiKhoan
                        .Where(tkm => tkm.ID == item.ID_NguoiGiuGia)
                        .FirstOrDefault());
                }

                ViewBag.DSNM = dsnm;
                return PartialView("PVHetHan", dshh);
            }
        }

        // Người bán sản phẩm đánh giá người mua:
        [HttpPost]
        public ActionResult HetHan_DanhGia(int idnguoigiugia, bool like, int iddaugia, string nhanxet)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                string thongbao = "";

                if (ctx.DauGia
                    .Where(dg => dg.ID == iddaugia)
                    .FirstOrDefault()
                    .NguoiBanDanhGia == true)
                {
                    thongbao = "Đánh giá thất bại! Sản phẩm đã được đánh giá";
                    return RedirectToAction("Index", "ThongBao", new { ThongBao = thongbao });
                }
                //thông tin người mua (giữ giá đến cuối cùng)
                var ds = ctx.TaiKhoan
                    .Where(dg => dg.ID == idnguoigiugia)
                    .FirstOrDefault();

                if (like == true)
                {
                    if (ds.DanhGiaTot != null)
                        ds.DanhGiaTot++;
                    else
                        ds.DanhGiaTot = 1;
                }
                else
                {
                    if (ds.DanhGiaXau != null)
                        ds.DanhGiaXau++;
                    else
                        ds.DanhGiaXau = 1;
                }

                thongbao = "Đánh giá thành công!";
                ctx.SaveChanges();

                ctx.DauGia
                    .Where(dg => dg.ID == iddaugia)
                    .FirstOrDefault()
                    .NguoiBanDanhGia = true;
                ctx.SaveChanges();

                //thêm vào bảng nhận xét;

                var nx = ctx.NhanXet.Add(new NhanXet()
                {
                    ID_DauGia = iddaugia,
                    ID_NguoiDanhGia = CurrentContext.GetCurUser().ID,
                    ID_NguoiDuocDanhGia = idnguoigiugia,
                    Like = like,
                    LoiNhanXet = nhanxet,
                    Ngay = DateTime.Now,
                    LaNguoiBan = true
                });
                ctx.SaveChanges();
                return RedirectToAction("Index", "ThongBao", new { ThongBao = thongbao });
            }
        }

        public ActionResult DangGiu(int? id)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                //lấy thông tin đang đăng nhập
                var tk = CurrentContext.GetCurUser();
                bool giu;
                //lấy danh sách đang bán
                var tkdanggiugia = ctx.DauGia
                    .Where(dg => dg.ID == id)
                    .FirstOrDefault();

                if (tkdanggiugia.ID_NguoiGiuGia == tk.ID)
                    giu  = true;
                else
                    giu = false;
                return PartialView("PVDangGiu", giu);
            }
        }
    }
}