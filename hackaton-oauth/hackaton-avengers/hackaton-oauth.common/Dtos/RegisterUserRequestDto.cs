using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hackaton_oauth.common.Dtos
{
    public class RegisterUserRequestDto
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string RoleName { get; set; }
    }
}
