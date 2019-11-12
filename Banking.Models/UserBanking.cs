using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Banking.Models
{
    public class UserBanking : IdentityUser
    {
        public string City { get; set; }
        public string State { get; set; }
        public string Street { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }


    }
}
