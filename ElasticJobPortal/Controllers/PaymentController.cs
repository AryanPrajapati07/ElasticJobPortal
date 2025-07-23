using ElasticJobPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ElasticJobPortal.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IConfiguration _config;
        private readonly string key = "rzp_test_kLX7L8BYDbH8a2";
        private readonly string secret = "bqFMooSdJGjmiE7JagpU9YZS";

        public PaymentController(IConfiguration config)
        {
            _config = config;
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

        public class RazorpayOrderRequest
        {
            public decimal amount { get; set; }
            public string planName { get; set; }

        }
    }
}
