using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;
using QLBH_ThuySan.Models.ViewModels;

namespace QLBH_ThuySan.Controllers
{
    /// <summary>
    /// Controller for user authentication (login/logout)
    /// </summary>
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            // If user is already authenticated, redirect to home
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Find user by username
            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.TenNguoiDung == model.TenNguoiDung);

            if (user == null || user.MatKhau != model.MatKhau)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            // Create claims for the authenticated user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.TenNguoiDung ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.MaNguoiDung),
                new Claim("QuyenNguoiDung", user.QuyenNguoiDung?.ToString() ?? "3"),
                new Claim(ClaimTypes.Role, GetRoleName(user.QuyenNguoiDung))
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // GET: Account/AccessDenied
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Get role name based on permission level
        /// </summary>
        private static string GetRoleName(int? quyenNguoiDung)
        {
            return quyenNguoiDung switch
            {
                1 => "Admin",
                2 => "Manager",
                3 => "Saler",
                _ => "Saler"
            };
        }
    }
}
