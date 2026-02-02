using QLBH_ThuySan.Models;

namespace QLBH_ThuySan.Services
{
    public interface IDiscountService
    {
        decimal CalculateSupplierDiscount(int supplierId, decimal orderAmount);
        decimal CalculateCustomerDiscount(int customerId, decimal orderAmount);
    }

    public class DiscountService : IDiscountService
    {
        private readonly ApplicationDbContext _context;

        public DiscountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public decimal CalculateSupplierDiscount(int supplierId, decimal orderAmount)
        {
            var discounts = _context.SupplierDiscounts
                .Where(d => d.SupplierId == supplierId 
                    && d.IsActive 
                    && d.StartDate <= DateTime.Now
                    && (d.EndDate == null || d.EndDate >= DateTime.Now)
                    && d.MinimumAmount <= orderAmount)
                .OrderByDescending(d => d.DiscountValue)
                .ToList();

            decimal totalDiscount = 0;

            foreach (var discount in discounts)
            {
                switch (discount.DiscountType)
                {
                    case DiscountType.Fixed:
                        totalDiscount += discount.DiscountValue;
                        break;

                    case DiscountType.Percentage:
                        totalDiscount += orderAmount * (discount.DiscountValue / 100);
                        break;

                    case DiscountType.Tiered:
                        var tiers = _context.DiscountTiers
                            .Where(t => t.SupplierDiscountId == discount.SupplierDiscountId
                                && t.FromAmount <= orderAmount
                                && t.ToAmount >= orderAmount)
                            .ToList();

                        foreach (var tier in tiers)
                        {
                            totalDiscount += orderAmount * (tier.DiscountValue / 100);
                        }
                        break;
                }
            }

            return totalDiscount;
        }

        public decimal CalculateCustomerDiscount(int customerId, decimal orderAmount)
        {
            var discounts = _context.CustomerDiscounts
                .Where(d => d.CustomerId == customerId 
                    && d.IsActive 
                    && d.StartDate <= DateTime.Now
                    && (d.EndDate == null || d.EndDate >= DateTime.Now)
                    && d.MinimumAmount <= orderAmount)
                .OrderByDescending(d => d.DiscountValue)
                .ToList();

            decimal totalDiscount = 0;

            foreach (var discount in discounts)
            {
                switch (discount.DiscountType)
                {
                    case DiscountType.Fixed:
                        totalDiscount += discount.DiscountValue;
                        break;

                    case DiscountType.Percentage:
                        totalDiscount += orderAmount * (discount.DiscountValue / 100);
                        break;

                    case DiscountType.Tiered:
                        var tiers = _context.DiscountTiers
                            .Where(t => t.CustomerDiscountId == discount.CustomerDiscountId
                                && t.FromAmount <= orderAmount
                                && t.ToAmount >= orderAmount)
                            .ToList();

                        foreach (var tier in tiers)
                        {
                            totalDiscount += orderAmount * (tier.DiscountValue / 100);
                        }
                        break;
                }
            }

            return totalDiscount;
        }
    }
}
