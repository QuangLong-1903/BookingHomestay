using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DoAnCoSo_Nhom2.Models;
using DoAnCoSo_Nhom2.ViewModels;
using DoAnCoSo_Nhom2.Data;
using Microsoft.EntityFrameworkCore;
using DoAnCoSo_Nhom2.Services.ActivityLog;

namespace DoAnCoSo_Nhom2.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ActivityLogService _logService;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ApplicationDbContext context,
            IConfiguration configuration,
            ActivityLogService logService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _configuration = configuration;
            _logService = logService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    Address = model.Address,
                    DateOfBirth = model.DateOfBirth,
                    PhoneNumber = model.PhoneNumber
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Customer");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["AccountRegisterMessage"] = "Bạn đã đăng ký thành công với vai trò: Customer, Nếu muốn làm Staff hãy liên lạc với Admin qua các đường dẫn ở dưới nhé!";
                    return RedirectToAction("Register", "Account");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
                        return View(model);
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa vì nhập sai quá nhiều lần. Vui lòng thử lại sau.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Đăng nhập không hợp lệ.");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Manage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfile
            {
                FullName = user.FullName,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditProfile(EditProfile model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = model.FullName;
            user.Address = model.Address;
            user.DateOfBirth = model.DateOfBirth;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _logService.LogAsync(user.Id, "Chỉnh sửa hồ sơ");

                TempData["AccountSuccess"] = "Thông tin đã được cập nhật!"; 
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToAction("EditProfile", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ActivityLog()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var logs = await _context.UserActivityLogs
                .Where(l => l.UserId == user.Id)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            return View(logs);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ClearActivityLog()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var logs = _context.UserActivityLogs.Where(l => l.UserId == user.Id);
            _context.UserActivityLogs.RemoveRange(logs);
            await _context.SaveChangesAsync();
            TempData["AccountSuccess"] = "Lịch sử hoạt động đã được xóa!"; 
            return RedirectToAction("ActivityLog");
        }
    }
}