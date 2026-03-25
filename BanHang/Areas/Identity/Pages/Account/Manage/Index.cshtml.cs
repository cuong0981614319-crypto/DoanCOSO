using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public string Username { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        public string PhoneNumber { get; set; }
    }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        Username = user.UserName;

        Input = new InputModel
        {
            PhoneNumber = user.PhoneNumber
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        user.PhoneNumber = Input.PhoneNumber;

        await _userManager.UpdateAsync(user);

        TempData["StatusMessage"] = "Cập nhật thành công!";
        return RedirectToPage();
    }
}