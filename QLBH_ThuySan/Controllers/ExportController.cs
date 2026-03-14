using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Controllers
{
    [Authorize]
    public class ExportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Export
        public async Task<IActionResult> Index()
        {
            var exports = await _context.PhieuXuats
                .Include(p => p.IdDaiLyBanNavigation)
                .Include(p => p.IdKhachHangNavigation)
                .OrderByDescending(p => p.NgayXuat)
                .ToListAsync();
            return View(exports);
        }

        // GET: Export/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieuXuat = await _context.PhieuXuats
                .Include(p => p.IdDaiLyBanNavigation)
                .Include(p => p.IdKhachHangNavigation)
                .Include(p => p.ChiTietPhieuXuats)
                .ThenInclude(ct => ct.MaHangNavigation)
                .FirstOrDefaultAsync(m => m.MaPhieu == id);

            if (phieuXuat == null)
            {
                return NotFound();
            }

            return View(phieuXuat);
        }

        // GET: Export/Create
        public IActionResult Create()
        {
            ViewData["IdDaiLyBan"] = new SelectList(_context.DaiLys.Where(d => d.LoaiDaiLy == "BAN_C" || d.LoaiDaiLy == "NHAP_B"), "MaDaiLy", "TenDaiLy");
            ViewData["IdKhachHang"] = new SelectList(_context.KhachHangs, "MaDoiTuong", "TenDoiTuong");
            
            // Should pass product list with current Price as default?
            ViewData["HangHoaList"] =  _context.HangHoas.Select(h => new { h.MaHang, h.TenHang, h.DonViTinh, h.GiaBanHienTai }).ToList();
            return View();
        }

        // POST: Export/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhieu,NgayXuat,IdDaiLyBan,IdKhachHang,TrangThaiThanhToan")] PhieuXuat phieuXuat, string[] MaHang, double[] SoLuong, decimal[] GiaBan)
        {
            if (ModelState.IsValid)
            {
                // Check duplicate ID
                if (_context.PhieuXuats.Any(e => e.MaPhieu == phieuXuat.MaPhieu))
                {
                    ModelState.AddModelError("MaPhieu", "Mã phiếu đã tồn tại.");
                    ViewData["IdDaiLyBan"] = new SelectList(_context.DaiLys.Where(d => d.LoaiDaiLy == "BAN_C" || d.LoaiDaiLy == "NHAP_B"), "MaDaiLy", "TenDaiLy", phieuXuat.IdDaiLyBan);
                    ViewData["IdKhachHang"] = new SelectList(_context.KhachHangs, "MaDoiTuong", "TenDoiTuong", phieuXuat.IdKhachHang);
                    ViewData["HangHoaList"] = _context.HangHoas.Select(h => new { h.MaHang, h.TenHang, h.DonViTinh, h.GiaBanHienTai }).ToList();
                    return View(phieuXuat);
                }

                // Add Details
                if (MaHang != null && MaHang.Length > 0)
                {
                    for (int i = 0; i < MaHang.Length; i++)
                    {
                        var detail = new ChiTietPhieuXuat
                        {
                            MaPhieu = phieuXuat.MaPhieu,
                            MaHang = MaHang[i],
                            SoLuong = SoLuong[i],
                            GiaBan = GiaBan[i],
                            GiaVonTaiThoiDiem = 0 // Will be updated by SP
                        };
                        _context.Add(detail);
                    }
                }

                phieuXuat.TongTien = 0; // Will be updated by SP
                _context.Add(phieuXuat);
                await _context.SaveChangesAsync();

                // Call SP logic
                await _context.Database.ExecuteSqlRawAsync("EXEC sp_PhieuXuat_XuLy @p0", phieuXuat.MaPhieu);

                return RedirectToAction(nameof(Index));
            }
            
            ViewData["IdDaiLyBan"] = new SelectList(_context.DaiLys.Where(d => d.LoaiDaiLy == "BAN_C" || d.LoaiDaiLy == "NHAP_B"), "MaDaiLy", "TenDaiLy", phieuXuat.IdDaiLyBan);
            ViewData["IdKhachHang"] = new SelectList(_context.KhachHangs, "MaDoiTuong", "TenDoiTuong", phieuXuat.IdKhachHang);
            ViewData["HangHoaList"] = _context.HangHoas.Select(h => new { h.MaHang, h.TenHang, h.DonViTinh, h.GiaBanHienTai }).ToList();
            return View(phieuXuat);
        }

        // POST: Export/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            // Similar warning as Import: Delete does not revert business logic perfectly in simple delete SP.
            await _context.Database.ExecuteSqlRawAsync("EXEC sp_PhieuXuat_Delete @p0", id);
            return RedirectToAction(nameof(Index));
        }
    }
}
