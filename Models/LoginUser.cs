using System;
using System.ComponentModel.DataAnnotations;
namespace WeddingPlanner.Models
{
    public class LoginUser
    {

        [Required]
        [Display(Name = "Email:")]
        [EmailAddress]
        public string LoginEmail { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password:")]

        [Required]
        public string LoginPassword { get; set; }

    }
}