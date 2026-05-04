using Microsoft.AspNetCore.Mvc;
using QLBH_ThuySan.Services;
using QLBH_ThuySan.ViewModels;

namespace QLBH_ThuySan.Controllers.Api
{
    [Route("api/inventory/outbound")]
    [ApiController]
    public class OutboundApiController : ControllerBase
    {
        private readonly IOutboundService _outboundService;

        public OutboundApiController(IOutboundService outboundService)
        {
            _outboundService = outboundService;
        }

        [HttpPost("sales")]
        public async Task<IActionResult> CreateSales([FromBody] OutboundSalesDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                await _outboundService.CreateSalesOutboundAsync(dto);
                return Ok(new { message = "Xuất bán hàng thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("return-vendor")]
        public async Task<IActionResult> CreateReturnVendor([FromBody] OutboundReturnVendorDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                await _outboundService.CreateReturnVendorOutboundAsync(dto);
                return Ok(new { message = "Xuất trả nhà cung cấp thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("damage")]
        public async Task<IActionResult> CreateDamage([FromBody] OutboundDamageDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                await _outboundService.CreateDamageOutboundAsync(dto);
                return Ok(new { message = "Xuất hủy/hao hụt thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> CreateTransfer([FromBody] OutboundTransferDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                await _outboundService.CreateTransferOutboundAsync(dto);
                return Ok(new { message = "Xuất điều chuyển kho thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
