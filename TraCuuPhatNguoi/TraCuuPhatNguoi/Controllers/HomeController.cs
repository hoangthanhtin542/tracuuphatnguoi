using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TraCuuPhatNguoi.Models;

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

        async public Task<IActionResult> TraCuu(string ten,string vehicleOption)
        {

            return await ProcessTraCuu(ten,vehicleOption);
        }

        async Task<IActionResult> ProcessTraCuu(String ten,String vehicleOption)
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

            var res = await _context.Tracuu.Where(b => b.SoChoNgoi == vehicleOption && b.BienSo == ten).FirstOrDefaultAsync();

            if (res != null)
            {
                string resultHTML = $@"
                <h6 class=""custom-heading"">Biển số xe vi phạm <span style=""color: red;"">{res.BienSo}</span></h6>
            <ul>
                <li><span>Biển Số:</span> <span>{res.BienSo}</span></li>
                <li><span>Số Seri:</span> <span>{res.SoSei}</span></li>
                <li><span>Mã Hồ Sơ:</span> <span>{res.MaHoSo}</span></li>
                <li><span>Số Chỗ Ngồi:</span> <span>{res.SoChoNgoi}</span></li>
            </ul>";

                return Content(resultHTML, "text/html");
            }
            else
            {
                return Content("<p style='color: green;'>Biển số xe không vi phạm.</p>", "text/html");
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
