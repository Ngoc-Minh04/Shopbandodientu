# Design Document: Discount Code Management

## Overview

Hệ thống quản lý mã giảm giá (Discount Code Management) cung cấp đầy đủ chức năng cho Admin quản lý các chương trình khuyến mãi và cho Customer áp dụng mã giảm giá khi đặt hàng. Thiết kế này tập trung vào tính nhất quán dữ liệu, xử lý race condition, và tích hợp mượt mà với luồng đặt hàng hiện tại.

### Key Design Goals

- Đảm bảo tính toàn vẹn dữ liệu khi nhiều người dùng đồng thời sử dụng mã giảm giá
- Tích hợp liền mạch với OrderServices hiện tại
- Tuân thủ các patterns và conventions của dự án
- Xử lý đúng các trường hợp edge case (hết hạn, hết số lượng, điều kiện không đủ)
- Hỗ trợ rollback khi hủy đơn hàng

## Architecture

### Layer Architecture

```
┌─────────────────────────────────────────┐
│         Controllers Layer               │
│  - DiscountController (Admin)           │
│  - OrderController (Customer - updated) │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│         Services Layer                  │
│  - IDiscountServices                    │
│  - DiscountServices                     │
│  - OrderServices (updated)              │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│         Data Layer                      │
│  - Magiamgium (Entity)                  │
│  - Lichsusudungmagiamgium (Entity)      │
│  - Donhang (Entity)                     │
│  - MinhContext (DbContext)              │
└─────────────────────────────────────────┘
```

### Component Interaction Flow

**Admin tạo mã giảm giá:**
```
Admin → DiscountController.Create() 
      → DiscountServices.CreateAsync() 
      → Validate & Insert Magiamgium 
      → Return response
```

**Customer áp dụng mã giảm giá:**
```
Customer → OrderController.CreateOrder() 
         → OrderServices.CreateOrderAsync() 
         → DiscountServices.ValidateAndApplyAsync() 
         → [Transaction Start]
         → Lock Magiamgium record
         → Check availability
         → Update Soluongdasudung
         → Create Lichsusudungmagiamgium
         → Calculate final price
         → Create Donhang & Chitietdonhang
         → [Transaction Commit]
         → Return order with discount info
```

## Components and Interfaces

### 1. DiscountController

**Responsibility:** Xử lý HTTP requests cho quản lý mã giảm giá (Admin only)

**Endpoints:**

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DiscountController : ControllerBase
{
    [HttpPost("create")]
    Task<dynamic> Create([FromBody] CreateDiscountRequest request);
    
    [HttpPost("update")]
    Task<dynamic> Update([FromBody] UpdateDiscountRequest request);
    
    [HttpPost("delete")]
    Task<dynamic> Delete([FromBody] int id);
    
    [HttpPost("list")]
    Task<dynamic> GetList([FromBody] DiscountListRequest request);
    
    [HttpGet("{id}")]
    Task<dynamic> GetDetail(int id);
    
    [HttpPost("toggle-status")]
    Task<dynamic> ToggleStatus([FromBody] ToggleDiscountStatusRequest request);
    
    [HttpGet("{id}/usage-history")]
    Task<dynamic> GetUsageHistory(int id, [FromQuery] UsageHistoryRequest request);
}
```

**Customer Endpoints (không cần Admin role):**

```csharp
[HttpPost("validate")]
[Authorize(Roles = "Customer")]
Task<dynamic> ValidateDiscount([FromBody] ValidateDiscountRequest request);
```

### 2. DiscountServices

**Responsibility:** Xử lý toàn bộ business logic liên quan đến mã giảm giá

**Interface:**

```csharp
public interface IDiscountServices
{
    // Admin operations
    Task<dynamic> CreateAsync(CreateDiscountRequest request);
    Task<dynamic> UpdateAsync(UpdateDiscountRequest request);
    Task<dynamic> DeleteAsync(int id);
    Task<dynamic> GetListAsync(DiscountListRequest request);
    Task<dynamic> GetDetailAsync(int id);
    Task<dynamic> ToggleStatusAsync(int id, bool status);
    Task<dynamic> GetUsageHistoryAsync(int discountId, UsageHistoryRequest request);
    
    // Customer operations
    Task<dynamic> ValidateDiscountAsync(string code, decimal orderAmount);
    
    // Internal operations (called by OrderServices)
    Task<dynamic> ValidateAndApplyAsync(string code, decimal orderAmount, int userId, int orderId);
    Task<dynamic> RollbackDiscountAsync(int orderId);
}
```

**Key Methods:**

- `CreateAsync`: Tạo mã giảm giá mới với đầy đủ validation
- `UpdateAsync`: Cập nhật mã giảm giá (không cho đổi code và loại giảm giá)
- `DeleteAsync`: Soft delete (set Trangthai = false)
- `GetListAsync`: Lấy danh sách với filter, search, pagination
- `GetDetailAsync`: Lấy chi tiết kèm thống kê sử dụng
- `ValidateDiscountAsync`: Kiểm tra tính hợp lệ và tính giá trị giảm (cho Customer xem trước)
- `ValidateAndApplyAsync`: Validate + Apply trong transaction (gọi từ OrderServices)
- `RollbackDiscountAsync`: Hoàn lại số lượng khi hủy đơn

### 3. OrderServices (Updated)

**Changes:** Tích hợp logic áp dụng mã giảm giá vào luồng đặt hàng

**Updated Method:**

```csharp
public async Task<dynamic> CreateOrderAsync(int userId, CreateOrderRequest request)
{
    // request.DiscountCode (optional) - mã giảm giá
    
    await using var transaction = await _context.Database
        .BeginTransactionAsync(IsolationLevel.Serializable);
    try
    {
        // 1. Validate giỏ hàng (existing logic)
        // 2. Tính tổng tiền ban đầu (existing logic)
        
        // 3. Áp dụng mã giảm giá (NEW)
        decimal discountAmount = 0;
        int? discountId = null;
        if (!string.IsNullOrEmpty(request.DiscountCode))
        {
            var discountResult = await _discountServices
                .ValidateAndApplyAsync(
                    request.DiscountCode, 
                    tongTien, 
                    userId, 
                    0 // orderId chưa có, sẽ update sau
                );
            
            if (discountResult.code != 200)
            {
                await transaction.RollbackAsync();
                return discountResult; // Trả về lỗi từ discount validation
            }
            
            discountAmount = discountResult.data.discountAmount;
            discountId = discountResult.data.discountId;
        }
        
        // 4. Tính tổng tiền sau giảm
        decimal finalAmount = Math.Max(0, tongTien - discountAmount);
        
        // 5. Tạo đơn hàng (existing logic)
        var donhang = new Donhang { ... Tongtien = finalAmount };
        
        // 6. Update orderId vào Lichsusudungmagiamgium
        if (discountId.HasValue)
        {
            var usageRecord = await _context.Lichsusudungmagiamgia
                .FirstOrDefaultAsync(l => l.Magiamgiaid == discountId 
                                       && l.Taikhoanid == userId 
                                       && l.Donhangid == null);
            if (usageRecord != null)
            {
                usageRecord.Donhangid = donhang.Id;
            }
        }
        
        // 7. Tạo chi tiết đơn hàng, trừ tồn kho (existing logic)
        // 8. Xóa giỏ hàng (existing logic)
        
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return new { 
            code = 200, 
            data = new { 
                orderId = donhang.Id,
                tongTien = tongTien,
                discountAmount = discountAmount,
                finalAmount = finalAmount
            }
        };
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return new { code = 500, message = "Lỗi: " + ex.Message };
    }
}
```

**Updated CancelOrderAsync:**

```csharp
public async Task<dynamic> CancelOrderAsync(int userId, int orderId)
{
    await using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // Existing validation logic...
        
        // Hoàn lại tồn kho (existing logic)
        
        // Hoàn lại mã giảm giá (NEW)
        await _discountServices.RollbackDiscountAsync(orderId);
        
        // Update trạng thái đơn hàng (existing logic)
        
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return new { code = 200, message = "Hủy đơn hàng thành công" };
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return new { code = 500, message = "Lỗi: " + ex.Message };
    }
}
```

## Data Models

### DTOs (Request/Response Models)

#### CreateDiscountRequest

```csharp
public class CreateDiscountRequest
{
    public string Macode { get; set; } = null!;
    public string Tenchuongtrinh { get; set; } = null!;
    public string? Mota { get; set; }
    public string Loaigiamgia { get; set; } = null!; // "percent" or "fixed"
    public decimal Giatrigiam { get; set; }
    public decimal? Giamtoida { get; set; } // Required if Loaigiamgia = "percent"
    public decimal? Giatridonhangtoithieu { get; set; }
    public int Soluong { get; set; }
    public DateTime Ngaybatdau { get; set; }
    public DateTime Ngayketthuc { get; set; }
    
    public string? Validate()
    {
        if (string.IsNullOrWhiteSpace(Macode))
            return "Mã code không được để trống";
        if (string.IsNullOrWhiteSpace(Tenchuongtrinh))
            return "Tên chương trình không được để trống";
        if (Loaigiamgia != "percent" && Loaigiamgia != "fixed")
            return "Loại giảm giá phải là 'percent' hoặc 'fixed'";
        if (Ngayketthuc <= Ngaybatdau)
            return "Ngày kết thúc phải lớn hơn ngày bắt đầu";
        if (Soluong <= 0)
            return "Số lượng phải lớn hơn 0";
        if (Giatridonhangtoithieu.HasValue && Giatridonhangtoithieu < 0)
            return "Giá trị đơn hàng tối thiểu không được âm";
            
        if (Loaigiamgia == "percent")
        {
            if (Giatrigiam <= 0 || Giatrigiam > 100)
                return "Giá trị giảm phần trăm phải từ 0 đến 100";
            if (!Giamtoida.HasValue || Giamtoida <= 0)
                return "Giảm tối đa là bắt buộc khi loại giảm giá là phần trăm";
        }
        else // fixed
        {
            if (Giatrigiam <= 0)
                return "Giá trị giảm phải lớn hơn 0";
        }
        
        return null;
    }
}
```

#### UpdateDiscountRequest

```csharp
public class UpdateDiscountRequest
{
    public int Id { get; set; }
    public string Tenchuongtrinh { get; set; } = null!;
    public string? Mota { get; set; }
    public decimal Giatrigiam { get; set; }
    public decimal? Giamtoida { get; set; }
    public decimal? Giatridonhangtoithieu { get; set; }
    public int Soluong { get; set; }
    public DateTime Ngaybatdau { get; set; }
    public DateTime Ngayketthuc { get; set; }
    
    // Validation tương tự CreateDiscountRequest
    // Không cho phép thay đổi Macode và Loaigiamgia
}
```

#### DiscountListRequest

```csharp
public class DiscountListRequest
{
    public string? Keyword { get; set; } // Search by code or name
    public bool? Trangthai { get; set; } // Filter by status
    public string? TimeFilter { get; set; } // "active", "expired", "upcoming"
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    public string? Validate()
    {
        if (PageIndex < 1)
            return "PageIndex phải lớn hơn 0";
        if (PageSize < 1 || PageSize > 100)
            return "PageSize phải từ 1 đến 100";
        return null;
    }
}
```

#### ValidateDiscountRequest

```csharp
public class ValidateDiscountRequest
{
    public string Macode { get; set; } = null!;
    public decimal OrderAmount { get; set; }
    
    public string? Validate()
    {
        if (string.IsNullOrWhiteSpace(Macode))
            return "Mã code không được để trống";
        if (OrderAmount <= 0)
            return "Giá trị đơn hàng phải lớn hơn 0";
        return null;
    }
}
```

#### ValidateDiscountResponse

```csharp
public class ValidateDiscountResponse
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string Tenchuongtrinh { get; set; } = null!;
}
```

#### DiscountDetailResponse

```csharp
public class DiscountDetailResponse
{
    public int Id { get; set; }
    public string Macode { get; set; } = null!;
    public string Tenchuongtrinh { get; set; } = null!;
    public string? Mota { get; set; }
    public string Loaigiamgia { get; set; } = null!;
    public decimal Giatrigiam { get; set; }
    public decimal? Giamtoida { get; set; }
    public decimal? Giatridonhangtoithieu { get; set; }
    public int Soluong { get; set; }
    public int Soluongdasudung { get; set; }
    public int Soluongconlai { get; set; }
    public DateTime Ngaybatdau { get; set; }
    public DateTime Ngayketthuc { get; set; }
    public bool Trangthai { get; set; }
    public DateTime Ngaytao { get; set; }
    
    // Statistics
    public decimal TongGiatriDagiam { get; set; }
    public int TongLuotSudung { get; set; }
}
```

#### UsageHistoryRequest

```csharp
public class UsageHistoryRequest
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? TaikhoanId { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
```

#### CreateOrderRequest (Updated)

```csharp
public class CreateOrderRequest
{
    // Existing fields
    public string Tennguoinhan { get; set; } = null!;
    public string Diachigiaohang { get; set; } = null!;
    public string Sdtnguoinhan { get; set; } = null!;
    
    // NEW field
    public string? DiscountCode { get; set; }
    
    // Existing Validate() method...
}
```

### Database Entities (Existing - No Changes)

#### Magiamgium

```csharp
public partial class Magiamgium
{
    public int Id { get; set; }
    public string Macode { get; set; } = null!;
    public string Tenchuongtrinh { get; set; } = null!;
    public string? Mota { get; set; }
    public string Loaigiamgia { get; set; } = null!;
    public decimal Giatrigiam { get; set; }
    public decimal? Giamtoida { get; set; }
    public decimal? Giatridonhangtoithieu { get; set; }
    public int Soluong { get; set; }
    public int? Soluongdasudung { get; set; }
    public DateTime Ngaybatdau { get; set; }
    public DateTime Ngayketthuc { get; set; }
    public bool? Trangthai { get; set; }
    public DateTime? Ngaytao { get; set; }
    
    public virtual ICollection<Lichsusudungmagiamgium> Lichsusudungmagiamgia { get; set; }
}
```

#### Lichsusudungmagiamgium

```csharp
public partial class Lichsusudungmagiamgium
{
    public int Id { get; set; }
    public int? Magiamgiaid { get; set; }
    public int? Taikhoanid { get; set; }
    public int? Donhangid { get; set; }
    public decimal Giatrigiamthucte { get; set; }
    public DateTime? Ngaysudung { get; set; }
    
    public virtual Donhang? Donhang { get; set; }
    public virtual Magiamgium? Magiamgia { get; set; }
    public virtual Taikhoan? Taikhoan { get; set; }
}
```


## Database Transaction Handling

### Transaction Strategy

Hệ thống sử dụng database transactions với isolation level phù hợp để đảm bảo tính nhất quán dữ liệu:

#### 1. Serializable Isolation Level

Sử dụng cho các operations có race condition risk:

```csharp
await using var transaction = await _context.Database
    .BeginTransactionAsync(IsolationLevel.Serializable);
```

**Áp dụng cho:**
- `ValidateAndApplyAsync`: Khi áp dụng mã giảm giá (đọc + cập nhật số lượng)
- `CreateOrderAsync`: Khi tạo đơn hàng có mã giảm giá
- `CancelOrderAsync`: Khi hủy đơn hàng và hoàn lại mã

**Lý do:** Serializable isolation level ngăn chặn:
- Dirty reads: Đọc dữ liệu chưa commit
- Non-repeatable reads: Dữ liệu thay đổi giữa 2 lần đọc
- Phantom reads: Xuất hiện records mới giữa 2 lần query
- Race conditions: Nhiều transactions cùng cập nhật số lượng

#### 2. Read Committed Isolation Level (Default)

Sử dụng cho các operations chỉ đọc hoặc không có race condition:

```csharp
await using var transaction = await _context.Database
    .BeginTransactionAsync(); // Default: Read Committed
```

**Áp dụng cho:**
- `CreateAsync`: Tạo mã giảm giá mới (chỉ insert)
- `UpdateAsync`: Cập nhật thông tin (không ảnh hưởng số lượng đang sử dụng)
- `DeleteAsync`: Soft delete (chỉ update status)

### Race Condition Handling

#### Scenario: Nhiều users cùng áp dụng mã giảm giá

**Problem:**
```
Time    User A                          User B
T1      Read: Soluongconlai = 1         
T2                                      Read: Soluongconlai = 1
T3      Check: OK (1 > 0)               
T4                                      Check: OK (1 > 0)
T5      Update: Soluongdasudung = 1     
T6                                      Update: Soluongdasudung = 2
Result: 2 users sử dụng mã chỉ có 1 slot!
```

**Solution: Serializable Transaction + Row Locking**

```csharp
public async Task<dynamic> ValidateAndApplyAsync(
    string code, decimal orderAmount, int userId, int orderId)
{
    await using var transaction = await _context.Database
        .BeginTransactionAsync(IsolationLevel.Serializable);
    try
    {
        // 1. Lock row khi đọc (FOR UPDATE trong SQL)
        var discount = await _context.Magiamgia
            .Where(m => m.Macode == code)
            .FirstOrDefaultAsync();
        
        if (discount == null)
        {
            await transaction.RollbackAsync();
            return new { code = 404, message = "Mã giảm giá không tồn tại" };
        }
        
        // 2. Validate sau khi lock
        int soluongConlai = discount.Soluong - (discount.Soluongdasudung ?? 0);
        if (soluongConlai <= 0)
        {
            await transaction.RollbackAsync();
            return new { code = 400, message = "Mã giảm giá đã hết" };
        }
        
        // 3. Validate các điều kiện khác
        if (discount.Trangthai != true)
            return new { code = 400, message = "Mã giảm giá không hoạt động" };
        
        DateTime now = DateTime.Now;
        if (now < discount.Ngaybatdau || now > discount.Ngayketthuc)
            return new { code = 400, message = "Mã giảm giá không trong thời gian hiệu lực" };
        
        if (discount.Giatridonhangtoithieu.HasValue 
            && orderAmount < discount.Giatridonhangtoithieu.Value)
            return new { code = 400, message = $"Đơn hàng tối thiểu {discount.Giatridonhangtoithieu}" };
        
        // 4. Tính giá trị giảm
        decimal discountAmount = CalculateDiscountAmount(discount, orderAmount);
        
        // 5. Cập nhật số lượng đã sử dụng
        discount.Soluongdasudung = (discount.Soluongdasudung ?? 0) + 1;
        
        // 6. Tạo bản ghi lịch sử
        var usageHistory = new Lichsusudungmagiamgium
        {
            Magiamgiaid = discount.Id,
            Taikhoanid = userId,
            Donhangid = orderId == 0 ? null : orderId,
            Giatrigiamthucte = discountAmount,
            Ngaysudung = DateTime.Now
        };
        _context.Lichsusudungmagiamgia.Add(usageHistory);
        
        // 7. Save changes
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return new
        {
            code = 200,
            message = "Áp dụng mã giảm giá thành công",
            data = new
            {
                discountId = discount.Id,
                discountAmount = discountAmount,
                tenchuongtrinh = discount.Tenchuongtrinh
            }
        };
    }
    catch (DbUpdateException ex)
    {
        await transaction.RollbackAsync();
        return new { code = 500, message = "Lỗi cập nhật database: " + ex.Message };
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return new { code = 500, message = "Lỗi: " + ex.Message };
    }
}
```

**Key Points:**
- Serializable isolation level đảm bảo không có transaction nào khác có thể đọc/ghi vào cùng row
- FirstOrDefaultAsync() trong Serializable transaction tự động lock row
- Kiểm tra lại số lượng sau khi lock để đảm bảo chính xác
- Rollback ngay khi phát hiện lỗi

#### Timeout và Retry Logic

```csharp
// Trong Program.cs hoặc DbContext configuration
builder.Services.AddDbContext<MinhContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(30); // 30 seconds timeout
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null
        );
    });
});
```

### Transaction Rollback Scenarios

#### 1. Rollback khi validation thất bại

```csharp
if (validationError != null)
{
    await transaction.RollbackAsync();
    return new { code = 400, message = validationError };
}
```

#### 2. Rollback khi exception xảy ra

```csharp
catch (Exception ex)
{
    await transaction.RollbackAsync();
    return new { code = 500, message = "Lỗi: " + ex.Message };
}
```

#### 3. Rollback khi hủy đơn hàng

```csharp
public async Task<dynamic> RollbackDiscountAsync(int orderId)
{
    await using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // Tìm usage history
        var usageHistory = await _context.Lichsusudungmagiamgia
            .Include(l => l.Magiamgia)
            .FirstOrDefaultAsync(l => l.Donhangid == orderId);
        
        if (usageHistory == null || usageHistory.Magiamgia == null)
        {
            // Không có mã giảm giá, không cần rollback
            await transaction.CommitAsync();
            return new { code = 200, message = "Không có mã giảm giá cần hoàn" };
        }
        
        // Giảm số lượng đã sử dụng
        var discount = usageHistory.Magiamgia;
        discount.Soluongdasudung = Math.Max(0, (discount.Soluongdasudung ?? 0) - 1);
        
        // Đánh dấu usage history (có thể soft delete hoặc update status)
        // Option 1: Xóa record
        _context.Lichsusudungmagiamgia.Remove(usageHistory);
        
        // Option 2: Đánh dấu là đã hủy (nếu có trường status)
        // usageHistory.Status = "Cancelled";
        
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return new { code = 200, message = "Hoàn mã giảm giá thành công" };
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return new { code = 500, message = "Lỗi khi hoàn mã: " + ex.Message };
    }
}
```

## Integration with Order Creation Flow

### Current Order Flow (Existing)

```
1. Validate giỏ hàng không rỗng
2. Kiểm tra tồn kho đủ
3. Tính tổng tiền
4. Tạo đơn hàng
5. Tạo chi tiết đơn hàng
6. Trừ tồn kho
7. Xóa giỏ hàng
8. Commit transaction
```

### Updated Order Flow (With Discount)

```
1. Validate giỏ hàng không rỗng
2. Kiểm tra tồn kho đủ
3. Tính tổng tiền ban đầu
4. [NEW] Áp dụng mã giảm giá (nếu có)
   4.1. Validate mã giảm giá
   4.2. Lock mã giảm giá
   4.3. Kiểm tra số lượng còn lại
   4.4. Tính giá trị giảm
   4.5. Cập nhật số lượng đã sử dụng
   4.6. Tạo usage history (Donhangid = null tạm thời)
5. [NEW] Tính tổng tiền sau giảm
6. Tạo đơn hàng (với Tongtien đã trừ discount)
7. [NEW] Update Donhangid vào usage history
8. Tạo chi tiết đơn hàng
9. Trừ tồn kho
10. Xóa giỏ hàng
11. Commit transaction
```

### Integration Points

#### 1. OrderServices Constructor

```csharp
public class OrderServices : IOrderServices
{
    private readonly MinhContext _context;
    private readonly IDiscountServices _discountServices; // NEW
    
    public OrderServices(MinhContext context, IDiscountServices discountServices)
    {
        _context = context;
        _discountServices = discountServices;
    }
}
```

#### 2. CreateOrderAsync Method

**Before:**
```csharp
decimal tongTien = giohang.Chitietgiohangs.Sum(ct => ct.Sanpham!.Gia * ct.Soluong);

var donhang = new Donhang
{
    Tongtien = tongTien,
    // ...
};
```

**After:**
```csharp
decimal tongTien = giohang.Chitietgiohangs.Sum(ct => ct.Sanpham!.Gia * ct.Soluong);

// Áp dụng mã giảm giá
decimal discountAmount = 0;
int? discountId = null;
if (!string.IsNullOrEmpty(request.DiscountCode))
{
    var discountResult = await _discountServices.ValidateAndApplyAsync(
        request.DiscountCode, tongTien, userId, 0);
    
    if (discountResult.code != 200)
    {
        await transaction.RollbackAsync();
        return discountResult;
    }
    
    discountAmount = discountResult.data.discountAmount;
    discountId = discountResult.data.discountId;
}

decimal finalAmount = Math.Max(0, tongTien - discountAmount);

var donhang = new Donhang
{
    Tongtien = finalAmount, // Sử dụng giá sau giảm
    // ...
};

// Update orderId vào usage history
if (discountId.HasValue)
{
    var usageRecord = await _context.Lichsusudungmagiamgia
        .FirstOrDefaultAsync(l => l.Magiamgiaid == discountId 
                               && l.Taikhoanid == userId 
                               && l.Donhangid == null);
    if (usageRecord != null)
    {
        usageRecord.Donhangid = donhang.Id;
    }
}
```

#### 3. CancelOrderAsync Method

**Before:**
```csharp
// Hoàn lại tồn kho
foreach (var chitiet in donhang.Chitietdonhangs)
{
    if (chitiet.Sanpham != null)
    {
        chitiet.Sanpham.Soluong += chitiet.Soluong;
        if (chitiet.Sanpham.Soluong > 0)
        {
            chitiet.Sanpham.Trangthai = true;
        }
    }
}

donhang.Trangthai = "Đã hủy";
```

**After:**
```csharp
// Hoàn lại tồn kho
foreach (var chitiet in donhang.Chitietdonhangs)
{
    if (chitiet.Sanpham != null)
    {
        chitiet.Sanpham.Soluong += chitiet.Soluong;
        if (chitiet.Sanpham.Soluong > 0)
        {
            chitiet.Sanpham.Trangthai = true;
        }
    }
}

// Hoàn lại mã giảm giá (NEW)
await _discountServices.RollbackDiscountAsync(orderId);

donhang.Trangthai = "Đã hủy";
```

### Error Handling in Integration

```csharp
// Trong CreateOrderAsync
if (!string.IsNullOrEmpty(request.DiscountCode))
{
    var discountResult = await _discountServices.ValidateAndApplyAsync(
        request.DiscountCode, tongTien, userId, 0);
    
    // Nếu mã giảm giá không hợp lệ, rollback toàn bộ transaction
    if (discountResult.code != 200)
    {
        await transaction.RollbackAsync();
        return new
        {
            code = discountResult.code,
            message = $"Lỗi mã giảm giá: {discountResult.message}"
        };
    }
    
    discountAmount = discountResult.data.discountAmount;
    discountId = discountResult.data.discountId;
}
```

### Response Format

**Successful Order with Discount:**
```json
{
  "code": 200,
  "message": "Đặt hàng thành công",
  "data": {
    "orderId": 123,
    "tongTien": 5000000,
    "discountAmount": 500000,
    "finalAmount": 4500000,
    "trangthai": "Chờ xử lý",
    "ngayDat": "2024-01-15T10:30:00"
  }
}
```

**Order Failed due to Invalid Discount:**
```json
{
  "code": 400,
  "message": "Lỗi mã giảm giá: Mã giảm giá đã hết"
}
```


## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system-essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property Reflection

After analyzing all acceptance criteria, I identified several areas where properties can be consolidated to eliminate redundancy:

**Consolidation 1: Validation Properties**
- Properties 1.2-1.9 (individual field validations) can be combined into a comprehensive validation property
- Properties 2.3 (validation consistency) is subsumed by the comprehensive validation property

**Consolidation 2: Existence Validation**
- Properties 2.2, 2.8, 3.2, 3.6, 5.6, 8.9, 9.2, 9.6 all test existence validation with 404 response
- These can be combined into one property about non-existent resource handling

**Consolidation 3: Immutability Properties**
- Properties 2.5 and 2.6 (immutable fields) can be combined into one property

**Consolidation 4: Discount Calculation**
- Properties 6.7 and 6.8 (percent vs fixed calculation) can be combined into one comprehensive calculation property

**Consolidation 5: Authorization Properties**
- Properties 12.2, 12.3, 12.6 (role-based access) can be combined into one comprehensive authorization property
- Properties 12.4, 12.5, 12.7 (authentication) can be combined into one authentication property

### Property 1: Discount Creation with Valid Data

*For any* valid discount code request with all required fields properly filled, creating the discount should result in a new record in the database with correct default values (Soluongdasudung = 0, Trangthai = true, Ngaytao = current time).

**Validates: Requirements 1.1, 1.10, 1.11, 1.12**

### Property 2: Comprehensive Input Validation

*For any* discount code creation or update request, the system should validate all business rules: unique code, end date > start date, discount type in ["percent", "fixed"], percent value 0-100 with required max discount, fixed value > 0, quantity > 0, minimum order value >= 0, and return specific error messages for each violation.

**Validates: Requirements 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8, 1.9, 1.13, 2.3**

### Property 3: Update Preserves Immutable Fields

*For any* existing discount code, updating it should preserve the original Macode and Loaigiamgia values regardless of what values are provided in the update request.

**Validates: Requirements 2.5, 2.6**

### Property 4: Update Quantity Constraint

*For any* discount code with N uses, attempting to update the total quantity to less than N should be rejected with an appropriate error message.

**Validates: Requirements 2.7**

### Property 5: Non-Existent Resource Returns 404

*For any* operation (update, delete, get detail, toggle status, get usage history) on a non-existent discount code ID, the system should return a 404 error code.

**Validates: Requirements 2.8, 3.6, 5.6, 8.9, 9.6**

### Property 6: Soft Delete Preserves Data

*For any* discount code, deleting it should set Trangthai to false while preserving all other data, maintaining relationships with usage history, and excluding it from active discount lists.

**Validates: Requirements 3.1, 3.3, 3.4, 3.5**

### Property 7: List Filtering and Pagination

*For any* discount list request with filters (status, time validity, keyword) and pagination parameters, the system should return only matching records, correctly paginated, sorted by creation date descending, with accurate remaining quantity calculations.

**Validates: Requirements 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7**

### Property 8: Detail Response Completeness

*For any* existing discount code, retrieving its details should return all fields (code, name, description, type, value, conditions, quantity, dates), calculated remaining quantity, total discount amount from usage history, and total usage count.

**Validates: Requirements 5.1, 5.2, 5.3, 5.4, 5.5**

### Property 9: Discount Validation Rules

*For any* discount code and order amount, validation should check: code exists and is active, current time is within validity period, remaining quantity > 0, and order amount >= minimum order value, returning specific error messages for each failure.

**Validates: Requirements 6.1, 6.2, 6.3, 6.4, 6.5, 6.9**

### Property 10: Discount Amount Calculation

*For any* valid discount code and order amount, the calculated discount should be: (orderAmount * value / 100) capped at max discount for percent type, or the fixed value for fixed type, and the final amount should be max(0, orderAmount - discount).

**Validates: Requirements 6.6, 6.7, 6.8, 7.7, 7.8**

### Property 11: Discount Application Atomicity

*For any* order creation with a valid discount code, the system should atomically: increment Soluongdasudung by 1, create a usage history record with all required fields (discount ID, account ID, order ID, actual discount amount, usage date), and apply the discount to the order total.

**Validates: Requirements 7.1, 7.2, 7.4, 7.5, 7.6, 7.10**

### Property 12: Quantity Constraint Enforcement

*For any* discount code with remaining quantity = 0, attempting to apply it should fail with an appropriate error message, and the Soluongdasudung should never exceed Soluong.

**Validates: Requirements 10.4, 10.5**

### Property 13: Discount Rollback on Order Cancellation

*For any* order with an applied discount that gets cancelled, the system should decrement Soluongdasudung by 1 (minimum 0), and update or mark the usage history record accordingly.

**Validates: Requirements 11.1, 11.3, 11.4, 11.5**

### Property 14: Usage History Retrieval and Aggregation

*For any* discount code, retrieving its usage history should return all usage records with complete information (account, order, actual discount, usage date), support filtering by date range and account, support pagination, sort by usage date descending, and calculate correct totals for discount amount and usage count.

**Validates: Requirements 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 8.7, 8.8**

### Property 15: Status Toggle Enforcement

*For any* discount code, toggling its status to false should prevent customers from applying it, while toggling to true should allow application if other conditions are met.

**Validates: Requirements 9.1, 9.3, 9.4**

### Property 16: Authentication Requirement

*For any* discount system endpoint, calling it without a valid JWT token should return a 401 Unauthorized error.

**Validates: Requirements 12.1, 12.4, 12.7**

### Property 17: Role-Based Authorization

*For any* admin-only endpoint (create, update, delete, usage history, toggle status), calling it with a Customer role should return 403 Forbidden, and for any customer endpoint (validate, apply), calling it with proper authentication should be allowed.

**Validates: Requirements 12.2, 12.3, 12.5, 12.6**


## Error Handling

### Error Response Format

Tất cả errors tuân theo format chuẩn của dự án:

```json
{
  "code": <HTTP_STATUS_CODE>,
  "message": "<ERROR_MESSAGE>"
}
```

### Error Categories

#### 1. Validation Errors (400 Bad Request)

**Triggers:**
- Missing required fields
- Invalid data types or formats
- Business rule violations (e.g., end date <= start date)
- Constraint violations (e.g., quantity < used count)

**Examples:**
```json
{ "code": 400, "message": "Mã code không được để trống" }
{ "code": 400, "message": "Ngày kết thúc phải lớn hơn ngày bắt đầu" }
{ "code": 400, "message": "Loại giảm giá phải là 'percent' hoặc 'fixed'" }
{ "code": 400, "message": "Giá trị giảm phần trăm phải từ 0 đến 100" }
{ "code": 400, "message": "Không thể giảm số lượng xuống dưới số lượng đã sử dụng" }
{ "code": 400, "message": "Mã giảm giá đã hết" }
{ "code": 400, "message": "Đơn hàng tối thiểu 500000" }
```

#### 2. Authentication Errors (401 Unauthorized)

**Triggers:**
- Missing JWT token
- Invalid JWT token
- Expired JWT token

**Examples:**
```json
{ "code": 401, "message": "Token không hợp lệ" }
{ "code": 401, "message": "Token đã hết hạn" }
{ "code": 401, "message": "Yêu cầu đăng nhập" }
```

#### 3. Authorization Errors (403 Forbidden)

**Triggers:**
- User role không đủ quyền truy cập endpoint
- Customer cố gắng truy cập admin endpoints

**Examples:**
```json
{ "code": 403, "message": "Bạn không có quyền truy cập chức năng này" }
{ "code": 403, "message": "Chỉ Admin mới có quyền tạo mã giảm giá" }
```

#### 4. Not Found Errors (404 Not Found)

**Triggers:**
- Discount code ID không tồn tại
- Resource không tìm thấy

**Examples:**
```json
{ "code": 404, "message": "Không tìm thấy mã giảm giá" }
{ "code": 404, "message": "Mã giảm giá không tồn tại" }
```

#### 5. Conflict Errors (409 Conflict)

**Triggers:**
- Duplicate discount code
- Concurrent modification conflicts

**Examples:**
```json
{ "code": 409, "message": "Mã code đã tồn tại trong hệ thống" }
```

#### 6. Server Errors (500 Internal Server Error)

**Triggers:**
- Database connection failures
- Unexpected exceptions
- Transaction failures

**Examples:**
```json
{ "code": 500, "message": "Đã xảy ra lỗi: <exception_message>" }
{ "code": 500, "message": "Lỗi cập nhật database: <exception_message>" }
{ "code": 500, "message": "Lỗi khi áp dụng mã giảm giá: <exception_message>" }
```

### Error Handling Patterns

#### 1. Validation Error Handling

```csharp
// Trong Service methods
var validationError = request.Validate();
if (validationError != null)
{
    return new { code = 400, message = validationError };
}

// Kiểm tra business rules
if (await _context.Magiamgia.AnyAsync(m => m.Macode == request.Macode))
{
    return new { code = 409, message = "Mã code đã tồn tại trong hệ thống" };
}
```

#### 2. Not Found Error Handling

```csharp
var discount = await _context.Magiamgia.FindAsync(id);
if (discount == null)
{
    return new { code = 404, message = "Không tìm thấy mã giảm giá" };
}
```

#### 3. Transaction Error Handling

```csharp
await using var transaction = await _context.Database
    .BeginTransactionAsync(IsolationLevel.Serializable);
try
{
    // Business logic
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
    return new { code = 200, message = "Thành công" };
}
catch (DbUpdateException ex)
{
    await transaction.RollbackAsync();
    return new { code = 500, message = "Lỗi cập nhật database: " + ex.Message };
}
catch (Exception ex)
{
    await transaction.RollbackAsync();
    return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
}
```

#### 4. Authorization Error Handling

```csharp
// Trong Controller với [Authorize] attribute
[Authorize(Roles = "Admin")]
public async Task<dynamic> Create([FromBody] CreateDiscountRequest request)
{
    // ASP.NET Core tự động trả về 401/403 nếu không đủ quyền
    return await _discountServices.CreateAsync(request);
}
```

### Logging Strategy

```csharp
// Log errors cho debugging và monitoring
catch (Exception ex)
{
    _logger.LogError(ex, "Error applying discount code {Code} for user {UserId}", 
        code, userId);
    await transaction.RollbackAsync();
    return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
}
```

### Retry Logic for Transient Failures

```csharp
// Trong DbContext configuration (Program.cs)
builder.Services.AddDbContext<MinhContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null
        );
    });
});
```

## Testing Strategy

### Dual Testing Approach

Hệ thống sử dụng kết hợp Unit Tests và Property-Based Tests để đảm bảo độ tin cậy toàn diện:

- **Unit Tests**: Kiểm tra các trường hợp cụ thể, edge cases, và integration points
- **Property-Based Tests**: Kiểm tra các properties phổ quát với nhiều inputs ngẫu nhiên

### Unit Testing

#### Focus Areas

1. **Specific Examples**: Các trường hợp sử dụng điển hình
2. **Edge Cases**: Các trường hợp biên (empty strings, zero values, boundary dates)
3. **Integration Points**: Tích hợp giữa DiscountServices và OrderServices
4. **Error Conditions**: Các trường hợp lỗi cụ thể

#### Example Unit Tests

```csharp
[Fact]
public async Task CreateDiscount_WithValidData_ShouldSucceed()
{
    // Arrange
    var request = new CreateDiscountRequest
    {
        Macode = "SUMMER2024",
        Tenchuongtrinh = "Summer Sale",
        Loaigiamgia = "percent",
        Giatrigiam = 20,
        Giamtoida = 100000,
        Soluong = 100,
        Ngaybatdau = DateTime.Now,
        Ngayketthuc = DateTime.Now.AddDays(30)
    };
    
    // Act
    var result = await _discountServices.CreateAsync(request);
    
    // Assert
    Assert.Equal(200, result.code);
    Assert.NotNull(result.data);
}

[Fact]
public async Task CreateDiscount_WithDuplicateCode_ShouldFail()
{
    // Arrange
    await CreateDiscountWithCode("DUPLICATE");
    var request = new CreateDiscountRequest { Macode = "DUPLICATE", ... };
    
    // Act
    var result = await _discountServices.CreateAsync(request);
    
    // Assert
    Assert.Equal(409, result.code);
    Assert.Contains("đã tồn tại", result.message);
}

[Fact]
public async Task ApplyDiscount_WhenOutOfStock_ShouldFail()
{
    // Arrange
    var discount = await CreateDiscountWithQuantity(1);
    await UseDiscount(discount.Id); // Use the only available slot
    
    // Act
    var result = await _discountServices.ValidateAndApplyAsync(
        discount.Macode, 1000000, userId, 0);
    
    // Assert
    Assert.Equal(400, result.code);
    Assert.Contains("đã hết", result.message);
}

[Fact]
public async Task CancelOrder_WithDiscount_ShouldRollbackQuantity()
{
    // Arrange
    var order = await CreateOrderWithDiscount();
    var initialUsedCount = await GetDiscountUsedCount(order.DiscountId);
    
    // Act
    await _orderServices.CancelOrderAsync(userId, order.Id);
    
    // Assert
    var finalUsedCount = await GetDiscountUsedCount(order.DiscountId);
    Assert.Equal(initialUsedCount - 1, finalUsedCount);
}
```

### Property-Based Testing

#### Configuration

- **Library**: Sử dụng **FsCheck** (cho C#/.NET)
- **Iterations**: Minimum 100 iterations per property test
- **Tagging**: Mỗi test phải reference design property tương ứng

#### Property Test Structure

```csharp
[Property(MaxTest = 100)]
public Property DiscountCreation_WithValidData_CreatesRecordWithDefaults()
{
    // Feature: discount-code-management, Property 1: Discount Creation with Valid Data
    
    return Prop.ForAll(
        ValidDiscountRequestGenerator(),
        async request =>
        {
            var result = await _discountServices.CreateAsync(request);
            
            Assert.Equal(200, result.code);
            
            var discount = await _context.Magiamgia
                .FirstAsync(m => m.Macode == request.Macode);
            
            Assert.Equal(0, discount.Soluongdasudung);
            Assert.True(discount.Trangthai);
            Assert.True((DateTime.Now - discount.Ngaytao.Value).TotalSeconds < 5);
        }
    );
}

[Property(MaxTest = 100)]
public Property InputValidation_RejectsInvalidRequests()
{
    // Feature: discount-code-management, Property 2: Comprehensive Input Validation
    
    return Prop.ForAll(
        InvalidDiscountRequestGenerator(),
        async request =>
        {
            var result = await _discountServices.CreateAsync(request);
            
            Assert.Equal(400, result.code);
            Assert.NotNull(result.message);
            Assert.NotEmpty(result.message);
        }
    );
}

[Property(MaxTest = 100)]
public Property DiscountCalculation_FollowsFormula()
{
    // Feature: discount-code-management, Property 10: Discount Amount Calculation
    
    return Prop.ForAll(
        ValidDiscountGenerator(),
        Arb.Generate<decimal>().Where(x => x > 0 && x < 100000000),
        async (discount, orderAmount) =>
        {
            var result = await _discountServices.ValidateDiscountAsync(
                discount.Macode, orderAmount);
            
            if (result.code == 200)
            {
                decimal expectedDiscount;
                if (discount.Loaigiamgia == "percent")
                {
                    expectedDiscount = Math.Min(
                        orderAmount * discount.Giatrigiam / 100,
                        discount.Giamtoida.Value
                    );
                }
                else
                {
                    expectedDiscount = discount.Giatrigiam;
                }
                
                Assert.Equal(expectedDiscount, result.data.discountAmount);
                Assert.Equal(
                    Math.Max(0, orderAmount - expectedDiscount),
                    result.data.finalAmount
                );
            }
        }
    );
}

[Property(MaxTest = 100)]
public Property QuantityConstraint_NeverExceeded()
{
    // Feature: discount-code-management, Property 12: Quantity Constraint Enforcement
    
    return Prop.ForAll(
        ValidDiscountGenerator(),
        async discount =>
        {
            // Try to use discount more times than available
            var tasks = Enumerable.Range(0, discount.Soluong + 10)
                .Select(i => _discountServices.ValidateAndApplyAsync(
                    discount.Macode, 1000000, i, 0));
            
            await Task.WhenAll(tasks);
            
            var finalDiscount = await _context.Magiamgia.FindAsync(discount.Id);
            Assert.True(finalDiscount.Soluongdasudung <= finalDiscount.Soluong);
        }
    );
}
```

#### Generators for Property Tests

```csharp
public static Gen<CreateDiscountRequest> ValidDiscountRequestGenerator()
{
    return from code in Arb.Generate<NonEmptyString>()
           from name in Arb.Generate<NonEmptyString>()
           from type in Gen.Elements("percent", "fixed")
           from value in type == "percent" 
               ? Gen.Choose(1, 100).Select(x => (decimal)x)
               : Gen.Choose(1000, 1000000).Select(x => (decimal)x)
           from maxDiscount in type == "percent"
               ? Gen.Choose(10000, 1000000).Select(x => (decimal?)x)
               : Gen.Constant((decimal?)null)
           from quantity in Gen.Choose(1, 1000)
           from startDate in Arb.Generate<DateTime>()
           from daysValid in Gen.Choose(1, 365)
           select new CreateDiscountRequest
           {
               Macode = code.Get,
               Tenchuongtrinh = name.Get,
               Loaigiamgia = type,
               Giatrigiam = value,
               Giamtoida = maxDiscount,
               Soluong = quantity,
               Ngaybatdau = startDate,
               Ngayketthuc = startDate.AddDays(daysValid)
           };
}

public static Gen<CreateDiscountRequest> InvalidDiscountRequestGenerator()
{
    return Gen.OneOf(
        // Empty code
        ValidDiscountRequestGenerator().Select(r => { r.Macode = ""; return r; }),
        // Invalid type
        ValidDiscountRequestGenerator().Select(r => { r.Loaigiamgia = "invalid"; return r; }),
        // End date before start date
        ValidDiscountRequestGenerator().Select(r => 
        { 
            r.Ngayketthuc = r.Ngaybatdau.AddDays(-1); 
            return r; 
        }),
        // Percent > 100
        ValidDiscountRequestGenerator().Select(r => 
        { 
            r.Loaigiamgia = "percent";
            r.Giatrigiam = 150; 
            return r; 
        }),
        // Zero quantity
        ValidDiscountRequestGenerator().Select(r => { r.Soluong = 0; return r; })
    );
}
```

### Test Coverage Goals

- **Unit Tests**: 80%+ code coverage
- **Property Tests**: 100% coverage of all 17 correctness properties
- **Integration Tests**: All critical paths (create order with discount, cancel order with discount)
- **Edge Cases**: All boundary conditions and error scenarios

### Testing Tools

- **Unit Testing Framework**: xUnit
- **Property-Based Testing**: FsCheck
- **Mocking**: Moq (for isolating dependencies)
- **Database**: In-memory SQLite or test database
- **Test Data**: Factory pattern for creating test entities

### Continuous Integration

```yaml
# Example CI pipeline
test:
  script:
    - dotnet test --configuration Release
    - dotnet test --filter "Category=PropertyTest" --configuration Release
  coverage: '/Total.*?(\d+\.?\d*)%/'
```

