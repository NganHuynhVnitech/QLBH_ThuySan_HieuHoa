-- Stored Procedure để tính giá vốn bình quân gia quyền liên hoàn
-- Được gọi sau mỗi lần nhập hàng để cập nhật giá vốn

CREATE OR ALTER PROCEDURE [dbo].[sp_CalculateWeightedAverageCost]
    @ProductId INT,
    @WarehouseId INT,
    @NewQuantity DECIMAL(18,2),
    @NewUnitPrice DECIMAL(18,2),
    @NewWeightedAverageCost DECIMAL(18,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CurrentQuantity DECIMAL(18,2);
    DECLARE @CurrentWeightedAverageCost DECIMAL(18,2);
    DECLARE @TotalValue DECIMAL(18,2);
    DECLARE @TotalQuantity DECIMAL(18,2);
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Lấy thông tin tồn kho hiện tại
        SELECT 
            @CurrentQuantity = ISNULL(Quantity, 0),
            @CurrentWeightedAverageCost = ISNULL(WeightedAverageCost, 0)
        FROM ProductWarehouses WITH (UPDLOCK)
        WHERE ProductId = @ProductId AND WarehouseId = @WarehouseId;
        
        -- Nếu chưa có record, tạo mới
        IF @CurrentQuantity IS NULL
        BEGIN
            INSERT INTO ProductWarehouses (ProductId, WarehouseId, Quantity, WeightedAverageCost, LastUpdated)
            VALUES (@ProductId, @WarehouseId, @NewQuantity, @NewUnitPrice, GETDATE());
            
            SET @NewWeightedAverageCost = @NewUnitPrice;
        END
        ELSE
        BEGIN
            -- Tính giá vốn bình quân gia quyền mới
            -- Công thức: WAC = (CurrentValue + NewValue) / (CurrentQty + NewQty)
            SET @TotalValue = (@CurrentQuantity * @CurrentWeightedAverageCost) + (@NewQuantity * @NewUnitPrice);
            SET @TotalQuantity = @CurrentQuantity + @NewQuantity;
            
            IF @TotalQuantity > 0
                SET @NewWeightedAverageCost = @TotalValue / @TotalQuantity;
            ELSE
                SET @NewWeightedAverageCost = 0;
            
            -- Cập nhật tồn kho
            UPDATE ProductWarehouses
            SET 
                Quantity = @TotalQuantity,
                WeightedAverageCost = @NewWeightedAverageCost,
                LastUpdated = GETDATE()
            WHERE ProductId = @ProductId AND WarehouseId = @WarehouseId;
        END
        
        -- Đồng bộ vào Kho Tổng Ảo (WarehouseId = 4)
        IF @WarehouseId != 4
        BEGIN
            DECLARE @VirtualWarehouseId INT = 4;
            DECLARE @VirtualCurrentQuantity DECIMAL(18,2);
            DECLARE @VirtualCurrentWAC DECIMAL(18,2);
            DECLARE @VirtualTotalValue DECIMAL(18,2);
            DECLARE @VirtualTotalQuantity DECIMAL(18,2);
            DECLARE @VirtualNewWAC DECIMAL(18,2);
            
            SELECT 
                @VirtualCurrentQuantity = ISNULL(Quantity, 0),
                @VirtualCurrentWAC = ISNULL(WeightedAverageCost, 0)
            FROM ProductWarehouses WITH (UPDLOCK)
            WHERE ProductId = @ProductId AND WarehouseId = @VirtualWarehouseId;
            
            IF @VirtualCurrentQuantity IS NULL
            BEGIN
                INSERT INTO ProductWarehouses (ProductId, WarehouseId, Quantity, WeightedAverageCost, LastUpdated)
                VALUES (@ProductId, @VirtualWarehouseId, @NewQuantity, @NewUnitPrice, GETDATE());
            END
            ELSE
            BEGIN
                SET @VirtualTotalValue = (@VirtualCurrentQuantity * @VirtualCurrentWAC) + (@NewQuantity * @NewUnitPrice);
                SET @VirtualTotalQuantity = @VirtualCurrentQuantity + @NewQuantity;
                
                IF @VirtualTotalQuantity > 0
                    SET @VirtualNewWAC = @VirtualTotalValue / @VirtualTotalQuantity;
                ELSE
                    SET @VirtualNewWAC = 0;
                
                UPDATE ProductWarehouses
                SET 
                    Quantity = @VirtualTotalQuantity,
                    WeightedAverageCost = @VirtualNewWAC,
                    LastUpdated = GETDATE()
                WHERE ProductId = @ProductId AND WarehouseId = @VirtualWarehouseId;
            END
        END
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO
