using BanHang.Models;
using BanHang.Repositories;
using BanHang.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BanHang.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly MoMoService _moMoService;
        private readonly VNPayService _vnPayService;
        private readonly IOrderRepository _orderRepo;

        public CartController(
            ICartService cartService,
            IOrderService orderService,
            MoMoService moMoService,
            VNPayService vnPayService,
            IOrderRepository orderRepo)
        {
            _cartService = cartService;
            _orderService = orderService;
            _moMoService = moMoService;
            _vnPayService = vnPayService;
            _orderRepo = orderRepo;
        }

        [Authorize]
        public IActionResult Index()
        {
            var cart = _cartService.GetCart(HttpContext.Session);
            return View(cart);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int id, int quantity = 1, string? returnUrl = null)
        {
            if (quantity <= 0) quantity = 1;

            var success = await _cartService.AddToCart(HttpContext.Session, id, quantity);
            if (!success) return NotFound();

            TempData["success"] = "Đã thêm vào giỏ hàng!";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddToCartAjax(int id, int quantity = 1)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                return Json(new { success = false, redirectToLogin = true });

            if (quantity <= 0) quantity = 1;

            var success = await _cartService.AddToCart(HttpContext.Session, id, quantity);
            if (!success)
                return Json(new { success = false, message = "Sản phẩm không tồn tại." });

            var cart = _cartService.GetCart(HttpContext.Session);

            return Json(new
            {
                success = true,
                message = "Đã thêm vào giỏ hàng!",
                cartCount = cart.Sum(x => x.SoLuong),
                cartTotal = cart.Sum(x => x.ThanhTien),
                items = cart.Select(x => new
                {
                    hinhAnh = x.HinhAnh,
                    tenSanPham = x.TenSanPham,
                    soLuong = x.SoLuong,
                    thanhTien = x.ThanhTien
                })
            });
        }

        [Authorize]
        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            _cartService.UpdateQuantity(HttpContext.Session, id, quantity);
            TempData["success"] = "Đã cập nhật số lượng.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public IActionResult Remove(int id)
        {
            _cartService.Remove(HttpContext.Session, id);
            TempData["success"] = "Đã xóa sản phẩm.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public IActionResult ClearCart()
        {
            _cartService.Clear(HttpContext.Session);
            TempData["success"] = "Đã xoá giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = _cartService.GetCart(HttpContext.Session);
            if (cart == null || !cart.Any())
            {
                TempData["error"] = "Giỏ hàng trống!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.GioHang = cart;
            ViewBag.TongTien = cart.Sum(x => x.ThanhTien);
            return View(new ThanhToan());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(ThanhToan model)
        {
            var cart = _cartService.GetCart(HttpContext.Session);
            if (!cart.Any())
            {
                TempData["error"] = "Giỏ hàng trống!";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.GioHang = cart;
                ViewBag.TongTien = cart.Sum(x => x.ThanhTien);
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var donHang = await _orderService.CreateOrder(userId, model, cart);

            if (model.PhuongThucThanhToan == "MOMO")
            {
                var payUrl = await _moMoService.CreatePaymentAsync(
                    donHang.MaDonHang.ToString(),
                    (long)donHang.TongTien,
                    $"Thanh toán đơn hàng #{donHang.MaDonHang}");
                return Redirect(payUrl);
            }

            if (model.PhuongThucThanhToan == "VNPAY")
            {
                var paymentUrl = _vnPayService.CreatePaymentUrl(
                    HttpContext,
                    donHang.MaDonHang,
                    donHang.TongTien,
                    $"Thanh toán đơn hàng {donHang.MaDonHang}");
                return Redirect(paymentUrl);
            }

            if (model.PhuongThucThanhToan == "COD")
            {
                _cartService.Clear(HttpContext.Session);
                return RedirectToAction(nameof(Success), new { id = donHang.MaDonHang });
            }

            TempData["error"] = "Phương thức thanh toán không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public IActionResult Success(int id)
        {
            ViewBag.MaDonHang = id;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> VnpayReturn()
        {
            var isValid = _vnPayService.ValidateSignature(Request.Query, out _);
            if (!isValid)
            {
                TempData["error"] = "Dữ liệu trả về không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var responseCode = Request.Query["vnp_ResponseCode"].ToString();
            var txnRef = Request.Query["vnp_TxnRef"].ToString();

            if (int.TryParse(txnRef, out int orderId) && responseCode == "00")
            {
                await _orderRepo.UpdatePaymentAsync(orderId, true, DateTime.Now);
                _cartService.Clear(HttpContext.Session);
                return RedirectToAction(nameof(Success), new { id = orderId });
            }

            TempData["error"] = "Thanh toán thất bại.";
            return RedirectToAction(nameof(Index));
        }
    }
}