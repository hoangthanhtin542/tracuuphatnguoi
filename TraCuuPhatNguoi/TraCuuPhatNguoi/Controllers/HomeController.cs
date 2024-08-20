using System.Diagnostics;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TraCuuPhatNguoi.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Net.Http.Headers;
using System;
using Azure.Core;

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
            string electroPattern = @"^[0-9]{2}[-]?[A-Za-z]{1,2}[0-9]{1,2}\.?[0-9]{2,5}$";


            bool isValid = (vehicleOption == "1" && Regex.IsMatch(ten, carPattern)) ||
                           (vehicleOption == "2" && Regex.IsMatch(ten, bikePattern))||
                           (vehicleOption == "3" && Regex.IsMatch(ten, electroPattern));


            if (!isValid)
            {
                return Content("<p style='color: red;'>Biển số xe không hợp lệ, vui lòng nhập đúng định dạng biển số.</p>", "text/html");
            }

            var localResult = await _context.Tracuu
                .Where(b => b.SoChoNgoi == vehicleOption && b.BienSo == ten)
                .FirstOrDefaultAsync();

            HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = true 
            };


            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("vi-VN,vi;q=0.9,fr-FR;q=0.8,fr;q=0.7,en-US;q=0.6,en;q=0.5");
                client.DefaultRequestHeaders.Connection.Clear();
                client.DefaultRequestHeaders.Connection.Add("keep-alive");
                client.DefaultRequestHeaders.Referrer = new Uri("https://phatnguoixe.com/");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://phatnguoixe.com");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                var cookieHeader = "_ga=GA1.1.704997958.1723734301; PHPSESSID=h2od860pj9gh6jugbubogdcdl1; __zi=3000.SSZzejyD2DOgdkEjonOFoJhVwRh36a249v7dee16KeuuWwMmbbKGcd_Efx3I71EKTCAYzD4J7jvmmg_rr1KOq3G.1; BienSo=30A77777; _ga_SGEN7V4BBG=GS1.1.1724135863.6.0.1724135863.0.0.0; __gads=ID=7b7361fdbcfa4573:T=1723734302:RT=1724135864:S=ALNI_MZ12zRbxfd_coUmiHf_xSDqIzuFfg; __gpi=UID=00000ebf9f84cb4f:T=1723734302:RT=1724135864:S=ALNI_MYF4MpyuJGeBAmyQs0UCDeCLEZwpw; __eoi=ID=f7bcd22d0e9c7350:T=1723734302:RT=1724135864:S=AA-AfjaZCBBXKNar9UhKn8mnL4NT; FCNEC=%5B%5B%22AKsRol8LyBIzN6MPdPQpkZfINFR_H0u10CXrLY-0f6Pj7KaE-D8XcJ9lOHOzPZKBsYnntLnXxHCmjVJmQo6NzDhx49StOyuvr17PESYM4OeiMtF-8BBaFfQ5tVnxpyguYRkRR318PqCY1rpmNbPpGcnTNPF2P7KIHw%3D%3D%22%5D%5D";
                client.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", cookieHeader);

                client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
                client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\"Not)A;Brand\";v=\"99\", \"Google Chrome\";v=\"127\", \"Chromium\";v=\"127\"");
                client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                client.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");

                var payload = new Dictionary<string, string>
                {
                    {"BienSo", ten},
                    {"LoaiXe",vehicleOption.ToString()}
                };
                var formData = new FormUrlEncodedContent(payload);
                HttpResponseMessage response = await client.PostAsync("https://phatnguoixe.com/1026", formData);
                if (response.IsSuccessStatusCode)
                {
                var result = await response.Content.ReadAsStringAsync();
                return Content(result,"text/html");
                }
                else{

                return Content("<p style='color: red;'>Không thể kết nối đến hệ thống CSGT. Vui lòng thử lại sau.</p>", "text/html");
                }
            }
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
