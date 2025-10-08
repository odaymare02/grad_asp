using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduate.DAL.Dto.Request
{
    public class RegisterRequest
    {
        [MinLength(3)]
        public string FirstName { get; set; }
        [MinLength(4)]
        public string LastName { get; set; }
        [MinLength(6)]
        public string? UserName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
        [Compare(nameof(Password), ErrorMessage = "passwords doesn't match")]
        public string ConfirmPassword { get; set; }
    }
}
