using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Magnolia_cares.authentication.entities
{

    public class loginusers
    {
        public string email { get; set; }
        public string password { get; set; }
        public string role_type { get; set; }
    }

    public class EmpMobileloginusers
    {
        public string phone { get; set; }
    }



}