using ASP_Project_DHS.Filters;
using ASP_Project_DHS.Helpers;
using ASP_Project_DHS.Models;
using BotDetect.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ASP_Project_DHS.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [CaptchaValidation("CaptchaCode", "DHSCaptcha", "Incorrect CAPTCHA code!")]
        public ActionResult Register(RegisterVM regModel)
        {
            Boolean mess;
            if (ModelState.IsValid)
            {
                try
                {
                    
                    TaiKhoan u = new TaiKhoan
                    {
                        TenDangNhap = regModel.UserName,
                        HoTen = regModel.Name,
                        SoDienThoai = regModel.NumPhone,
                        Email = regModel.Email,
                        MatKhau = StringUtils.GetMD5(regModel.RawPWD),
                        NgaySinh = DateTime.ParseExact(regModel.DOB, "dd/MM/yyyy", null),
                        DiaChi = regModel.Address,
                        Diem = 0,
                        DanhGiaTot = 0,
                        DanhGiaXau = 0,
                        Quyen = false,
                        Vip = false,
                        TonTai = true,
                    };
                    using (QLDauGiaEntities ctx = new QLDauGiaEntities())
                    {
                        var tktrung = ctx.TaiKhoan.Where(tt => String.Compare(tt.Email, u.Email) == 0 || String.Compare(tt.TenDangNhap, u.TenDangNhap) == 0)
                            .ToList();

                        if (tktrung.Count == 0)
                        {
                            mess = true;
                            ctx.TaiKhoan.Add(u);
                            ctx.SaveChanges();
                        }
                        else
                            mess = false;
                        
                    }
                }
                catch (Exception)
                {
                    mess = false;
                }
            }
            else
            {
                mess = false;
            }
            ViewBag.Mess = mess;
            return View();
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public ActionResult Login(LoginVM logModel)
        {
            string encPwd = StringUtils.GetMD5(logModel.RawPWD);

            using (var ctx = new QLDauGiaEntities())
            {
                var user = ctx.TaiKhoan
                    .Where(u => u.TenDangNhap == logModel.UserName && u.MatKhau == encPwd && u.TonTai == true)
                    .FirstOrDefault();

                if (user != null)
                {
                    Session["isLogin"] = 1;
                    Session["user"] = user;

                    if (logModel.Remember)
                    {
                        Response.Cookies["userIdCookie"].Value = user.ID.ToString();
                        Response.Cookies["userIdCookie"].Expires = DateTime.Now.AddDays(7);
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.ErrorMsg = "Vui lòng đăng nhập lại! Tên đăng nhập hoặc mật khẩu không đúng!";
                    return View();
                }
            }
        }

        // POST: Account/Logout
        [HttpPost]
        public ActionResult Logout()
        {
            if (CurrentContext.IsLogged() == true)
            {
                CurrentContext.Destroy();
            }
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //POST: Account/ChangePassword
        [HttpPost]
        public ActionResult ChangePassword(ChangePassVM chgPasModel)
        {
            Boolean mess;
            if (ModelState.IsValid)
            {
                try
                {
                    TaiKhoan mAcc = CurrentContext.GetCurUser();
                    string passOld = StringUtils.GetMD5(chgPasModel.RawPWDOld);
                    if (passOld.Equals(mAcc.MatKhau))
                    {
                        mAcc.MatKhau = StringUtils.GetMD5(chgPasModel.RawPWDNew);

                        using (var ctx = new QLDauGiaEntities())
                        {
                            ctx.Entry(mAcc).State = System.Data.Entity.EntityState.Modified;
                            ctx.SaveChanges();
                        }
                        mess = true;
                    }else
                    {
                        mess = false;
                    }
                }
                catch (Exception)
                {
                    mess = false;
                }
            }
            else
            {
                mess = false;
            }
            ViewBag.Mess = mess;
            return View();
        }

        // GET: Account/ThongTin
        public ActionResult ThongTin(int? id)
        {
            using (var ctx = new QLDauGiaEntities())
            {

                var tk = ctx.TaiKhoan
                    .Where(tkk => tkk.ID == id)
                    .FirstOrDefault();

                if (tk == null)
                    return RedirectToAction("Index", "ThongBao", new { ThongBao = "Không tìm thấy thông tin tài khoản! Vui lòng kiểm tra lại" });
                else
                {
                    tk.Email = CurrentContext.MaHoaEmail(tk.Email);
                    return View(tk);
                }
            }
        }

        // POST: Account/XinBan
        [HttpPost]
        [CheckLogin(RequiredPermission = false)]
        public ActionResult XinBan(int userID)
        {
            try
            {
                using (var ctx = new QLDauGiaEntities())
                {
                    TaiKhoan mtk = ctx.TaiKhoan.Where(t => t.ID == userID).FirstOrDefault();
                    mtk.ThoiGianXinBan = DateTime.Now;

                    ctx.Entry(mtk).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                    return RedirectToAction("Index", "ThongBao", new { ThongBao = "Xin quyền đăng bán sản phẩm thành công! Vui lòng chờ quản trị xác nhận!" });
                }                
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "ThongBao", new { ThongBao = "Xin quyền đăng bán thất bại! Vui lòng thử lại!" });
            }
        }

        // GET: Account/QuenMatKhau
        public ActionResult QuenMatKhau()
        {
            return View();
        }

        // POST: Account/QuenMatKhau
        [HttpPost]
        [CaptchaValidation("CaptchaCode", "DHSCaptcha", "Incorrect CAPTCHA code!")]
        public ActionResult QuenMatKhau(ResetPassVM resetModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var ctx = new QLDauGiaEntities())
                    {
                        TaiKhoan mtk = ctx.TaiKhoan.Where(t => t.TenDangNhap == resetModel.UserName && t.Email == resetModel.Email).FirstOrDefault();
                        mtk.ResetPass = true;

                        ctx.Entry(mtk).State = System.Data.Entity.EntityState.Modified;
                        ctx.SaveChanges();
                        return RedirectToAction("Index", "ThongBao", new { ThongBao = "Yêu cầu của bạn đã được ghi nhận! Vui lòng chờ quản trị xác nhận! Hãy kiểm tra mail thường xuyên để lấy lại mật khẩu" });
                    }
                }
                catch (Exception)
                {
                    return RedirectToAction("Index", "ThongBao", new { ThongBao = "Xin cấp lại mật khẩu thất bại! Vui lòng thử lại!" });
                }
            }
            else
            {
                return RedirectToAction("Index", "ThongBao", new { ThongBao = "Mã xác nhận sai! Vui lòng thử lại!" });
            }
        }
    }
}