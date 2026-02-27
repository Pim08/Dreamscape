using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dreamscape.Data
{
    public class User
    {
        public const int ROLE_USER = 1;
        public const int ROLE_OWNER = 2;

        public int Id { get; set; }

        [Required]
        public string username { get; set; }

        [Required]
        public string password_hash { get; set; }

        [Required, EmailAddress]
        public string Emailadress { get; set; }

        public int Role { get; set; }

        // Ingelogde gebruiker bijhouden
        public static User LoggedInUser { get; set; }
    }
}
