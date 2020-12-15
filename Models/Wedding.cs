using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Models
{
    public class WedDate : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if ((DateTime)value < DateTime.Now)
                return new ValidationResult("Date must be in the future");
            return ValidationResult.Success;
        }
    }
    public class Wedding
    {

        [Key]
        public int WeddingId { get; set; }

        [Required]
        [Display(Name = "Wedder One:")]
        public string WedderOne { get; set; }

        [Required]
        [Display(Name = "Wedder Two:")]
        public string WedderTwo { get; set; }

        [Required]
        // [WedDate]
        [DataType(DataType.Date)]
        public DateTime WeddingDate { get; set; }

        [Required]

        public string Address { get; set; }

        [Required]
        public int UserId {get;set;}

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public List<RSVP> Guests { get; set; }

        public User Planner {get;set;}
    }
}