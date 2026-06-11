#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BanHang.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ResetPasswordModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [Display(Name = "Mã OTP")]
            public string Code { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "Mật khẩu phải ít nhất {2} ký tự.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Nhập lại mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không khớp.")]
            public string ConfirmPassword { get; set; }
        }

        public IActionResult OnGet(string email = null)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Yêu cầu không hợp lệ.");
            }

            Input = new InputModel
            {
                Email = email
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var savedOtp = HttpContext.Session.GetString("ResetOtp");
            var savedEmail = HttpContext.Session.GetString("ResetEmail");

            if (string.IsNullOrEmpty(savedOtp) ||
                string.IsNullOrEmpty(savedEmail) ||
                savedOtp != Input.Code ||
                savedEmail != Input.Email)
            {
                ModelState.AddModelError(string.Empty, "Mã OTP không đúng hoặc đã hết hạn.");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);

            if (user == null)
            {
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(
                user,
                token,
                Input.Password);

            if (result.Succeeded)
            {
                HttpContext.Session.Remove("ResetOtp");
                HttpContext.Session.Remove("ResetEmail");

                return RedirectToPage("./ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}