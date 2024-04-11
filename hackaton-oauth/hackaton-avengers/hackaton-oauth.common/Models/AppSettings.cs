using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hackaton_oauth.common.Models
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
        public string AdminEmail { get; set; }
        public string S2SUsername { get; set; }
        public string S2SPassword { get; set; }

    }
}
