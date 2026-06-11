# TODO - CSS login/register/forgot + banner Product

## Plan đã được chốt (style giữ nguyên layout, đồng bộ)

- [x] 1. Chuẩn hoá CSS cho Identity pages trong `wwwroot/css/login_logout.css` (login/register/forgot/reset) để đồng bộ spacing, nền, nút bấm, link quay lại, validation.
- [x] 2. Gộp style trùng từ `Forgot.css` vào `login_logout.css` để giữ 1 nguồn style.
- [x] 3. Chỉnh banner tại `Views/Product/Index.cshtml` bằng cách hoàn thiện CSS trong `wwwroot/css/productUser/index.css` (đảm bảo responsive/spacing hợp lý).
- [ ] 4. Chạy build + kiểm tra các route: Login/Register/ForgotPassword/ResetPassword và Product/Index.
