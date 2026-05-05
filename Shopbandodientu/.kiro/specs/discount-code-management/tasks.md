# Implementation Plan: Discount Code Management

## Overview

Triển khai hệ thống quản lý mã giảm giá cho phép Admin tạo và quản lý các chương trình khuyến mãi, và cho phép Customer áp dụng mã giảm giá khi đặt hàng. Hệ thống tích hợp với OrderServices hiện tại, xử lý race condition với Serializable transaction, và hỗ trợ rollback khi hủy đơn hàng.

## Tasks

- [x] 1. Tạo DTOs cho discount code operations
  - Tạo file `Models/DTOs/CreateDiscountRequest.cs` với validation method
  - Tạo file `Models/DTOs/UpdateDiscountRequest.cs` với validation method
  - Tạo file `Models/DTOs/DiscountListRequest.cs` với validation method
  - Tạo file `Models/DTOs/ValidateDiscountRequest.cs` với validation method
  - Tạo file `Models/DTOs/ValidateDiscountResponse.cs`
  - Tạo file `Models/DTOs/DiscountDetailResponse.cs`
  - Tạo file `Models/DTOs/UsageHistoryRequest.cs`
  - Tạo file `Models/DTOs/ToggleDiscountStatusRequest.cs`
  - Cập nhật `Models/DTOs/CreateOrderRequest.cs` thêm field `DiscountCode` (optional)
  - _Requirements: 1.1-1.13, 2.1-2.8, 4.1-4.7, 5.1-5.6, 6.1-6.9, 8.1-8.9_

- [x] 2. Implement DiscountServices với business logic
  - [x] 2.1 Tạo interface và class structure
    - Tạo file `Services/DiscountServices.cs` với interface `IDiscountServices` và class `DiscountServices`
    - Inject `MinhContext` vào constructor
    - Định nghĩa tất cả method signatures theo design document
    - _Requirements: 1.1, 2.1, 3.1, 4.1, 5.1, 6.1, 7.1, 8.1, 9.1_

  - [x] 2.2 Implement CreateAsync method
    - Validate request với method `Validate()`
    - Kiểm tra mã code unique với `AnyAsync()`
    - Set default values: `Soluongdasudung = 0`, `Trangthai = true`, `Ngaytao = DateTime.Now`
    - Insert vào database và return response
    - _Requirements: 1.1-1.13_

  - [x] 2.3 Implement UpdateAsync method
    - Validate request và kiểm tra discount tồn tại
    - Validate không cho phép giảm số lượng xuống dưới số lượng đã sử dụng
    - Không cho phép thay đổi `Macode` và `Loaigiamgia`
    - Update các fields được phép và save changes
    - _Requirements: 2.1-2.8_

  - [x] 2.4 Implement DeleteAsync method (soft delete)
    - Kiểm tra discount tồn tại
    - Set `Trangthai = false` (không xóa khỏi database)
    - Giữ nguyên relationships với usage history
    - _Requirements: 3.1-3.6_

  - [x] 2.5 Implement GetListAsync method
    - Hỗ trợ filter theo `Trangthai`, `TimeFilter` (active/expired/upcoming), `Keyword`
    - Hỗ trợ pagination với `PageIndex` và `PageSize`
    - Sắp xếp theo `Ngaytao` descending
    - Tính `Soluongconlai = Soluong - Soluongdasudung`
    - Sử dụng `AsNoTracking()` và `Select()` chỉ lấy fields cần thiết
    - _Requirements: 4.1-4.7_

  - [x] 2.6 Implement GetDetailAsync method
    - Lấy thông tin đầy đủ của discount code
    - Tính `Soluongconlai`, `TongGiatriDagiam`, `TongLuotSudung` từ usage history
    - Return 404 nếu không tồn tại
    - _Requirements: 5.1-5.6_

  - [x] 2.7 Implement ValidateDiscountAsync method (cho Customer xem trước)
    - Validate discount tồn tại và `Trangthai = true`
    - Kiểm tra thời gian hiệu lực (current time trong khoảng `Ngaybatdau` đến `Ngayketthuc`)
    - Kiểm tra `Soluongconlai > 0`
    - Kiểm tra `orderAmount >= Giatridonhangtoithieu`
    - Tính discount amount theo công thức (percent hoặc fixed)
    - Return `ValidateDiscountResponse` với `IsValid`, `DiscountAmount`, `FinalAmount`
    - _Requirements: 6.1-6.9_

  - [x] 2.8 Implement ValidateAndApplyAsync method (gọi từ OrderServices)
    - Sử dụng `BeginTransactionAsync(IsolationLevel.Serializable)` để xử lý race condition
    - Lock discount record khi đọc với `FirstOrDefaultAsync()`
    - Validate tất cả điều kiện giống `ValidateDiscountAsync`
    - Kiểm tra lại `Soluongconlai > 0` sau khi lock
    - Increment `Soluongdasudung` lên 1
    - Tạo record trong `Lichsusudungmagiamgium` với `Donhangid = null` (tạm thời)
    - Commit transaction và return discount info
    - Rollback nếu có lỗi hoặc validation thất bại
    - _Requirements: 7.1-7.10, 10.1-10.6_

  - [x] 2.9 Implement RollbackDiscountAsync method
    - Tìm usage history record theo `Donhangid`
    - Decrement `Soluongdasudung` xuống 1 (minimum 0)
    - Xóa hoặc đánh dấu usage history record
    - Sử dụng transaction để đảm bảo atomicity
    - _Requirements: 11.1-11.6_

  - [x] 2.10 Implement ToggleStatusAsync method
    - Validate discount tồn tại
    - Update `Trangthai` field
    - Return success response
    - _Requirements: 9.1-9.6_

  - [x] 2.11 Implement GetUsageHistoryAsync method
    - Lấy usage history từ `Lichsusudungmagiamgium` với `Include` cho `Taikhoan` và `Donhang`
    - Hỗ trợ filter theo `FromDate`, `ToDate`, `TaikhoanId`
    - Hỗ trợ pagination
    - Sắp xếp theo `Ngaysudung` descending
    - Tính `TongGiatriDagiam` và `TongLuotSudung`
    - _Requirements: 8.1-8.9_

- [x] 3. Checkpoint - Ensure DiscountServices tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 4. Tạo DiscountController với endpoints
  - Tạo file `Controllers/DiscountController.cs`
  - Inject `IDiscountServices` vào constructor
  - Thêm `[Authorize(Roles = "Admin")]` cho controller
  - Implement endpoint `[HttpPost("create")]` gọi `CreateAsync`
  - Implement endpoint `[HttpPost("update")]` gọi `UpdateAsync`
  - Implement endpoint `[HttpPost("delete")]` gọi `DeleteAsync`
  - Implement endpoint `[HttpPost("list")]` gọi `GetListAsync`
  - Implement endpoint `[HttpGet("{id}")]` gọi `GetDetailAsync`
  - Implement endpoint `[HttpPost("toggle-status")]` gọi `ToggleStatusAsync`
  - Implement endpoint `[HttpGet("{id}/usage-history")]` gọi `GetUsageHistoryAsync`
  - Implement endpoint `[HttpPost("validate")]` với `[Authorize(Roles = "Customer")]` gọi `ValidateDiscountAsync`
  - Tất cả methods return `Task<dynamic>`
  - _Requirements: 12.1-12.7_

- [x] 5. Tích hợp discount vào OrderServices
  - [x] 5.1 Update OrderServices constructor
    - Inject `IDiscountServices` vào constructor của `OrderServices`
    - _Requirements: 7.1_

  - [x] 5.2 Update CreateOrderAsync method
    - Sau khi tính `tongTien` ban đầu, kiểm tra `request.DiscountCode`
    - Nếu có discount code, gọi `_discountServices.ValidateAndApplyAsync()`
    - Nếu validation thất bại, rollback transaction và return error
    - Tính `finalAmount = Math.Max(0, tongTien - discountAmount)`
    - Tạo đơn hàng với `Tongtien = finalAmount`
    - Sau khi tạo đơn hàng, update `Donhangid` trong usage history record
    - Return response bao gồm `tongTien`, `discountAmount`, `finalAmount`
    - _Requirements: 7.1-7.10_

  - [x] 5.3 Update CancelOrderAsync method
    - Sau khi hoàn lại tồn kho, gọi `_discountServices.RollbackDiscountAsync(orderId)`
    - Đảm bảo rollback discount trong cùng transaction với cancel order
    - _Requirements: 11.1-11.6_

- [x] 6. Đăng ký Dependency Injection
  - Mở file `Program.cs`
  - Thêm dòng `builder.Services.AddScoped<IDiscountServices, DiscountServices>();`
  - Đảm bảo đăng ký trước `builder.Build()`
  - _Requirements: 1.1, 2.1, 3.1, 4.1, 5.1, 6.1, 7.1, 8.1, 9.1_

- [x] 7. Final checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tất cả tasks tuân thủ patterns của dự án: Interface và class cùng file, validation trong DTO, transaction cho operations phức tạp
- Sử dụng `IsolationLevel.Serializable` cho `ValidateAndApplyAsync` để xử lý race condition
- Soft delete cho discount codes (set `Trangthai = false`)
- Tích hợp liền mạch với OrderServices hiện tại
- Response format chuẩn: `{ code, message, data }`
- Authorization: Admin cho CRUD operations, Customer cho validate và apply
