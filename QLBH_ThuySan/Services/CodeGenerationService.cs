using Microsoft.EntityFrameworkCore;
using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Services
{
    public interface ICodeGenerationService
    {
        Task<string> GenerateProductCodeAsync();
        Task<string> GenerateCustomerCodeAsync();
        Task<string> GenerateSupplierCodeAsync();
        Task<string> GenerateWarehouseCodeAsync();
        Task<string> GenerateAgencyCodeAsync();
        Task<string> GenerateImportCodeAsync();
        Task<string> GenerateExportCodeAsync();
        Task<string> GenerateFinancialCodeAsync(string type);
        Task<string> GenerateDiscountCodeAsync();
        Task<string> GenerateCostObjectCodeAsync();
    }

    public class CodeGenerationService(ApplicationDbContext context) : ICodeGenerationService
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<string> GenerateProductCodeAsync() => await GenerateCodeAsync("HH", 3, _context.HangHoas.Select(h => h.MaHang));

        public async Task<string> GenerateCustomerCodeAsync() => await GenerateCodeAsync("KH", 3, _context.KhachHangs.Select(h => h.MaDoiTuong));

        public async Task<string> GenerateSupplierCodeAsync() => await GenerateCodeAsync("NCC", 3, _context.NhaCungCaps.Select(h => h.MaDoiTuong));

        public async Task<string> GenerateWarehouseCodeAsync() => await GenerateCodeAsync("K", 2, _context.Khos.Select(h => h.MaKho));

        public async Task<string> GenerateAgencyCodeAsync() => await GenerateCodeAsync("DL", 2, _context.DaiLys.Select(h => h.MaDaiLy));

        public async Task<string> GenerateImportCodeAsync() => await GenerateDateCodeAsync("PN", _context.PhieuNhaps.Select(h => h.MaPhieu));

        public async Task<string> GenerateExportCodeAsync() => await GenerateDateCodeAsync("PX", _context.PhieuXuats.Select(h => h.MaPhieu));

        public async Task<string> GenerateFinancialCodeAsync(string type)
        {
            string prefix = type?.ToUpper() == "THU" ? "PT" : "PC";
            return await GenerateDateCodeAsync(prefix, _context.PhieuThuChis.Select(h => h.MaPhieu));
        }

        public async Task<string> GenerateDiscountCodeAsync() => await GenerateDateCodeAsync("CK", _context.PhieuTinhChietKhaus.Select(h => h.MaPhieuTinh));

        public async Task<string> GenerateCostObjectCodeAsync() => await GenerateCodeAsync("CP", 3, _context.DoiTuongChiPhis.Select(h => h.MaDoiTuong));

        private async Task<string> GenerateCodeAsync(string prefix, int numericLength, IQueryable<string> query)
        {
            // Fetch all codes with prefix, but filter by reasonable length to ignore legacy timestamp IDs
            // Standard expected: prefix (2-3) + numeric (2-4) = 4-7 chars.
            // Old was KH + 12 chars = 14 chars.
            var codes = await query
                .Where(c => c.StartsWith(prefix) && c.Length < 10)
                .OrderByDescending(c => c)
                .Take(100)
                .ToListAsync();

            long nextNumber = 1;
            if (codes.Any())
            {
                var maxNumber = codes
                    .Select(c => c[prefix.Length..])
                    .Where(s => long.TryParse(s, out _))
                    .Select(long.Parse)
                    .DefaultIfEmpty(0)
                    .Max();
                
                nextNumber = maxNumber + 1;
            }

            return $"{prefix}{nextNumber.ToString().PadLeft(numericLength, '0')}";
        }

        private async Task<string> GenerateDateCodeAsync(string prefix, IQueryable<string> query)
        {
            string datePart = DateTime.Now.ToString("yyMMdd");
            string fullPrefix = $"{prefix}{datePart}";

            // For date-based prefixes, they are more predictable.
            var codes = await query
                .Where(c => c.StartsWith(fullPrefix))
                .OrderByDescending(c => c)
                .Take(50)
                .ToListAsync();

            int nextNumber = 1;
            if (codes.Any())
            {
                var maxNumber = codes
                    .Select(c => c[fullPrefix.Length..])
                    .Where(s => int.TryParse(s, out _))
                    .Select(int.Parse)
                    .DefaultIfEmpty(0)
                    .Max();
                
                nextNumber = maxNumber + 1;
            }

            return $"{fullPrefix}{nextNumber.ToString().PadLeft(3, '0')}";
        }
    }
}
