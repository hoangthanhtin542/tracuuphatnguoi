using System.Diagnostics;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TraCuuPhatNguoi.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace TraCuuPhatNguoi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TraCuu(string ten, string vehicleOption)
        {
            if (string.IsNullOrWhiteSpace(ten) || string.IsNullOrWhiteSpace(vehicleOption))
            {
                return Content("<p style='color: red;'>Thông tin tra cứu không hợp lệ.</p>", "text/html");
            }

            string carPattern = @"^[0-9]{2}[A-Za-z]{1}[-]?[0-9]{3}\.?[0-9]{2}$";
            string bikePattern = @"^[0-9]{2}[-]?[A-Za-z]{1,2}[0-9]{1,2}\.?[0-9]{2,5}$";

            bool isValid = (vehicleOption == "4" && Regex.IsMatch(ten, carPattern)) ||
                           (vehicleOption == "2" && Regex.IsMatch(ten, bikePattern));

            if (!isValid)
            {
                return Content("<p style='color: red;'>Biển số xe không hợp lệ, vui lòng nhập đúng định dạng biển số.</p>", "text/html");
            }

            using (HttpClient client = new HttpClient())
            {
                var res = await client.GetAsync($"https://www.csgt.vn/tra-cuu-phuong-tien-vi-pham.html?&LoaiXe=1&SoChoNgoi={vehicleOption}&BienSo={ten}");

                if (res != null && res.IsSuccessStatusCode)
                {
                    var dataHTML = await res.Content.ReadAsStringAsync();
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(dataHTML);

                    HtmlNode resultOk = doc.GetElementbyId("bodyPrint123");

                    if (resultOk != null)
                    {
                        return Content(resultOk.InnerHtml, "text/html");
                    }
                    else
                    {
                        return Content("<p style='color: green;'>Biển số xe không vi phạm.</p>", "text/html");
                    }
                }
            }

            return Content("<p style='color: red;'>Không thể kết nối đến hệ thống CSGT. Vui lòng thử lại sau.</p>", "text/html");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
