using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ElasticJobPortal.Models
{
    public class PaymentDetail
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public int PlanId { get; set; }

        public string RazorpayOrderId { get; set; }

        public string RazorpayPaymentId { get; set; }

        public string RazorpaySignature { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public string Status { get; set; }

        public string PlanName { get; set; }
    }
}
