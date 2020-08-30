using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASP_Project_DHS.Models
{
    public class LoginVM
    {
        public string UserName { get; set; }

        public string RawPWD { get; set; }

        public bool Remember { get; set; }
    }
}