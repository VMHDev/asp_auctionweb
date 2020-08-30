using ASP_Project_DHS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ASP_Project_DHS.Helpers;
using ASP_Project_DHS.Filters;

namespace ASP_Project_DHS.Controllers
{
    [CheckLogin]
    public class DauGiaDBController : Controller
    {
        // GET: DauGiaDB/Index. . Thêm sản phẩm đấu giá
        public ActionResult Them()
        {
            return View();
        }

        // POST: DauGiaDB/Them. Thêm sản phẩm đấu giá
        [HttpPost]
        public ActionResult Them(DauGia dg, List<HttpPostedFileBase> anhchitiet, HttpPostedFileBase anhdaidien, int? SoNgay)
        {
            if (CurrentContext.GetCurUser().Vip != true)
                return RedirectToAction("Index", "ThongBao", new { ThongBao = "Bạn không có quyền đăng bán!" });
            using (var ctx = new QLDauGiaEntities())
            {
                var ds = ctx.DauGia
                    .Add(dg);
                ds.NgayBatDau = DateTime.Now;
                ds.NgayKetThuc = ds.NgayBatDau.Value.AddDays(SoNgay.Value);
                ds.SoLuongHinh = anhchitiet.Count();
                ds.ID_NguoiDauGia = CurrentContext.GetCurUser().ID;
                ctx.SaveChanges();

                //tạo folder chứa hình [~/asets/images/DauGia/{ID}]
                string spDirPath = Server.MapPath($"~/assets/images/DauGia");
                string targetDirPath = Path.Combine(spDirPath, ds.ID.ToString());
                Directory.CreateDirectory(targetDirPath);

                //copy hình đại diện
                string daidien = Path.Combine(targetDirPath, $"daidien.jpg");
                anhdaidien.SaveAs(daidien);

                //copy hình đại diện
                for (int i = 0; i < ds.SoLuongHinh; i++)
                {
                    string thumbs = Path.Combine(targetDirPath, $"{i + 1}.jpg");
                    anhchitiet[i].SaveAs(thumbs);
                }

                return RedirectToAction("Index", "ThongBao", new { ThongBao = "Đăng sản phẩm đấu giá thành công!" });
            }
        }

        

        public int DauGiaOK(decimal buocnhay, decimal giacaonhathientai, decimal giadat, decimal gianguoigiu)
        {
            if (giadat < giacaonhathientai + buocnhay)
                return 0;
            else
            {
                if (giadat <= gianguoigiu)
                    return -1;
                else
                    return 1;
            }
        }

        public ActionResult DatGia(ChiTietDauGia ctdg)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                //lấy sản phẩm đấu giá
                var dg = ctx.DauGia
                    .Where(dgd => dgd.ID == ctdg.ID_DauGia)
                    .FirstOrDefault();

                string thongbao = "";
                int ID = CurrentContext.GetCurUser().ID;

                if (ctx.ChiTietDauGia.Count() != 0)
                {
                    //kiểm tra tài khoản đang muốn đặt giá có bị cấm k
                    var tkhientai = ctx.ChiTietDauGia
                        .Where(ctd => ctd.ID_DauGia == dg.ID && ctd.ID_TaiKhoanThamGiaDauGia == ID)
                        .FirstOrDefault();
                    if (tkhientai != null)
                    {
                        if (tkhientai.BiCam == true)
                        {
                            thongbao = "Bạn đã bị cấm khỏi phiên đấu giá này!";
                            return RedirectToAction("Index", "ThongBao", new { ThongBao = thongbao });
                        }
                    }
                }
                

                var tkyeucau = ctx.TaiKhoan
                    .Where(ttk => ttk.ID == ID)
                    .FirstOrDefault();

                if (dg.YeuCauDanhGia > ((tkyeucau.DanhGiaTot * 1.0 * 100) / (tkyeucau.DanhGiaTot + tkyeucau.DanhGiaXau)) || (tkyeucau.DanhGiaTot + tkyeucau.DanhGiaXau) == 0)
                {
                    thongbao = "Bạn không đáp ứng được yêu cầu tối thiểu để tham gia đặt giá! Vui lòng kiểm tra lại";
                }
                else if (dg.ID_NguoiGiuGia == null)
                {
                    ctdg.ID_TaiKhoanThamGiaDauGia = ID;
                    dg.ID_NguoiGiuGia = ctdg.ID_TaiKhoanThamGiaDauGia;
                    dg.GiaCaoNhatHienTai = dg.GiaKhoiDiem;
                    ctx.SaveChanges();


                    ctdg.NgayThamGiaDauGiaGanNhat = DateTime.Now;
                    ctdg.GiaHienTaiLucDat = dg.GiaKhoiDiem;
                    ctx.ChiTietDauGia.Add(ctdg);
                    ctx.SaveChanges();
                    thongbao = "Đặt giá thành công! Chúc mừng bạn là người đang giữ giá";

                    //gửi mail người yêu cầu
                    CurrentContext.SendMail(tkyeucau.Email, "Chúc mừng bạn đặt giá thành công! Sản phẩm: " + dg.Ten);

                    //gửi mail người bán
                    var nguoiban = ctx.TaiKhoan
                        .Where(tkk => tkk.ID == dg.ID_NguoiDauGia)
                        .FirstOrDefault();

                    CurrentContext.SendMail(nguoiban.Email, "Sản phẩm của bạn có người mới đấu giá thành công! Sản phẩm: " + dg.Ten);

                    //gia hạn tự động nếu có đăng ký chức năng này
                    if (dg.GiaHanTuDong == true && DateTime.Now.AddMinutes(-5) <= dg.NgayKetThuc.Value)
                    {
                        dg.NgayKetThuc.Value.AddMinutes(10);
                        ctx.SaveChanges();
                    }
                }
                else
                {


                    //lấy giá của người đang giữ giá
                    var tk = ctx.ChiTietDauGia
                        .Where(tkc => tkc.ID_TaiKhoanThamGiaDauGia == dg.ID_NguoiGiuGia && tkc.ID_DauGia == dg.ID)
                        .Max(tkc => tkc.GiaGanNhat);

                    

                    int kq = DauGiaOK(dg.BuocNhay.Value, dg.GiaCaoNhatHienTai.Value, ctdg.GiaGanNhat.Value, tk.Value);


                    //kiểm tra
                    if (kq == 0)
                        thongbao = "Đặt giá thất bại! Vui lòng đặt giá cao hơn";
                    else if (kq == -1)
                    {
                        thongbao = "Đặt giá thất bại! Vui lòng đặt giá cao hơn";
                        dg.GiaCaoNhatHienTai += dg.BuocNhay;
                        if (dg.GiaHanTuDong == true && DateTime.Now.AddMinutes(-5) <= dg.NgayKetThuc.Value)
                        {
                            dg.NgayKetThuc.Value.AddMinutes(10);
                        }
                        ctx.SaveChanges();
                    }
                    else
                    {
                        //lấy email người đang giữ giá
                        var tkgiugia = ctx.TaiKhoan
                            .Where(tkk => tkk.ID == dg.ID_NguoiGiuGia)
                            .FirstOrDefault()
                            .Email;

                        thongbao = "Đặt giá thành công! Chúc mừng bạn là người đang giữ giá";

                        //gửi mail cho người yêu cầu đấu giá
                        CurrentContext.SendMail(tkyeucau.Email, "Chúc mừng bạn đặt giá thành công!");

                        //gửi mail cho người đang giữ giá
                        CurrentContext.SendMail(tkgiugia, "Đã có người đặt giá cao hơn bạn, sản phẩm: " + dg.Ten);


                        //gia hạn tự động
                        if (dg.GiaHanTuDong == true && DateTime.Now.AddMinutes(-5) <= dg.NgayKetThuc.Value)
                        {
                            dg.NgayKetThuc.Value.AddMinutes(10);
                        }

                        dg.GiaCaoNhatHienTai += dg.BuocNhay;
                        ctdg.ID_TaiKhoanThamGiaDauGia = ID;

                        if (dg.GiaCaoNhatHienTai < tk)
                            dg.GiaCaoNhatHienTai = tk;
                        dg.ID_NguoiGiuGia = ctdg.ID_TaiKhoanThamGiaDauGia;
                        ctx.SaveChanges();  //lưu lại người giữ giá

                        //thêm vào bảng chi tiết
                        ctdg.NgayThamGiaDauGiaGanNhat = DateTime.Now;
                        ctx.ChiTietDauGia.Add(ctdg);
                        ctdg.GiaHienTaiLucDat = dg.GiaCaoNhatHienTai;
                        ctx.SaveChanges();
                    }
                }

                return RedirectToAction("Index", "ThongBao", new { ThongBao = thongbao });
            }
        }

        //Cập nhật mô tả 
        [HttpPost]
        public ActionResult CapNhatMoTaDayDu(int id_daugia, string motadaydu)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var date = DateTime.Now;
                var dsdg = ctx.DauGia
                    .Where(dg => dg.ID == id_daugia)
                    .FirstOrDefault();
                dsdg.MoTaDayDu += "&lt;br&gt;";
                var kq = "Cập nhật " + date.ToString(string.Format("(dd/MM/yyyy, HH:mm:ss)")) + ":";
                kq += "&lt;br&gt;";
                kq += motadaydu;

                var kq2 = "" + "<br/>" + "Cập nhật " + date.ToString(string.Format("(dd/MM/yyyy, HH:mm:ss)")) + ":" + "<br/>" + motadaydu;
                dsdg.MoTaDayDu += kq;
                ctx.SaveChanges();

                return Json(kq2, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Kich(int id_dg, int id_nbk)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var tb = "KICK thành công";

                string ten = null;
                string sdt = null;
                string tong = null;
                string tot = null;
                string xau = null;
                string gia = null;
                bool thaydoi = false;
                var ds = ctx.ChiTietDauGia
                    .Where(ctdg => ctdg.ID_DauGia == id_dg && ctdg.ID_TaiKhoanThamGiaDauGia == id_nbk)
                    .ToList();
                foreach (var item in ds)
                {
                    item.BiCam = true;
                }
                ctx.SaveChanges();

                //kiểm tra có đang giữ giá
                var dg = ctx.DauGia
                    .Where(dgg => dgg.ID == id_dg)
                    .FirstOrDefault();

                if (dg.ID_NguoiGiuGia == id_nbk)
                {
                    thaydoi = true;
                    var nguoiketiep = ctx.ChiTietDauGia
                        .Where(ctdg => ctdg.ID_DauGia == id_dg && ctdg.BiCam != true)
                        .OrderByDescending(ctdg => ctdg.GiaGanNhat)
                        .FirstOrDefault();

                    if (nguoiketiep == null)
                    {
                        dg.GiaCaoNhatHienTai = null;
                        dg.ID_NguoiGiuGia = null;
                        gia = String.Format("{0:N0}", dg.GiaKhoiDiem);
                    }
                    else
                    {
                        dg.GiaCaoNhatHienTai = nguoiketiep.GiaGanNhat;
                        dg.ID_NguoiGiuGia = nguoiketiep.ID_TaiKhoanThamGiaDauGia;
                        var tk_nkt = ctx.TaiKhoan
                            .Where(tk => tk.ID == nguoiketiep.ID_TaiKhoanThamGiaDauGia)
                            .FirstOrDefault();
                        ten = tk_nkt.HoTen;
                        sdt = tk_nkt.SoDienThoai;
                        gia = String.Format("{0:N0}", dg.GiaCaoNhatHienTai);
                        tong = (tk_nkt.DanhGiaTot - tk_nkt.DanhGiaXau).ToString();
                        tot = tk_nkt.DanhGiaTot.ToString();
                        xau = tk_nkt.DanhGiaXau.ToString();
                    }
                }
                ctx.SaveChanges();

                //gửi mail cho người bị kích
                var tkbk = ctx.TaiKhoan
                    .Where(tkk => tkk.ID == id_nbk)
                    .FirstOrDefault()
                    .Email;
                CurrentContext.SendMail(tkbk, "Rất tiếc! Bạn đã bị kích khỏi phiên đấu giá! Sản phẩm: " + dg.Ten);

                var kq = new { tb = tb, t = ten, sdt = sdt, dg_tong = tong, dg_tot = tot, dg_xau = xau, g = gia, td = thaydoi };
                return Json(kq, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult MuaNgay(int? id)
        {
            using (var ctx = new QLDauGiaEntities())
            {
                //lấy tài khoản mua
                var tk = CurrentContext.GetCurUser();

                //lấy sản phẩm
                var sp = ctx.DauGia
                    .Where(dg => dg.ID == id)
                    .FirstOrDefault();

                sp.ID_NguoiGiuGia = tk.ID;
                sp.GiaCaoNhatHienTai = sp.GiaMuaNgay;
                sp.NgayKetThuc = DateTime.Now.AddSeconds(-1);
                

                //gửi mail
                CurrentContext.SendMail(tk.Email, $"Chúc mừng bạn đã mua được sản phẩm! Sản phẩm: {sp.Ten}");
                CurrentContext.SendMail(ctx.TaiKhoan.Where(tkk => tkk.ID == sp.ID_NguoiDauGia).FirstOrDefault().Email, $"Đã có người mua ngay sản phẩm của bạn! Sản phẩm: {sp.Ten}");
                sp.GuiMailThatBai = true;
                ctx.SaveChanges();

                var ct = ctx.ChiTietDauGia.Add(new ChiTietDauGia() { GiaGanNhat = sp.GiaMuaNgay, ID_DauGia = id.Value, ID_TaiKhoanThamGiaDauGia = tk.ID, NgayThamGiaDauGiaGanNhat = DateTime.Now });

                ctx.SaveChanges();

                return RedirectToAction("Index", "ThongBao", new { ThongBao = "Bạn đã mua thành công"});
            }
        }
    }
}