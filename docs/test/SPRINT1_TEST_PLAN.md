# KẾ HOẠCH & DANH SÁCH KIỂM THỬ (TEST PLAN) - SPRINT 1
**Dự án:** Ứng dụng học ngôn ngữ (SE347)
**Người phụ trách:** Lê Văn Huy (Tester/Op)

---

## 1. PHẠM VI KIỂM THỬ SPRINT 1
* Xác thực: Đăng ký, Đăng nhập, Đăng xuất, Quên mật khẩu.
* Giao diện & Trải nghiệm: Chuyển đổi Dark/Light mode, Chuyển đổi ngôn ngữ (EN/VI).
* Giao diện Dashboard cơ bản.

---

## 2. BẢNG TEST CASES CHI TIẾT

| Mã TC | Chức năng | Hành động kiểm thử | Kết quả mong đợi | Trạng thái |
| :--- | :--- | :--- | :--- | :--- |
| **TC-AUTH-01** | Đăng ký | Để trống tất cả các trường và bấm Đăng ký | Hiển thị thông báo lỗi yêu cầu nhập đủ thông tin | Chưa test |
| **TC-AUTH-02** | Đăng ký | Nhập email sai định dạng (ví dụ: `abc@`) | Hiển thị lỗi email không hợp lệ | Chưa test |
| **TC-AUTH-03** | Đăng ký | Nhập mật khẩu xác nhận không khớp | Báo lỗi mật khẩu xác nhận không trùng khớp | Chưa test |
| **TC-AUTH-04** | Đăng ký | Nhập email đã tồn tại trong hệ thống | Báo lỗi email đã được đăng ký | Chưa test |
| **TC-AUTH-05** | Đăng ký | Điền đầy đủ thông tin hợp lệ | Đăng ký thành công, điều hướng về màn hình Đăng nhập | Chưa test |
| **TC-AUTH-06** | Đăng nhập | Nhập sai tài khoản hoặc mật khẩu | Báo lỗi thông tin đăng nhập không chính xác | Chưa test |
| **TC-AUTH-07** | Đăng nhập | Nhập đúng tài khoản và mật khẩu | Đăng nhập thành công, chuyển hướng vào Dashboard | Chưa test |
| **TC-UI-01** | Theme | Bấm nút chuyển đổi Dark Mode | Giao diện đổi sang nền tối, chữ sáng, màu sắc dễ nhìn | Chưa test |
| **TC-UI-02** | Theme | F5 (Reload) lại trình duyệt sau khi đổi theme | Giao diện vẫn giữ nguyên Dark mode (lưu vào localStorage) | Chưa test |
| **TC-UI-03** | Đa ngôn ngữ | Đổi ngôn ngữ từ EN sang VI | Toàn bộ nhãn nút bấm, tiêu đề chuyển sang tiếng Việt | Chưa test |
| **TC-UI-04** | Đa ngôn ngữ | F5 lại trình duyệt sau khi đổi ngôn ngữ | Ngôn ngữ đã chọn vẫn được duy trì | Chưa test |
