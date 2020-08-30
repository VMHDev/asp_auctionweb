using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ASP_Project_DHS.Filters;
using ASP_Project_DHS.Models;
using ASP_Project_DHS.Helpers;

namespace ASP_Project_DHS.Controllers
{
    public class YeuThichController : Controller
    {
        // GET: YeuThich
        [HttpPost]
        public ActionResult Index(int? id)
        {
            string thongbao = "";
            int slyt = 0;
            
            var tk = CurrentContext.GetCurUser();
            if (tk == null)
            {
                thongbao = "Bạn chưa đăng nhập! Vui lòng đăng nhập hoặc đăng ký mới để sử dụng được tính năng này";
            }

            else if (id.HasValue == false)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            else
            {
                using (var ctx = new QLDauGiaEntities())
                {
                    var exists = ctx.YeuThich
                        .Where(yt => yt.ID_DauGia == id && yt.ID_NguoiThich == tk.ID)
                        .FirstOrDefault();
                    slyt = ctx.YeuThich
                            .Where(yt => yt.ID_NguoiThich == tk.ID)
                            .Count();

                    if (exists != null)
                    {
                        thongbao = "Sản phẩm đã nằm trong danh sách yêu thích của bạn! Vui lòng kiểm tra lại";
                    }
                    else
                    {
                        ctx.YeuThich.Add(new YeuThich()
                        {
                            ID_DauGia = id,
                            ID_NguoiThich = tk.ID
                        });
                        ctx.SaveChanges();

                        thongbao = "Xin chức mừng! Bạn đã thêm thành công!";
                        slyt++;
                    }
                }

            }
            var kq = new { tb = thongbao, sl = slyt };
            return Json(kq, JsonRequestBehavior.AllowGet);
        }

        public ActionResult YeuThich()
        {
            using (var ctx = new QLDauGiaEntities())
            {
                var tk = CurrentContext.GetCurUser();

                    
                ViewBag.SL = ctx.YeuThich
                    .Where(yt => yt.ID_NguoiThich == tk.ID)
                    .Count();
                return PartialView("PVYeuThich");
            }
        }
    }
}