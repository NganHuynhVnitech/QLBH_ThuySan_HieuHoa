using QLBH_ThuySan.Models;
using Xunit;

namespace QLBH_ThuySan.Tests.Business;

public class BusinessLogicTests
{
    [Fact]
    public void Should_Calculate_Moving_Weighted_Average_COGS()
    {
        // Scenario: Initial 100 kg @ 10,000. New Import 50 kg @ 13,000. 
        // New Avg = (100 * 10,000 + 50 * 13,000) / 150 = 1,650,000 / 150 = 11,000
        
        double currentQty = 100;
        decimal currentAvg = 10000;
        
        double importQty = 50;
        decimal importPrice = 13000;
        
        decimal totalValue = (decimal)currentQty * currentAvg + (decimal)importQty * importPrice;
        decimal newAvg = totalValue / (decimal)(currentQty + importQty);
        
        Assert.Equal(11000, newAvg);
    }

    [Fact]
    public void Should_Calculate_Tiered_Discount()
    {
        // Scenario: 0-100kg = 500đ, 101-200kg = 600đ. Input 150kg.
        // Total = 100 * 500 + 50 * 600 = 50,000 + 30,000 = 80,000
        
        double qty = 150;
        decimal discount = 0;
        
        if (qty <= 100)
        {
            discount = (decimal)qty * 500;
        }
        else
        {
            discount = (100 * 500) + (decimal)(qty - 100) * 600;
        }
        
        Assert.Equal(80000, discount);
    }
}
