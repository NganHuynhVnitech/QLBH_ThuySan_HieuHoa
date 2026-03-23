using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using QLBH_ThuySan.Models;
using QLBH_ThuySan.ViewModels;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace QLBH_ThuySan.Controllers
{
    [Authorize]
    public class OpeningStockController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        private string DraftFilePath => Path.Combine(_env.ContentRootPath, "App_Data", "OpeningStockDrafts.json");

        public OpeningStockController(ApplicationDbContext context, Microsoft.Extensions.Configuration.IConfiguration config, IWebHostEnvironment env)
        {
            _context = context;
            _config = config;
            _env = env;

            var appDataPath = Path.Combine(_env.ContentRootPath, "App_Data");
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
        }

        private bool IsFinalized()
        {
            return _config.GetValue<bool>("IsOpeningStockFinalized");
        }

        private List<OpeningStockDraftViewModel> GetDrafts()
        {
            if (!System.IO.File.Exists(DraftFilePath)) return new List<OpeningStockDraftViewModel>();
            try
            {
                var json = System.IO.File.ReadAllText(DraftFilePath);
                return JsonSerializer.Deserialize<List<OpeningStockDraftViewModel>>(json) ?? new List<OpeningStockDraftViewModel>();
            }
            catch
            {
                return new List<OpeningStockDraftViewModel>();
            }
        }

        private void SaveDrafts(List<OpeningStockDraftViewModel> drafts)
        {
            var json = JsonSerializer.Serialize(drafts);
            System.IO.File.WriteAllText(DraftFilePath, json);
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.IsFinalized = IsFinalized();

            if (ViewBag.IsFinalized)
            {
                var finalizedData = await _context.ChiTietPhieuNhaps
                    .Include(c => c.MaPhieuNavigation)
                    .Include(c => c.MaHangNavigation)
                    .Where(c => c.MaPhieuNavigation.IdNhaCungCap == "SYS_OPENING_STOCK")
                    .ToListAsync();
                
                return View(finalizedData);
            }
            else
            {
                ViewBag.Khos = await _context.Khos.ToListAsync();
                ViewBag.HangHoas = await _context.HangHoas.ToListAsync();
                var drafts = GetDrafts();
                return View(drafts);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddDraft([FromBody] OpeningStockDraftViewModel model)
        {
            if (IsFinalized()) return BadRequest("Tồn kho đã được chốt.");

            if (string.IsNullOrEmpty(model.MaKho) || string.IsNullOrEmpty(model.MaHang) || model.SoLuong < 0 || model.GiaVon < 0)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            var hanghoa = await _context.HangHoas.FindAsync(model.MaHang);
            if (hanghoa == null) return NotFound("Hàng hóa không tồn tại.");
            model.TenHang = hanghoa.TenHang;

            var drafts = GetDrafts();
            var existing = drafts.FirstOrDefault(d => d.MaKho == model.MaKho && d.MaHang == model.MaHang);
            if (existing != null)
            {
                existing.SoLuong += model.SoLuong;
                existing.GiaVon = model.GiaVon; // Cập nhật giá vốn mới nhất
            }
            else
            {
                drafts.Add(model);
            }

            SaveDrafts(drafts);
            return Ok(new { success = true });
        }

        [HttpPost]
        public IActionResult DeleteDraft(string maKho, string maHang)
        {
            if (IsFinalized()) return BadRequest("Tồn kho đã được chốt.");

            var drafts = GetDrafts();
            var item = drafts.FirstOrDefault(d => d.MaKho == maKho && d.MaHang == maHang);
            if (item != null)
            {
                drafts.Remove(item);
                SaveDrafts(drafts);
            }
            return Ok(new { success = true });
        }

        public async Task<IActionResult> DownloadTemplate()
        {
            var hangHoas = await _context.HangHoas.Select(h => new { h.MaHang, h.TenHang }).ToListAsync();
            var khos = await _context.Khos.Select(k => new { k.MaKho, k.TenKho }).ToListAsync();

            var template = new[] { new { MaHang = "", TenHang = "", MaKho = "", SoLuong = 0.0, GiaVon = 0m } };

            var memoryStream = new MemoryStream();
            var sheets = new Dictionary<string, object>
            {
                { "NhapTonKho", template },
                { "DanhSachHangHoa", hangHoas },
                { "DanhSachKho", khos }
            };

            MiniExcel.SaveAs(memoryStream, sheets);
            memoryStream.Position = 0;

            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_TonKhoBanDau.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (IsFinalized()) return BadRequest("Tồn kho đã được chốt.");
            if (file == null || file.Length == 0) return BadRequest("File trống.");

            var drafts = GetDrafts();
            int successCount = 0;
            var errors = new List<string>();

            using (var stream = file.OpenReadStream())
            {
                var rows = stream.Query(useHeaderRow: true, sheetName: "NhapTonKho").ToList();
                int rowNum = 1;
                foreach (var row in rows)
                {
                    rowNum++;
                    string? maHang = row.MaHang?.ToString();
                    string? maKho = row.MaKho?.ToString();
                    
                    if (string.IsNullOrEmpty(maHang) || string.IsNullOrEmpty(maKho)) continue;

                    var hangHoa = await _context.HangHoas.FindAsync(maHang);
                    var kho = await _context.Khos.FindAsync(maKho);

                    if (hangHoa == null)
                    {
                        errors.Add($"Dòng {rowNum}: Lỗi - Mã hàng {maHang} không tồn tại.");
                        continue;
                    }
                    if (kho == null)
                    {
                        errors.Add($"Dòng {rowNum}: Lỗi - Mã kho {maKho} không tồn tại.");
                        continue;
                    }

                    double soLuong = Convert.ToDouble(row.SoLuong ?? 0);
                    decimal giaVon = Convert.ToDecimal(row.GiaVon ?? 0);

                    if (soLuong < 0 || giaVon < 0)
                    {
                        errors.Add($"Dòng {rowNum}: Lỗi - Số lượng hoặc giá vốn báo âm.");
                        continue;
                    }

                    var existing = drafts.FirstOrDefault(d => d.MaKho == maKho && d.MaHang == maHang);
                    if (existing != null)
                    {
                        existing.SoLuong += soLuong;
                        existing.GiaVon = giaVon;
                    }
                    else
                    {
                        drafts.Add(new OpeningStockDraftViewModel
                        {
                            MaKho = maKho,
                            MaHang = maHang,
                            TenHang = hangHoa.TenHang,
                            SoLuong = soLuong,
                            GiaVon = giaVon
                        });
                    }
                    successCount++;
                }
            }

            SaveDrafts(drafts);
            return Ok(new { success = true, successCount, errors });
        }

        [HttpPost]
        public async Task<IActionResult> FinalizeOpeningStock()
        {
            if (IsFinalized()) return BadRequest("Tồn kho đã được chốt.");

            var drafts = GetDrafts();
            if (!drafts.Any()) return BadRequest("Chưa có dữ liệu tồn kho để chốt.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Ensure SYS_OPENING_STOCK Supplier exists
                var nccSys = await _context.NhaCungCaps.FindAsync("SYS_OPENING_STOCK");
                if (nccSys == null)
                {
                    nccSys = new NhaCungCap
                    {
                        MaDoiTuong = "SYS_OPENING_STOCK",
                        TenDoiTuong = "Nhà Cung Cấp Tồn Kho Hệ Thống",
                        SoNgayDuocNo = 0
                    };
                    _context.NhaCungCaps.Add(nccSys);
                    await _context.SaveChangesAsync();
                }

                // 2. Ensure KhoTongAo exists
                string maKhoTongAo = _config.GetValue<string>("KhoTongAo_MaKho") ?? "KHO_TONG";
                var khoTongAo = await _context.Khos.FindAsync(maKhoTongAo);
                if (khoTongAo == null)
                {
                    khoTongAo = new Kho
                    {
                        MaKho = maKhoTongAo,
                        TenKho = "Kho Tổng (Ảo)",
                        LoaiKho = "Ao"
                    };
                    _context.Khos.Add(khoTongAo);
                    await _context.SaveChangesAsync();
                }

                var groupedDrafts = drafts.GroupBy(d => d.MaKho).ToList();

                foreach (var group in groupedDrafts)
                {
                    string maKho = group.Key;
                    
                    var newPhieuNhap = new PhieuNhap
                    {
                        MaPhieu = "PN_TKBD_" + maKho + "_" + DateTime.Now.ToString("ddMMyyHHmmss"),
                        NgayNhap = DateTime.Now,
                        IdNhaCungCap = "SYS_OPENING_STOCK",
                        // IdDaiLyNhap maps to DaiLy, we can optionally map this if Kho has a MaDaiLyPhuTrach
                        TrangThaiThanhToan = "Đã Thanh Toán", // Opening stock so shouldn't affect debt
                        SoPhaiThanhToan = group.Sum(x => (decimal)x.SoLuong * x.GiaVon)
                    };

                    // Try to map DaiLyNhap if possible (fallback)
                    var khoEntity = await _context.Khos.FindAsync(maKho);
                    if (khoEntity != null && !string.IsNullOrEmpty(khoEntity.MaDaiLyPhuTrach))
                    {
                        newPhieuNhap.IdDaiLyNhap = khoEntity.MaDaiLyPhuTrach;
                    }

                    _context.PhieuNhaps.Add(newPhieuNhap);
                    await _context.SaveChangesAsync(); // save to get context

                    foreach (var item in group)
                    {
                        // Insert ChiTietPhieuNhap
                        var ctPhieuNhap = new ChiTietPhieuNhap
                        {
                            MaPhieu = newPhieuNhap.MaPhieu,
                            MaHang = item.MaHang,
                            SoLuong = item.SoLuong,
                            DonGiaNhap = item.GiaVon
                        };
                        _context.ChiTietPhieuNhaps.Add(ctPhieuNhap);

                        // Update Kho Vat Ly ChiTietTon
                        var ctTonVatLy = await _context.ChiTietTons.FirstOrDefaultAsync(c => c.MaKho == maKho && c.MaHang == item.MaHang);
                        if (ctTonVatLy == null)
                        {
                            ctTonVatLy = new ChiTietTon { MaKho = maKho, MaHang = item.MaHang, SoLuongTon = item.SoLuong, GiaTriTon = (decimal)item.SoLuong * item.GiaVon };
                            _context.ChiTietTons.Add(ctTonVatLy);
                        }
                        else
                        {
                            ctTonVatLy.SoLuongTon += item.SoLuong;
                            ctTonVatLy.GiaTriTon += (decimal)item.SoLuong * item.GiaVon;
                        }

                        // Update Kho Tong Ao ChiTietTon
                        var ctTonKTA = await _context.ChiTietTons.FirstOrDefaultAsync(c => c.MaKho == maKhoTongAo && c.MaHang == item.MaHang);
                        if (ctTonKTA == null)
                        {
                            ctTonKTA = new ChiTietTon { MaKho = maKhoTongAo, MaHang = item.MaHang, SoLuongTon = item.SoLuong, GiaTriTon = (decimal)item.SoLuong * item.GiaVon };
                            _context.ChiTietTons.Add(ctTonKTA);
                        }
                        else
                        {
                            ctTonKTA.SoLuongTon += item.SoLuong;
                            ctTonKTA.GiaTriTon += (decimal)item.SoLuong * item.GiaVon;
                        }

                        // Update HangHoa.GiaVonHienTai
                        var hh = await _context.HangHoas.FindAsync(item.MaHang);
                        if (hh != null)
                        {
                            hh.GiaVonHienTai = item.GiaVon;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Lock the module
                SetAppSettingFlag();
                
                // Clear drafts
                if (System.IO.File.Exists(DraftFilePath))
                {
                    System.IO.File.Delete(DraftFilePath);
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Có lỗi xảy ra khi chốt tồn kho: {ex.Message}");
            }
        }

        private void SetAppSettingFlag()
        {
            try
            {
                var filePath = Path.Combine(_env.ContentRootPath, "appsettings.json");
                var json = System.IO.File.ReadAllText(filePath);
                if (JsonNode.Parse(json) is JsonObject jObject)
                {
                    jObject["IsOpeningStockFinalized"] = true;
                    // Format output
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    System.IO.File.WriteAllText(filePath, jObject.ToJsonString(options));
                }
            }
            catch
            {
                // Fallback / ignore logic if file is locked or structured differently
            }
        }
    }
}

