using BanHang.Models;
using BanHang.Services;
using Microsoft.AspNetCore.Mvc;

namespace BanHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        public async Task<IActionResult> Index(int? maDanhMuc, string? keyword)
        {
            var (khuVucs, bestSellers) = await _homeService.GetHomeDataAsync(maDanhMuc, keyword);
            var promoProducts = await _homeService.GetPromoProductsAsync();

            ViewBag.PromoProducts = promoProducts;
            ViewBag.BestSellers = bestSellers;
            ViewBag.MaDanhMucDangChon = maDanhMuc;
            ViewBag.Keyword = keyword;

            return View(khuVucs);
        }

        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Json(new List<object>());

            var results = await _homeService.SearchSuggestionsAsync(
                keyword,
                id => Url.Action("Details", "Product", new { id })
            );

            return Json(results);
        }
    }
}