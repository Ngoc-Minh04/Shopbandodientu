# Discount Code Management - API Endpoints

## Admin Endpoints (Yêu cầu role: Admin)

### 1. Tạo mã giảm giá
```
POST /api/Discount/tao
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "macode": "SUMMER2024",
  "tenchuongtrinh": "Khuyến mãi mùa hè",
  "mota": "Giảm giá 20% cho đơn hàng từ 500k",
  "loaigiamgia": "percent",  // "percent" hoặc "fixed"
  "giatrigiam": 20,
  "giamtoida": 100000,  // Bắt buộc nếu loaigiamgia = "percent"
  "giatridonhangtoithieu": 500000,
  "soluong": 100,
  "ngaybatdau": "2024-06-01T00:00:00",
  "ngayketthuc": "2024-08-31T23:59:59"
}
```

### 2. Cập nhật mã giảm giá
```
POST /api/Discount/capnhat
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "id": 1,
  "tenchuongtrinh": "Khuyến mãi mùa hè 2024",
  "mota": "Giảm giá 25% cho đơn hàng từ 500k",
  "giatrigiam": 25,
  "giamtoida": 150000,
  "giatridonhangtoithieu": 500000,
  "soluong": 150,
  "ngaybatdau": "2024-06-01T00:00:00",
  "ngayketthuc": "2024-08-31T23:59:59"
}

Lưu ý: Không thể thay đổi macode và loaigiamgia
```

### 3. Xóa mã giảm giá (Soft delete)
```
POST /api/Discount/xoa
Authorization: Bearer {token}
Content-Type: application/json

Body: 1  // ID của mã giảm giá
```

### 4. Lấy danh sách mã giảm giá
```
POST /api/Discount/danhsach
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "keyword": "SUMMER",  // Tìm kiếm theo mã code hoặc tên (optional)
  "trangthai": true,  // true = hoạt động, false = đã xóa (optional)
  "timeFilter": "active",  // "active", "expired", "upcoming" (optional)
  "pageIndex": 1,
  "pageSize": 10
}
```

### 5. Xem chi tiết mã giảm giá
```
GET /api/Discount/chitiet/{id}
Authorization: Bearer {token}
```

### 6. Kích hoạt/Vô hiệu hóa mã giảm giá
```
POST /api/Discount/doitrangthai
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "id": 1,
  "trangthai": false  // true = kích hoạt, false = vô hiệu hóa
}
```

### 7. Xem lịch sử sử dụng mã giảm giá
```
GET /api/Discount/lichsusudung/{id}?fromDate=2024-06-01&toDate=2024-08-31&taikhoanId=5&pageIndex=1&pageSize=10
Authorization: Bearer {token}

Query Parameters (tất cả optional):
- fromDate: Từ ngày
- toDate: Đến ngày
- taikhoanId: Lọc theo tài khoản
- pageIndex: Trang hiện tại (default: 1)
- pageSize: Số items mỗi trang (default: 10)
```

## Customer Endpoints (Yêu cầu role: Customer)

### 8. Kiểm tra tính hợp lệ của mã giảm giá
```
POST /api/Discount/kiemtra
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "macode": "SUMMER2024",
  "orderAmount": 1000000
}

Response:
{
  "code": 200,
  "message": "Mã giảm giá hợp lệ",
  "data": {
    "isValid": true,
    "message": "Mã giảm giá hợp lệ",
    "discountAmount": 100000,
    "finalAmount": 900000,
    "discountId": 1,
    "macode": "SUMMER2024",
    "tenchuongtrinh": "Khuyến mãi mùa hè"
  }
}
```

### 9. Áp dụng mã giảm giá khi đặt hàng
```
POST /api/Order/taodathang
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "tennguoinhan": "Nguyễn Văn A",
  "diachigiaohang": "123 Đường ABC, Quận 1, TP.HCM",
  "sdtnguoinhan": "0901234567",
  "discountCode": "SUMMER2024"  // Optional - Mã giảm giá
}

Response:
{
  "code": 200,
  "message": "Đặt hàng thành công",
  "data": {
    "orderId": 123,
    "tongTien": 1000000,
    "discountAmount": 100000,
    "finalAmount": 900000,
    "trangthai": "Chờ xử lý",
    "ngayDat": "2024-06-15T10:30:00"
  }
}
```

## Lưu ý quan trọng

### Validation Rules
1. **Mã code**: Phải unique, không được trùng
2. **Loại giảm giá**:
   - `percent`: Giá trị từ 0-100, bắt buộc có giảm tối đa
   - `fixed`: Giá trị phải > 0
3. **Ngày**: Ngày kết thúc phải > ngày bắt đầu
4. **Số lượng**: Phải > 0, không thể giảm xuống dưới số lượng đã sử dụng

### Race Condition Handling
- Hệ thống sử dụng Serializable transaction để xử lý race condition
- Đảm bảo số lượng mã không bị vượt quá giới hạn khi nhiều người dùng đồng thời

### Rollback khi hủy đơn hàng
- Khi hủy đơn hàng có sử dụng mã giảm giá, số lượng sẽ được hoàn lại tự động
- Lịch sử sử dụng sẽ bị xóa

### Authorization
- Admin endpoints: Chỉ Admin mới có quyền truy cập
- Customer endpoints: Customer có thể kiểm tra và áp dụng mã
- Tất cả endpoints yêu cầu JWT token hợp lệ
