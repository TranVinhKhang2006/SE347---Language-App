# Supabase

Schema database (bảng, trigger, RLS, storage) của dự án. Mỗi thay đổi schema là một file mới trong `migrations/`, đặt tên `YYYYMMDDHHMMSS_mo_ta.sql`, **không sửa file đã chạy**.

## Chạy migration

**Cách 1 — SQL Editor:** Supabase Dashboard → SQL Editor → New query → dán nội dung file → Run. Chạy các file theo thứ tự tên.

**Cách 2 — Supabase CLI:**

```bash
supabase link --project-ref <project-ref>
supabase db push
```

## Lưu ý cho backend C# (EF Core)

- Bảng trong `migrations/` do SQL quản lý. Entity trong `Models/` chỉ map vào bảng có sẵn, **không** tạo EF migration cho các bảng này.
- Backend kết nối bằng role `postgres` qua **Session pooler** nên **không bị RLS chặn** → mọi query phải tự lọc theo `user_id` lấy từ JWT và kiểm tra `profiles.status = 'active'`.
- Connection string và key để trong biến môi trường / user-secrets, không commit.

## Cấu hình key (SCRUM-12)

Danh sách biến nằm trong [`.env.example`](../.env.example). Lấy giá trị ở Supabase Dashboard:

| Biến | Lấy ở đâu | Ai được dùng |
|---|---|---|
| `Supabase:Url` | Project Settings → Data API → Project URL | Backend + frontend |
| `Supabase:PublishableKey` | Project Settings → API Keys → Publishable key (`sb_publishable_...`, bản cũ là `anon`) | Backend + frontend |
| `Supabase:SecretKey` | Project Settings → API Keys → Secret key (`sb_secret_...`, bản cũ là `service_role`) | **Chỉ backend** |
| `ConnectionStrings:Supabase` | Nút **Connect** → **Session pooler** → .NET, thay `[YOUR-PASSWORD]` | **Chỉ backend** |

Key và mật khẩu database xin lead backend gửi riêng (tin nhắn riêng), không dán lên Jira, group chat chung hay GitHub.

### Local: dotnet user-secrets

Secret được lưu ngoài thư mục repo (`~/.microsoft/usersecrets/`), không thể lỡ commit. Chạy ở thư mục gốc repo:

```bash
dotnet user-secrets set "Supabase:Url" "https://<project-ref>.supabase.co"
dotnet user-secrets set "Supabase:PublishableKey" "sb_publishable_..."
dotnet user-secrets set "Supabase:SecretKey" "sb_secret_..."
dotnet user-secrets set "ConnectionStrings:Supabase" "Host=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"
dotnet user-secrets list
```

Trong code đọc bằng `builder.Configuration["Supabase:Url"]` và `builder.Configuration.GetConnectionString("Supabase")`. User-secrets chỉ được nạp khi chạy môi trường Development (`dotnet run` / `dotnet watch` mặc định là Development).

### Render / Docker

Khai báo biến môi trường theo tên trong `.env.example` (dùng `__` thay cho `:`), ví dụ `Supabase__Url`, `ConnectionStrings__Supabase`.

### Lỡ commit key thì sao?

Xóa khỏi code chưa đủ vì key vẫn nằm trong lịch sử git. Báo lead ngay để **rotate**: tạo Secret key mới và xóa key cũ ở API Keys; đổi Database password ở Project Settings → Database.

## Migration hiện có

| File | Nội dung |
|---|---|
| `20261005090000_user_schema.sql` | `profiles` (1-1 với `auth.users`), `user_settings`, `user_learning_profiles`, `user_stats`, `user_daily_activity`, `achievements`, `user_achievements`, trigger tạo hồ sơ khi đăng ký, RLS, bucket `avatars` |
