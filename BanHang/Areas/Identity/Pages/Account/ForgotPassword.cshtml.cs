#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BanHang.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);

            if (user == null)
            {
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            var otp = new Random().Next(100000, 999999).ToString();

            HttpContext.Session.SetString("ResetEmail", Input.Email);
            HttpContext.Session.SetString("ResetOtp", otp);

            await _emailSender.SendEmailAsync(
                Input.Email,
                "Mã OTP khôi phục mật khẩu",
                $"Mã OTP khôi phục mật khẩu của bạn là: <b>{otp}</b>. Vui lòng không chia sẻ mã này cho bất kỳ ai.");

            return RedirectToPage("./ResetPassword", new { email = Input.Email });
        }
    }
}