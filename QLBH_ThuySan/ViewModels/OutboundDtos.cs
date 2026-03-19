using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QLBH_ThuySan.ViewModels
{
    public class OutboundItemDto
    {
        [Required]
        public string MaHang { get; set; } = null!;
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public double SoLuong { get; set; }

        public decimal GiaBan { get; set; } // Used for SALES
    }

    public class OutboundBaseDto
    {
        [Required]
        public string SourceWarehouseCode { get; set; } = null!; // Xuất từ kho nào
        
        [Required]
        public List<OutboundItemDto> Items { get; set; } = new List<OutboundItemDto>();
    }

    public class OutboundSalesDto : OutboundBaseDto
    {
        public string? CustomerId { get; set; } // Can be null for retail walk-in
        public string PaymentStatus { get; set; } = "Đã Thanh Toán";
    }

    public class OutboundReturnVendorDto : OutboundBaseDto
    {
        [Required]
        public string VendorId { get; set; } = null!;
    }

    public class OutboundDamageDto : OutboundBaseDto
    {
        [Required]
        public string Reason { get; set; } = null!;
    }

    public class OutboundTransferDto : OutboundBaseDto
    {
        [Required]
        public string DestinationWarehouseCode { get; set; } = null!;
    }
}
