using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASP_Project_DHS.Models
{
    public class RegisterVM
    {
        public string UserName { get; set; }

        public string Name { get; set; }

        public string NumPhone { get; set; }

        public string Email { get; set; }

        public string DOB { get; set; }

        public string Address { get; set; }

        public string RawPWD { get; set; }
    }
}