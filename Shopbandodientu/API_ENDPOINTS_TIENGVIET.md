# API Endpoints - Tiếng Việt Không Dấu

## 1. Authentication (Xác thực) - `/api/xacthuc`

### Đăng ký
```
POST /api/xacthuc/dangky
Content-Type: application/json

Body:
{
  "hoten": "Nguyễn Văn A",
  "email": "nguyenvana@example.com",
  "matkhau": "Password123!",
  "sodienthoai": "0901234567",
  "diachi": "123 Đường ABC, Quận 1, TP.HCM"
}
```

### Đăng nhập
```
POST /api/xacthuc/dangnhap
Content-Type: application/json

Body:
{
  "email": "nguyenvana@example.com",
  "matkhau": "Password123!"
}

Response:
{
  "code": 200,
  "message": "Đăng nhập thành công",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "userId": 1,
    "hoten": "Nguyễn Văn A",
    "email": "nguyenvana@example.com",
    "role": "Customer"
  }
}
```

---

## 2. Product (Sản phẩm) - `/api/sanpham`

### Lấy danh sách sản phẩm (Public)
```
POST /api/sanpham/danhsach
Content-Type: application/json

Body:
{
  "keyword": "laptop",  // optional
  "danhmucId": 1,  // optional
  "minPrice": 5000000,  // optional
  "maxPrice": 20000000,  // optional
  "sortBy": "gia",  // "gia", "ten", "ngaytao" - optional
  "sortOrder": "asc",  // "asc", "desc" - optional
  "pageIndex": 1,
  "pageSize": 10
}
```

### Lấy chi tiết sản phẩm (Public)
```
GET /api/sanpham/chitiet/{id}
```

### Tạo sản phẩm (Admin only)
```
POST /api/sanpham/tao
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "tensanpham": "Laptop Dell XPS 15",
  "mota": "Laptop cao cấp cho dân văn phòng",
  "gia": 25000000,
  "soluong": 50,
  "thuonghieu": "Dell",
  "danhmucid": 1,
  "hinhanh": ["url1", "url2"],
  "thongso": [
    { "ten": "CPU", "giatri": "Intel Core i7" },
    { "ten": "RAM", "giatri": "16GB" }
  ]
}
```

### Cập nhật sản phẩm (Admin only)
```
POST /api/sanpham/capnhat
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "id": 1,
  "tensanpham": "Laptop Dell XPS 15 2024",
  "mota": "Laptop cao cấp phiên bản mới",
  "gia": 27000000,
  "soluong": 30,
  "thuonghieu": "Dell",
  "danhmucid": 1
}
```

### Xóa sản phẩm (Admin only)
```
POST /api/sanpham/xoa
Authorization: Bearer {token}
Content-Type: application/json

Body: 1  // ID sản phẩm
```

---

## 3. Category (Danh mục) - `/api/danhmuc`

### Lấy danh sách danh mục (Public)
```
GET /api/danhmuc/danhsach
```

### Lấy chi tiết danh mục (Public)
```
GET /api/danhmuc/chitiet/{id}
```

### Tạo danh mục (Admin only)
```
POST /api/danhmuc/tao
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "tendanhmuc": "Laptop",
  "mota": "Các loại laptop"
}
```

### Cập nhật danh mục (Admin only)
```
POST /api/danhmuc/capnhat
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "id": 1,
  "tendanhmuc": "Laptop Gaming",
  "mota": "Laptop chuyên game"
}
```

### Xóa danh mục (Admin only)
```
POST /api/danhmuc/xoa
Authorization: Bearer {token}
Content-Type: application/json

Body: 1  // ID danh mục
```

---

## 4. Cart (Giỏ hàng) - `/api/giohang` (Customer only)

### Thêm sản phẩm vào giỏ
```
POST /api/giohang/them
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "sanphamId": 1,
  "soluong": 2
}
```

### Xem giỏ hàng
```
GET /api/giohang/xem
Authorization: Bearer {token}
```

### Cập nhật số lượng
```
POST /api/giohang/capnhat
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "sanphamId": 1,
  "soluong": 3
}
```

### Xóa sản phẩm khỏi giỏ
```
POST /api/giohang/xoa
Authorization: Bearer {token}
Content-Type: application/json

Body: 1  // ID sản phẩm
```

### Xóa toàn bộ giỏ hàng
```
POST /api/giohang/xoatatca
Authorization: Bearer {token}
```

---

## 5. Order (Đơn hàng) - `/api/donhang` (Customer only)

### Đặt hàng
```
POST /api/donhang/taodathang
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "tennguoinhan": "Nguyễn Văn A",
  "diachigiaohang": "123 Đường ABC, Quận 1, TP.HCM",
  "sdtnguoinhan": "0901234567",
  "discountCode": "SUMMER2024"  // optional
}
```

### Xem lịch sử đơn hàng
```
GET /api/donhang/lichsu
Authorization: Bearer {token}
```

### Xem chi tiết đơn hàng
```
GET /api/donhang/chitiet/{id}
Authorization: Bearer {token}
```

### Hủy đơn hàng
```
POST /api/donhang/huy
Authorization: Bearer {token}
Content-Type: application/json

Body: 123  // ID đơn hàng
```

---

## 6. Discount (Mã giảm giá) - `/api/magiamgia`

### Admin Endpoints (Yêu cầu role: Admin)

#### Tạo mã giảm giá
```
POST /api/magiamgia/tao
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "macode": "SUMMER2024",
  "tenchuongtrinh": "Khuyến mãi mùa hè",
  "mota": "Giảm giá 20% cho đơn hàng từ 500k",
  "loaigiamgia": "percent",  // "percent" hoặc "fixed"
  "giatrigiam": 20,
  "giamtoida": 100000,
  "giatridonhangtoithieu": 500000,
  "soluong": 100,
  "ngaybatdau": "2024-06-01T00:00:00",
  "ngayketthuc": "2024-08-31T23:59:59"
}
```

#### Cập nhật mã giảm giá
```
POST /api/magiamgia/capnhat
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "id": 1,
  "tenchuongtrinh": "Khuyến mãi mùa hè 2024",
  "mota": "Giảm giá 25%",
  "giatrigiam": 25,
  "giamtoida": 150000,
  "giatridonhangtoithieu": 500000,
  "soluong": 150,
  "ngaybatdau": "2024-06-01T00:00:00",
  "ngayketthuc": "2024-08-31T23:59:59"
}
```

#### Xóa mã giảm giá
```
POST /api/magiamgia/xoa
Authorization: Bearer {token}
Content-Type: application/json

Body: 1  // ID mã giảm giá
```

#### Lấy danh sách mã giảm giá
```
POST /api/magiamgia/danhsach
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "keyword": "SUMMER",  // optional
  "trangthai": true,  // optional
  "timeFilter": "active",  // "active", "expired", "upcoming" - optional
  "pageIndex": 1,
  "pageSize": 10
}
```

#### Xem chi tiết mã giảm giá
```
GET /api/magiamgia/chitiet/{id}
Authorization: Bearer {token}
```

#### Kích hoạt/Vô hiệu hóa mã
```
POST /api/magiamgia/doitrangthai
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "id": 1,
  "trangthai": false  // true = kích hoạt, false = vô hiệu hóa
}
```

#### Xem lịch sử sử dụng
```
GET /api/magiamgia/lichsusudung/{id}?fromDate=2024-06-01&toDate=2024-08-31&pageIndex=1&pageSize=10
Authorization: Bearer {token}
```

### Customer Endpoint

#### Kiểm tra mã giảm giá
```
POST /api/magiamgia/kiemtra
Authorization: Bearer {token}
Content-Type: application/json

Body:
{
  "macode": "SUMMER2024",
  "orderAmount": 1000000
}
```

---

## Tổng hợp Routes

| Controller | Route Base | Mô tả |
|------------|-----------|-------|
| AuthController | `/api/xacthuc` | Đăng ký, đăng nhập |
| ProductController | `/api/sanpham` | Quản lý sản phẩm |
| CategoryController | `/api/danhmuc` | Quản lý danh mục |
| CartController | `/api/giohang` | Quản lý giỏ hàng |
| OrderController | `/api/donhang` | Quản lý đơn hàng |
| DiscountController | `/api/magiamgia` | Quản lý mã giảm giá |

## Lưu ý

- Tất cả endpoints yêu cầu `Content-Type: application/json`
- Endpoints có `Authorization: Bearer {token}` cần JWT token hợp lệ
- Admin endpoints yêu cầu role "Admin"
- Customer endpoints yêu cầu role "Customer"
- Public endpoints không cần authentication
