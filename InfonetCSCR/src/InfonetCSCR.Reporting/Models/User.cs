using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace InfonetCSCR.Reporting.Models
{
    public class User
    {
        [Required()]
        [DisplayName("Name")]
        public string Code { get; set; }
        [Required()]
        public string Password { get; set; }
    }
}