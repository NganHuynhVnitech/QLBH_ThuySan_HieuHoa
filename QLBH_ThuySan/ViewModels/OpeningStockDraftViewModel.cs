using System;
using System.ComponentModel.DataAnnotations;

namespace QLBH_ThuySan.ViewModels
{
    public class OpeningStockDraftViewModel
    {
        [Required]
        [Display(Name = "Mã Kho Vật Lý")]
        public string MaKho { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Mã Hàng Hóa")]
        public string MaHang { get; set; } = string.Empty;
        
        [Display(Name = "Tên Hàng Hóa")]
        public string TenHang { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Số Lượng Ban Đầu")]
        [Range(0, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        public double SoLuong { get; set; }

        [Required]
        [Display(Name = "Giá Vốn Khởi Điểm")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá vốn phải lớn hơn hoặc bằng 0")]
        public decimal GiaVon { get; set; }
    }
}
