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

## Migration hiện có

| File | Nội dung |
|---|---|
| `20261005090000_user_schema.sql` | `profiles` (1-1 với `auth.users`), `user_settings`, `user_learning_profiles`, `user_stats`, `user_daily_activity`, `achievements`, `user_achievements`, trigger tạo hồ sơ khi đăng ký, RLS, bucket `avatars` |
