using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASP_Project_DHS.Models
{
    public class EditUserVM
    {
        public int ID { get; set; }

        public string TenDangNhap { get; set; }

        public string HoTen { get; set; }

        public string SoDienThoai { get; set; }

        public string Email { get; set; }

        public string NgaySinh { get; set; }

        public string DiaChi { get; set; }
    }
}