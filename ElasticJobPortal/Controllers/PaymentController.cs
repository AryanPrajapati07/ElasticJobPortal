using ElasticJobPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using ElasticJobPortal.Services;

namespace ElasticJobPortal.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly string key = "rzp_test_kLX7L8BYDbH8a2";
        private readonly string secret = "bqFMooSdJGjmiE7JagpU9YZS";

        public PaymentController(IConfiguration config,UserManager<ApplicationUser> userManager,ApplicationDbContext context)
        {
            _config = config;
            _userManager = userManager;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] RazorpayOrderRequest request)
        {
            var client = new HttpClient();
            var credentials = Encoding.ASCII.GetBytes($"{key}:{secret}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));

            var orderData = new
            {
                amount = (int)(request.amount * 100), // Convert to paise
                currency = "INR",
                receipt = Guid.NewGuid().ToString(),
                payment_capture = 1 // Auto capture payment
            };

            var content = new StringContent(JsonSerializer.Serialize(orderData), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://api.razorpay.com/v1/orders", content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            return Content(jsonResponse,"application/json");
        }

        //payment success
        [HttpPost]
        public async Task<IActionResult> PaymentSuccess([FromBody] RazorpayPaymentSuccessDto paymentData)
        {
            var userId = _userManager.GetUserId(User);
            var payment = new PaymentDetail
            {
                UserId = userId,
                PlanId = paymentData.PlanId,
                RazorpayOrderId = paymentData.razorpay_order_id,
                RazorpayPaymentId = paymentData.razorpay_payment_id,
                RazorpaySignature = paymentData.razorpay_signature,
                Amount = paymentData.Amount,
                PlanName = paymentData.PlanName,
                PaymentDate = DateTime.UtcNow,
                Status = "Success"
            };

            _context.PaymentDetails.Add(payment);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Payment successful" });
        }

        public class RazorpayPaymentSuccessDto
        {
            public int PlanId { get; set; }
            public string razorpay_payment_id { get; set; }
            public string razorpay_order_id { get; set; }
            public string razorpay_signature { get; set; }
            public decimal Amount { get; set; }
            public string PlanName { get; set; }
        }
        public class RazorpayOrderRequest
        {
            public decimal amount { get; set; }
            public string planName { get; set; }

        }





    }

    
}
