# Requirements Document

## Introduction

Hệ thống quản lý mã giảm giá (Discount Code Management) cho phép Admin tạo, quản lý các chương trình khuyến mãi và cho phép khách hàng áp dụng mã giảm giá khi đặt hàng. Đây là công cụ marketing quan trọng để thu hút và giữ chân khách hàng trong hệ thống thương mại điện tử.

## Glossary

- **Discount_System**: Hệ thống quản lý mã giảm giá
- **Admin**: Người quản trị có toàn quyền quản lý mã giảm giá
- **Customer**: Khách hàng có thể áp dụng mã giảm giá khi đặt hàng
- **Discount_Code**: Mã giảm giá (voucher) với mã code duy nhất
- **Usage_History**: Lịch sử sử dụng mã giảm giá
- **Order**: Đơn hàng của khách hàng
- **Discount_Code_Entity**: Entity Magiamgium trong database
- **Usage_History_Entity**: Entity Lichsusudungmagiamgium trong database
- **Discount_Service**: Service layer xử lý logic nghiệp vụ mã giảm giá
- **Discount_Controller**: Controller xử lý HTTP requests cho mã giảm giá

## Requirements

### Requirement 1: Tạo mã giảm giá

**User Story:** Là Admin, tôi muốn tạo mã giảm giá mới, để có thể chạy các chương trình khuyến mãi thu hút khách hàng.

#### Acceptance Criteria

1. WHEN Admin gửi request tạo mã giảm giá với đầy đủ thông tin hợp lệ, THE Discount_System SHALL tạo Discount_Code mới trong database
2. THE Discount_System SHALL validate mã code là duy nhất (không trùng với mã đã tồn tại)
3. THE Discount_System SHALL validate ngày kết thúc phải lớn hơn ngày bắt đầu
4. THE Discount_System SHALL validate loại giảm giá chỉ nhận giá trị "percent" hoặc "fixed"
5. WHEN loại giảm giá là "percent", THE Discount_System SHALL validate giá trị giảm từ 0 đến 100
6. WHEN loại giảm giá là "percent", THE Discount_System SHALL yêu cầu giá trị giảm tối đa
7. WHEN loại giảm giá là "fixed", THE Discount_System SHALL validate giá trị giảm lớn hơn 0
8. THE Discount_System SHALL validate số lượng mã lớn hơn 0
9. THE Discount_System SHALL validate giá trị đơn hàng tối thiểu lớn hơn hoặc bằng 0
10. THE Discount_System SHALL set số lượng đã sử dụng mặc định là 0
11. THE Discount_System SHALL set trạng thái mặc định là true (hoạt động)
12. THE Discount_System SHALL set ngày tạo là thời điểm hiện tại
13. IF validation thất bại, THEN THE Discount_System SHALL trả về thông báo lỗi cụ thể

### Requirement 2: Cập nhật mã giảm giá

**User Story:** Là Admin, tôi muốn chỉnh sửa thông tin mã giảm giá đã tạo, để điều chỉnh chương trình khuyến mãi khi cần thiết.

#### Acceptance Criteria

1. WHEN Admin gửi request cập nhật mã giảm giá với ID hợp lệ, THE Discount_System SHALL cập nhật thông tin Discount_Code trong database
2. THE Discount_System SHALL validate Discount_Code tồn tại trước khi cập nhật
3. THE Discount_System SHALL áp dụng tất cả validation rules giống như khi tạo mới
4. THE Discount_System SHALL cho phép cập nhật tên chương trình, mô tả, số lượng, thời gian hiệu lực
5. THE Discount_System SHALL không cho phép thay đổi mã code sau khi đã tạo
6. THE Discount_System SHALL không cho phép thay đổi loại giảm giá sau khi đã tạo
7. THE Discount_System SHALL không cho phép giảm số lượng mã xuống dưới số lượng đã sử dụng
8. IF Discount_Code không tồn tại, THEN THE Discount_System SHALL trả về lỗi 404

### Requirement 3: Xóa mã giảm giá

**User Story:** Là Admin, tôi muốn xóa mã giảm giá không còn sử dụng, để dọn dẹp dữ liệu và tránh nhầm lẫn.

#### Acceptance Criteria

1. WHEN Admin gửi request xóa mã giảm giá với ID hợp lệ, THE Discount_System SHALL thực hiện soft-delete bằng cách set trạng thái là false
2. THE Discount_System SHALL validate Discount_Code tồn tại trước khi xóa
3. THE Discount_System SHALL giữ nguyên dữ liệu trong database để bảo toàn lịch sử
4. THE Discount_System SHALL không hiển thị mã đã xóa trong danh sách mã hoạt động
5. THE Discount_System SHALL vẫn giữ liên kết với Usage_History để tra cứu lịch sử
6. IF Discount_Code không tồn tại, THEN THE Discount_System SHALL trả về lỗi 404

### Requirement 4: Xem danh sách mã giảm giá

**User Story:** Là Admin, tôi muốn xem danh sách tất cả mã giảm giá, để theo dõi và quản lý các chương trình khuyến mãi.

#### Acceptance Criteria

1. WHEN Admin gửi request xem danh sách mã giảm giá, THE Discount_System SHALL trả về tất cả Discount_Code
2. THE Discount_System SHALL hỗ trợ lọc theo trạng thái (hoạt động/đã xóa)
3. THE Discount_System SHALL hỗ trợ lọc theo thời gian hiệu lực (đang hiệu lực/hết hạn/chưa bắt đầu)
4. THE Discount_System SHALL hỗ trợ tìm kiếm theo mã code hoặc tên chương trình
5. THE Discount_System SHALL hỗ trợ phân trang với page size và page number
6. THE Discount_System SHALL sắp xếp mặc định theo ngày tạo giảm dần
7. THE Discount_System SHALL hiển thị số lượng còn lại (số lượng - số lượng đã sử dụng)

### Requirement 5: Xem chi tiết mã giảm giá

**User Story:** Là Admin, tôi muốn xem chi tiết một mã giảm giá cụ thể, để kiểm tra thông tin và hiệu quả sử dụng.

#### Acceptance Criteria

1. WHEN Admin gửi request xem chi tiết mã giảm giá với ID hợp lệ, THE Discount_System SHALL trả về thông tin đầy đủ của Discount_Code
2. THE Discount_System SHALL bao gồm tất cả thông tin: mã code, tên, mô tả, loại giảm giá, giá trị, điều kiện, số lượng, thời gian
3. THE Discount_System SHALL tính và hiển thị số lượng còn lại
4. THE Discount_System SHALL tính và hiển thị tổng giá trị đã giảm từ Usage_History
5. THE Discount_System SHALL hiển thị số lượt sử dụng thành công
6. IF Discount_Code không tồn tại, THEN THE Discount_System SHALL trả về lỗi 404

### Requirement 6: Kiểm tra tính hợp lệ của mã giảm giá

**User Story:** Là Customer, tôi muốn kiểm tra mã giảm giá trước khi đặt hàng, để biết mã có hợp lệ và được giảm bao nhiêu.

#### Acceptance Criteria

1. WHEN Customer gửi request kiểm tra mã giảm giá với mã code và giá trị đơn hàng, THE Discount_System SHALL validate tính hợp lệ của Discount_Code
2. THE Discount_System SHALL kiểm tra Discount_Code tồn tại và có trạng thái hoạt động
3. THE Discount_System SHALL kiểm tra thời gian hiện tại nằm trong khoảng từ ngày bắt đầu đến ngày kết thúc
4. THE Discount_System SHALL kiểm tra số lượng còn lại lớn hơn 0
5. THE Discount_System SHALL kiểm tra giá trị đơn hàng lớn hơn hoặc bằng giá trị đơn hàng tối thiểu
6. WHEN tất cả điều kiện hợp lệ, THE Discount_System SHALL tính và trả về giá trị giảm thực tế
7. WHEN loại giảm giá là "percent", THE Discount_System SHALL tính giá trị giảm = giá trị đơn hàng * giá trị giảm / 100, tối đa là giảm tối đa
8. WHEN loại giảm giá là "fixed", THE Discount_System SHALL trả về giá trị giảm cố định
9. IF bất kỳ điều kiện nào không hợp lệ, THEN THE Discount_System SHALL trả về thông báo lỗi cụ thể

### Requirement 7: Áp dụng mã giảm giá khi đặt hàng

**User Story:** Là Customer, tôi muốn áp dụng mã giảm giá khi đặt hàng, để được giảm giá và tiết kiệm chi phí.

#### Acceptance Criteria

1. WHEN Customer đặt hàng với mã giảm giá hợp lệ, THE Discount_System SHALL áp dụng giảm giá vào Order
2. THE Discount_System SHALL thực hiện tất cả validation giống như kiểm tra tính hợp lệ
3. THE Discount_System SHALL sử dụng database transaction để đảm bảo tính nhất quán
4. WITHIN transaction, THE Discount_System SHALL tăng số lượng đã sử dụng của Discount_Code lên 1
5. WITHIN transaction, THE Discount_System SHALL tạo bản ghi mới trong Usage_History_Entity
6. THE Discount_System SHALL lưu thông tin: mã giảm giá ID, tài khoản ID, đơn hàng ID, giá trị giảm thực tế, ngày sử dụng
7. THE Discount_System SHALL tính tổng tiền đơn hàng sau khi trừ giá trị giảm
8. THE Discount_System SHALL đảm bảo tổng tiền sau giảm không âm
9. IF validation thất bại hoặc xảy ra lỗi, THEN THE Discount_System SHALL rollback transaction
10. IF transaction thành công, THE Discount_System SHALL commit và trả về thông tin đơn hàng với giá trị giảm

### Requirement 8: Xem lịch sử sử dụng mã giảm giá

**User Story:** Là Admin, tôi muốn xem lịch sử sử dụng mã giảm giá, để đánh giá hiệu quả chương trình khuyến mãi.

#### Acceptance Criteria

1. WHEN Admin gửi request xem lịch sử sử dụng với ID mã giảm giá, THE Discount_System SHALL trả về tất cả bản ghi từ Usage_History
2. THE Discount_System SHALL bao gồm thông tin: tài khoản sử dụng, đơn hàng, giá trị giảm thực tế, ngày sử dụng
3. THE Discount_System SHALL hỗ trợ lọc theo khoảng thời gian
4. THE Discount_System SHALL hỗ trợ lọc theo tài khoản
5. THE Discount_System SHALL hỗ trợ phân trang
6. THE Discount_System SHALL sắp xếp mặc định theo ngày sử dụng giảm dần
7. THE Discount_System SHALL tính tổng giá trị đã giảm
8. THE Discount_System SHALL tính tổng số lượt sử dụng
9. IF mã giảm giá không tồn tại, THEN THE Discount_System SHALL trả về lỗi 404

### Requirement 9: Kích hoạt/Vô hiệu hóa mã giảm giá

**User Story:** Là Admin, tôi muốn tạm dừng hoặc kích hoạt lại mã giảm giá, để kiểm soát thời điểm chương trình khuyến mãi có hiệu lực.

#### Acceptance Criteria

1. WHEN Admin gửi request thay đổi trạng thái mã giảm giá, THE Discount_System SHALL cập nhật trường trạng thái của Discount_Code
2. THE Discount_System SHALL validate Discount_Code tồn tại
3. WHEN trạng thái được set là false, THE Discount_System SHALL không cho phép Customer áp dụng mã
4. WHEN trạng thái được set là true, THE Discount_System SHALL cho phép Customer áp dụng mã nếu các điều kiện khác hợp lệ
5. THE Discount_System SHALL ghi log thay đổi trạng thái
6. IF Discount_Code không tồn tại, THEN THE Discount_System SHALL trả về lỗi 404

### Requirement 10: Xử lý race condition khi sử dụng mã giảm giá

**User Story:** Là hệ thống, tôi cần xử lý đúng khi nhiều người dùng cùng sử dụng mã giảm giá đồng thời, để đảm bảo số lượng mã không bị vượt quá giới hạn.

#### Acceptance Criteria

1. WHEN nhiều Customer đồng thời áp dụng cùng một Discount_Code, THE Discount_System SHALL sử dụng database transaction với isolation level Serializable
2. THE Discount_System SHALL lock bản ghi Discount_Code khi đọc số lượng còn lại
3. THE Discount_System SHALL kiểm tra lại số lượng còn lại sau khi lock
4. IF số lượng còn lại không đủ sau khi lock, THEN THE Discount_System SHALL rollback transaction và trả về lỗi
5. THE Discount_System SHALL đảm bảo số lượng đã sử dụng không vượt quá số lượng tổng
6. THE Discount_System SHALL xử lý timeout và retry logic phù hợp

### Requirement 11: Xử lý khi hủy đơn hàng có mã giảm giá

**User Story:** Là hệ thống, tôi cần hoàn lại số lượng mã giảm giá khi đơn hàng bị hủy, để mã có thể được sử dụng lại.

#### Acceptance Criteria

1. WHEN Order có sử dụng Discount_Code bị hủy, THE Discount_System SHALL giảm số lượng đã sử dụng của Discount_Code xuống 1
2. THE Discount_System SHALL sử dụng database transaction để đảm bảo tính nhất quán
3. THE Discount_System SHALL validate Order tồn tại và có sử dụng mã giảm giá
4. THE Discount_System SHALL cập nhật hoặc đánh dấu bản ghi trong Usage_History
5. THE Discount_System SHALL đảm bảo số lượng đã sử dụng không âm
6. IF xảy ra lỗi, THEN THE Discount_System SHALL rollback transaction

### Requirement 12: Authorization và phân quyền

**User Story:** Là hệ thống, tôi cần kiểm soát quyền truy cập các chức năng mã giảm giá, để đảm bảo bảo mật và phân quyền đúng.

#### Acceptance Criteria

1. THE Discount_System SHALL yêu cầu authentication cho tất cả endpoints
2. THE Discount_System SHALL chỉ cho phép Admin truy cập các endpoint: tạo, sửa, xóa, xem lịch sử, kích hoạt/vô hiệu hóa
3. THE Discount_System SHALL cho phép Customer truy cập các endpoint: kiểm tra tính hợp lệ, áp dụng mã
4. THE Discount_System SHALL validate JWT token hợp lệ
5. THE Discount_System SHALL kiểm tra role từ JWT token claims
6. IF user không có quyền, THEN THE Discount_System SHALL trả về lỗi 403 Forbidden
7. IF token không hợp lệ hoặc hết hạn, THEN THE Discount_System SHALL trả về lỗi 401 Unauthorized
