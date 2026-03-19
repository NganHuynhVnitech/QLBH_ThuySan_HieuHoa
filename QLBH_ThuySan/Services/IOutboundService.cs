using QLBH_ThuySan.ViewModels;
using System.Threading.Tasks;

namespace QLBH_ThuySan.Services
{
    public interface IOutboundService
    {
        Task<bool> CreateSalesOutboundAsync(OutboundSalesDto dto);
        Task<bool> CreateReturnVendorOutboundAsync(OutboundReturnVendorDto dto);
        Task<bool> CreateDamageOutboundAsync(OutboundDamageDto dto);
        Task<bool> CreateTransferOutboundAsync(OutboundTransferDto dto);
    }
}
