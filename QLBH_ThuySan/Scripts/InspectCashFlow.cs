using QLBH_ThuySan.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace QLBH_ThuySan.Scripts
{
    public class InspectCashFlow
    {
        public static async Task Run(ApplicationDbContext context)
        {
            var vouchers = await context.PhieuThuChis.ToListAsync();
            Console.WriteLine($"Total vouchers: {vouchers.Count}");
            
            var groups = vouchers.GroupBy(v => v.LoaiPhieu)
                                 .Select(g => new { Type = g.Key, Count = g.Count() });
            
            foreach (var g in groups)
            {
                Console.WriteLine($"Type: {g.Type}, Count: {g.Count}");
                var samples = vouchers.Where(v => v.LoaiPhieu == g.Type).Take(5);
                foreach (var s in samples)
                {
                    Console.WriteLine($"  - [{s.MaPhieu}] {s.LyDo} ({s.MaDoiTuong})");
                }
            }
        }
    }
}
