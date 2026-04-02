using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BanHang.Models;
using BanHang.Services.Interfaces;

namespace BanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IAdminProductService _productService;

        public ProductController(IAdminProductService productService)
        {
            _productService = productService;
        }

        // ================== DANH SÁCH ==================
        public async Task<IActionResult> Index()
        {
            var data = await _productService.GetAllAsync();
            return View(data);
        }

        // ================== CREATE ==================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await _productService.LoadDropdownsAsync(ViewBag);
            return View(new SanPham());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPham model)
        {
            var result = await _productService.CreateAsync(model, ModelState);

            if (!result)
            {
                await _productService.LoadDropdownsAsync(ViewBag, model);
                return View(model);
            }

            TempData["success"] = "Thêm sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ================== EDIT ==================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _productService.GetByIdAsync(id);

            if (model == null)
                return NotFound();

            await _productService.LoadDropdownsAsync(ViewBag, model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SanPham model)
        {
            if (id != model.MaSanPham)
                return NotFound();

            var result = await _productService.UpdateAsync(id, model, ModelState);

            if (!result)
            {
                await _productService.LoadDropdownsAsync(ViewBag, model);
                return View(model);
            }

            TempData["success"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ================== DELETE ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteAsync(id);

            if (result)
                TempData["success"] = "Xóa sản phẩm thành công!";
            else
                TempData["error"] = "Xóa thất bại!";

            return RedirectToAction(nameof(Index));
        }
    }
}