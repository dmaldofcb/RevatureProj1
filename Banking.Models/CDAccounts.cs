using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using CustomDataAnnotations;

namespace Banking.Models
{
    public class CDAccounts
    {
        public int Id { get; set; }
        [DisplayFormat(DataFormatString = "{0:c}")]

        [Range(.01, Double.PositiveInfinity)]
        [DisplayName("CD Balance")]
        public decimal Balance { get; set; }
        [DisplayName("Account Status")]
        public bool IsOpen { get; set; }

        [DisplayFormat(DataFormatString = "{0:P2}")]
        [DisplayName("Interest Rate")]
        public decimal InterestRate { get; set; }

        [DisplayName("Account Number")]
        public string AccountNumber { get; set; }


        [DisplayName("Maturity Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [CurrentDate(ErrorMessage ="Maturity Date must be after current date")]
        public DateTime MaturityDate { get; set; }


        [DisplayName("Creation Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime CreationDate { get; set; }

        public string UserID { get; set; }
    }
}
