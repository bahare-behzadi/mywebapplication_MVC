﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mywebapplication.Models.Models
{
    public class OrderHeader
    {
        public int OrderHeaderId { get; set; }
        [Required]
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ShippingDate { get; set; }
        public double OrderTotal {  get; set; }

        public string? OrderStatus {  get; set; }
        public string? PaymentStatus { get; set; }
        public string? TrackingNymber { get; set; }
        public string? Carrier { get; set; }

        public DateTime PaymentDate { get; set; }
        public DateOnly PaymentDue { get; set; }
        public string? SessionId { get; set; }
        public string? PaymentIntentId { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string? streetAdress { get; set; }
        [Required]
        public string? city { get; set; }
        [Required]
        public string? state { get; set; }
        [Required]
        public string? postalCode { get; set; }
        //[Required]
        //public string? PhoneNumber { get; set; }

    }
}
