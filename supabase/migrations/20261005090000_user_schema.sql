-- =====================================================================
-- User schema cho app học tiếng Anh (kiểu Duolingo) trên Supabase
-- Jira: SCRUM-6 (SCRUM-10, SCRUM-11)
--
-- Dựa trên "Đặc tả bảng users", điều chỉnh cho Supabase Auth:
--   * Email, mật khẩu, xác thực email, session, OAuth, 2FA nằm ở auth.users
--     (Supabase quản lý) -> KHÔNG lưu lại email/password/last_login ở đây.
--   * public.profiles.id = auth.users.id, tự tạo bằng trigger khi đăng ký.
--   * Thêm các bảng cho trải nghiệm kiểu Duolingo: settings, hồ sơ học,
--     XP/streak, hoạt động theo ngày, huy hiệu.
--   * Frontend (supabase-js) chỉ đọc dữ liệu của chính mình và sửa một số cột
--     được cho phép. XP/streak/huy hiệu chỉ backend C# ghi (role postgres,
--     không bị RLS chặn -> backend PHẢI tự kiểm tra quyền).
--
-- EF Core (Data/, Models/): các bảng ở đây do file SQL này quản lý.
-- Entity C# chỉ map vào bảng có sẵn, KHÔNG sinh EF migration cho chúng
-- (EF không tạo được trigger trên auth.users, RLS, storage policy).
--
-- Chạy 1 lần trong Supabase Dashboard -> SQL Editor (hoặc `supabase db push`).
-- =====================================================================

begin;

-- ---------------------------------------------------------------------
-- 0. Hàm tiện ích
-- ---------------------------------------------------------------------

-- Quy tắc username: 3–30 ký tự, a-z 0-9 . _ ; đầu/cuối là chữ hoặc số;
-- không có ".." "__" "._" liên tiếp; không trùng tên đặt trước.
create or replace function public.is_valid_username(p text)
returns boolean
language sql
immutable
set search_path = ''
as $$
  select coalesce(
    p ~ '^[a-z0-9][a-z0-9._]{1,28}[a-z0-9]$'
    and p !~ '[._]{2}'
    and p <> all (array[
      'admin', 'administrator', 'support', 'system', 'root',
      'moderator', 'api', 'null', 'undefined', 'me', 'help'
    ]),
    false
  );
$$;

-- Cập nhật updated_at cho các bảng phụ.
create or replace function public.set_updated_at()
returns trigger
language plpgsql
set search_path = ''
as $$
begin
  new.updated_at := statement_timestamp();
  return new;
end;
$$;

-- ---------------------------------------------------------------------
-- 1. profiles — định danh & hồ sơ công khai của người học
-- ---------------------------------------------------------------------
create table public.profiles (
  id                      uuid primary key
                          references auth.users (id) on delete cascade,
  username                varchar(30)  not null,
  display_name            varchar(80)  not null,
  avatar_path             varchar(512),             -- path trong bucket "avatars", không lưu URL
  bio                     varchar(160),
  ui_locale               varchar(10)  not null default 'vi',
  timezone                varchar(64)  not null default 'Asia/Ho_Chi_Minh',
  profile_visibility      varchar(10)  not null default 'private',
  role                    varchar(16)  not null default 'learner',
  status                  varchar(16)  not null default 'active',
  onboarding_completed_at timestamptz,
  created_at              timestamptz  not null default now(),
  updated_at              timestamptz  not null default now(),
  deleted_at              timestamptz,
  row_version             bigint       not null default 1,

  constraint uq_profiles_username unique (username),
  constraint ck_profiles_username check (
    username ~ '^[a-z0-9][a-z0-9._]{1,28}[a-z0-9]$' and username !~ '[._]{2}'
  ),
  constraint ck_profiles_display_name check (
    display_name = btrim(display_name) and char_length(display_name) between 1 and 80
  ),
  constraint ck_profiles_avatar_path check (
    avatar_path is null or (avatar_path = btrim(avatar_path) and char_length(avatar_path) > 0)
  ),
  constraint ck_profiles_bio check (
    bio is null or (bio = btrim(bio) and char_length(bio) > 0)
  ),
  constraint ck_profiles_locale     check (ui_locale in ('vi', 'en')),
  constraint ck_profiles_timezone   check (timezone = btrim(timezone) and char_length(timezone) > 0),
  constraint ck_profiles_visibility check (profile_visibility in ('public', 'private')),
  constraint ck_profiles_role       check (role in ('learner', 'admin')),
  constraint ck_profiles_status     check (status in ('active', 'suspended', 'deactivated', 'deleted')),
  constraint ck_profiles_deletion   check (
    (status = 'deleted' and deleted_at is not null)
    or (status <> 'deleted' and deleted_at is null)
  ),
  constraint ck_profiles_deleted_private check (status <> 'deleted' or profile_visibility = 'private'),
  constraint ck_profiles_version    check (row_version >= 1),
  constraint ck_profiles_timestamps check (updated_at >= created_at)
);

create index idx_profiles_deleted_at on public.profiles (deleted_at) where deleted_at is not null;

comment on table  public.profiles is 'Hồ sơ người học; 1-1 với auth.users. Email/mật khẩu/session do Supabase Auth quản lý.';
comment on column public.profiles.avatar_path is 'Path trong bucket avatars ({user_id}/...); không lưu signed URL';
comment on column public.profiles.row_version is 'Tăng mỗi UPDATE, dùng optimistic locking (expected_version)';

-- Bảo vệ id/created_at, tự set updated_at và row_version.
create or replace function public.profiles_before_write()
returns trigger
language plpgsql
set search_path = ''
as $$
begin
  if tg_op = 'INSERT' then
    new.created_at  := statement_timestamp();
    new.updated_at  := new.created_at;
    new.row_version := 1;
  else
    if new.id is distinct from old.id or new.created_at is distinct from old.created_at then
      raise exception 'immutable profile identity or creation time' using errcode = '23514';
    end if;
    new.updated_at  := greatest(statement_timestamp(), old.updated_at, old.created_at);
    new.row_version := old.row_version + 1;
  end if;
  return new;
end;
$$;

create trigger trg_profiles_before_write
before insert or update on public.profiles
for each row execute function public.profiles_before_write();

-- ---------------------------------------------------------------------
-- 2. user_settings — tùy chỉnh app (theme, mục tiêu ngày, nhắc học)
-- ---------------------------------------------------------------------
create table public.user_settings (
  user_id           uuid primary key references public.profiles (id) on delete cascade,
  theme             varchar(16) not null default 'system',
  daily_goal_xp     smallint    not null default 20,
  sound_enabled     boolean     not null default true,
  reminder_enabled  boolean     not null default false,
  reminder_time     time,                                -- giờ địa phương theo profiles.timezone
  updated_at        timestamptz not null default now(),

  constraint ck_settings_theme      check (theme in ('light', 'dark', 'system')),
  constraint ck_settings_daily_goal check (daily_goal_xp in (10, 20, 30, 50)),
  constraint ck_settings_reminder   check (not reminder_enabled or reminder_time is not null)
);

create trigger trg_user_settings_updated_at
before update on public.user_settings
for each row execute function public.set_updated_at();

-- ---------------------------------------------------------------------
-- 3. user_learning_profiles — trình độ & mục tiêu học (onboarding + đầu vào)
-- ---------------------------------------------------------------------
create table public.user_learning_profiles (
  user_id                 uuid primary key references public.profiles (id) on delete cascade,
  learning_reason         varchar(20),                   -- chọn ở màn hình lần đầu
  cefr_level              varchar(2),                    -- do bài kiểm tra đầu vào xếp
  placement_completed_at  timestamptz,
  updated_at              timestamptz not null default now(),

  constraint ck_learning_reason check (
    learning_reason is null or learning_reason in ('travel', 'work', 'school', 'exam', 'fun', 'other')
  ),
  constraint ck_learning_cefr check (
    cefr_level is null or cefr_level in ('A1', 'A2', 'B1', 'B2', 'C1')
  ),
  constraint ck_learning_placement check (
    placement_completed_at is null or cefr_level is not null
  )
);

create trigger trg_user_learning_profiles_updated_at
before update on public.user_learning_profiles
for each row execute function public.set_updated_at();

-- ---------------------------------------------------------------------
-- 4. user_stats — tổng XP & chuỗi ngày học (backend C# cập nhật)
-- ---------------------------------------------------------------------
create table public.user_stats (
  user_id             uuid primary key references public.profiles (id) on delete cascade,
  total_xp            bigint   not null default 0,
  current_streak      integer  not null default 0,
  longest_streak      integer  not null default 0,
  last_activity_date  date,                               -- theo timezone của user
  streak_freezes      smallint not null default 0,
  updated_at          timestamptz not null default now(),

  constraint ck_stats_xp      check (total_xp >= 0),
  constraint ck_stats_streak  check (current_streak >= 0 and longest_streak >= current_streak),
  constraint ck_stats_freezes check (streak_freezes between 0 and 2)
);

create trigger trg_user_stats_updated_at
before update on public.user_stats
for each row execute function public.set_updated_at();

-- ---------------------------------------------------------------------
-- 5. user_daily_activity — mỗi ngày học 1 dòng (lịch streak, XP tuần, leaderboard)
-- ---------------------------------------------------------------------
create table public.user_daily_activity (
  user_id            uuid     not null references public.profiles (id) on delete cascade,
  activity_date      date     not null,
  xp_earned          integer  not null default 0,
  lessons_completed  integer  not null default 0,
  words_learned      integer  not null default 0,
  practice_seconds   integer  not null default 0,
  streak_freeze_used boolean  not null default false,

  primary key (user_id, activity_date),
  constraint ck_daily_non_negative check (
    xp_earned >= 0 and lessons_completed >= 0 and words_learned >= 0 and practice_seconds >= 0
  )
);

-- Bảng xếp hạng theo tuần: lọc theo khoảng ngày rồi group by user.
create index idx_daily_activity_date on public.user_daily_activity (activity_date);

-- ---------------------------------------------------------------------
-- 6. achievements + user_achievements — huy hiệu
-- ---------------------------------------------------------------------
create table public.achievements (
  code            varchar(50) primary key,
  category        varchar(20) not null,
  threshold       integer     not null,
  name_vi         varchar(80) not null,
  name_en         varchar(80) not null,
  description_vi  varchar(200) not null,
  description_en  varchar(200) not null,
  icon_path       varchar(255),
  xp_reward       integer     not null default 0,
  sort_order      smallint    not null default 0,

  constraint ck_achievements_category check (category in ('streak', 'xp', 'lesson', 'vocab', 'quiz', 'exam')),
  constraint ck_achievements_threshold check (threshold > 0),
  constraint ck_achievements_reward check (xp_reward >= 0)
);

create table public.user_achievements (
  user_id          uuid        not null references public.profiles (id) on delete cascade,
  achievement_code varchar(50) not null references public.achievements (code) on update cascade,
  awarded_at       timestamptz not null default now(),
  primary key (user_id, achievement_code)
);

insert into public.achievements
  (code, category, threshold, name_vi, name_en, description_vi, description_en, xp_reward, sort_order)
values
  ('first_lesson', 'lesson', 1,    'Bước đầu tiên',  'First Step',       'Hoàn thành bài học đầu tiên', 'Complete your first lesson',  10, 1),
  ('streak_3',     'streak', 3,    'Khởi động',      'Warming Up',       'Học 3 ngày liên tiếp',        'Reach a 3-day streak',        10, 2),
  ('streak_7',     'streak', 7,    'Một tuần',       'One Week',         'Học 7 ngày liên tiếp',        'Reach a 7-day streak',        20, 3),
  ('streak_30',    'streak', 30,   'Bền bỉ',         'Unstoppable',      'Học 30 ngày liên tiếp',       'Reach a 30-day streak',       50, 4),
  ('xp_100',       'xp',     100,  'Tập sự',         'Apprentice',       'Đạt 100 XP',                  'Earn 100 XP',                 0,  5),
  ('xp_1000',      'xp',     1000, 'Chăm chỉ',       'Hard Worker',      'Đạt 1.000 XP',                'Earn 1,000 XP',               0,  6),
  ('words_100',    'vocab',  100,  '100 từ đầu tiên','First 100 Words',  'Học 100 từ vựng',             'Learn 100 words',             20, 7),
  ('words_500',    'vocab',  500,  'Kho từ vựng',    'Word Collector',   'Học 500 từ vựng',             'Learn 500 words',             50, 8),
  ('perfect_quiz', 'quiz',   1,    'Không sai câu nào', 'Flawless',      'Làm đúng 100% một bài quiz',  'Score 100% on a quiz',        10, 9),
  ('first_exam',   'exam',   1,    'Thí sinh',       'Test Taker',       'Hoàn thành bài thi thử đầu tiên', 'Finish your first mock exam', 20, 10);

-- ---------------------------------------------------------------------
-- 7. Tự tạo hồ sơ khi user đăng ký (Supabase Auth)
--    Form Pages/Account/Signup.cshtml gửi fullName, email, password:
--      supabase.auth.signUp({ email, password,
--        options: { data: { full_name: fullName, ui_locale: 'vi' } } })
--    (có thể gửi thêm username nếu sau này form có ô username)
--    Không có username hợp lệ -> sinh từ phần trước @ của email
--    (danh.nguyen@gmail.com -> danh_nguyen, trùng thì thêm _1234),
--    cuối cùng mới dùng "u" + 12 ký tự hex. Không bao giờ làm hỏng đăng ký.
-- ---------------------------------------------------------------------

-- Gợi ý username từ email: chỉ giữ a-z 0-9, ký tự khác thành "_".
create or replace function public.username_from_email(p_email text)
returns text
language sql
immutable
set search_path = ''
as $$
  select regexp_replace(
           left(
             regexp_replace(
               regexp_replace(lower(split_part(coalesce(p_email, ''), '@', 1)), '[^a-z0-9]+', '_', 'g'),
               '^_+|_+$', '', 'g'),
             24),
           '_+$', '');
$$;

create or replace function public.handle_new_user()
returns trigger
language plpgsql
security definer
set search_path = ''
as $$
declare
  v_meta     jsonb := coalesce(new.raw_user_meta_data, '{}'::jsonb);
  v_username text  := lower(btrim(coalesce(v_meta ->> 'username', '')));
  v_base     text  := public.username_from_email(new.email);
  v_display  text  := btrim(coalesce(
                        nullif(btrim(v_meta ->> 'display_name'), ''),
                        nullif(btrim(v_meta ->> 'full_name'), ''),
                        nullif(btrim(v_meta ->> 'fullName'), ''),
                        split_part(coalesce(new.email, ''), '@', 1)
                      ));
  v_locale   text  := coalesce(v_meta ->> 'ui_locale', 'vi');
begin
  if not public.is_valid_username(v_username)
     or exists (select 1 from public.profiles where username = v_username) then
    v_username := v_base;
    for i in 1..5 loop
      exit when public.is_valid_username(v_username)
            and not exists (select 1 from public.profiles where username = v_username);
      v_username := v_base || '_' || lpad(floor(random() * 10000)::int::text, 4, '0');
    end loop;
    if not public.is_valid_username(v_username)
       or exists (select 1 from public.profiles where username = v_username) then
      v_username := 'u' || substr(replace(gen_random_uuid()::text, '-', ''), 1, 12);
    end if;
  end if;

  v_display := btrim(left(v_display, 80));
  if v_display = '' then
    v_display := v_username;
  end if;

  if v_locale not in ('vi', 'en') then
    v_locale := 'vi';
  end if;

  insert into public.profiles (id, username, display_name, ui_locale)
  values (new.id, v_username, v_display, v_locale);

  insert into public.user_settings (user_id)          values (new.id);
  insert into public.user_learning_profiles (user_id) values (new.id);
  insert into public.user_stats (user_id)             values (new.id);

  return new;
end;
$$;

create trigger on_auth_user_created
after insert on auth.users
for each row execute function public.handle_new_user();

-- Tạo hồ sơ cho các user đã đăng ký trước khi chạy migration này (nếu có).
insert into public.profiles (id, username, display_name)
select u.id,
       'u' || substr(replace(gen_random_uuid()::text, '-', ''), 1, 12),
       coalesce(nullif(btrim(split_part(coalesce(u.email, ''), '@', 1)), ''), 'learner')
from auth.users u
where not exists (select 1 from public.profiles p where p.id = u.id);

insert into public.user_settings (user_id)          select id from public.profiles on conflict do nothing;
insert into public.user_learning_profiles (user_id) select id from public.profiles on conflict do nothing;
insert into public.user_stats (user_id)             select id from public.profiles on conflict do nothing;

-- Kiểm tra username trước khi đăng ký/đổi username (gọi từ frontend: supabase.rpc('is_username_available', { p_username })).
create or replace function public.is_username_available(p_username text)
returns boolean
language sql
stable
security definer
set search_path = ''
as $$
  select public.is_valid_username(lower(btrim(coalesce(p_username, ''))))
     and not exists (
       select 1 from public.profiles where username = lower(btrim(coalesce(p_username, '')))
     );
$$;

-- ---------------------------------------------------------------------
-- 8. Quyền (Data API) + RLS
--    Project tắt "Automatically expose new tables" nên phải grant thủ công.
--    Frontend: chỉ đọc dữ liệu của mình, sửa một số cột cho phép.
--    Backend C# (role postgres) ghi XP/streak/huy hiệu và đọc hồ sơ public.
-- ---------------------------------------------------------------------
alter table public.profiles               enable row level security;
alter table public.user_settings          enable row level security;
alter table public.user_learning_profiles enable row level security;
alter table public.user_stats             enable row level security;
alter table public.user_daily_activity    enable row level security;
alter table public.achievements           enable row level security;
alter table public.user_achievements      enable row level security;

revoke all on public.profiles, public.user_settings, public.user_learning_profiles,
              public.user_stats, public.user_daily_activity, public.achievements,
              public.user_achievements
  from anon, authenticated;

grant select on public.profiles, public.user_settings, public.user_learning_profiles,
                public.user_stats, public.user_daily_activity, public.achievements,
                public.user_achievements
  to authenticated;

-- Cột user được tự sửa. KHÔNG có role, status, deleted_at, row_version.
grant update (username, display_name, avatar_path, bio, ui_locale, timezone,
              profile_visibility, onboarding_completed_at)
  on public.profiles to authenticated;
grant update (theme, daily_goal_xp, sound_enabled, reminder_enabled, reminder_time)
  on public.user_settings to authenticated;
grant update (learning_reason)
  on public.user_learning_profiles to authenticated;

-- profiles
create policy "profiles: read own" on public.profiles
  for select to authenticated
  using ((select auth.uid()) = id);

create policy "profiles: update own when active" on public.profiles
  for update to authenticated
  using ((select auth.uid()) = id and status = 'active')
  with check ((select auth.uid()) = id);

-- user_settings
create policy "settings: read own" on public.user_settings
  for select to authenticated
  using ((select auth.uid()) = user_id);

create policy "settings: update own" on public.user_settings
  for update to authenticated
  using ((select auth.uid()) = user_id)
  with check ((select auth.uid()) = user_id);

-- user_learning_profiles
create policy "learning: read own" on public.user_learning_profiles
  for select to authenticated
  using ((select auth.uid()) = user_id);

create policy "learning: update own" on public.user_learning_profiles
  for update to authenticated
  using ((select auth.uid()) = user_id)
  with check ((select auth.uid()) = user_id);

-- Chỉ đọc (backend ghi)
create policy "stats: read own" on public.user_stats
  for select to authenticated
  using ((select auth.uid()) = user_id);

create policy "daily: read own" on public.user_daily_activity
  for select to authenticated
  using ((select auth.uid()) = user_id);

create policy "user_achievements: read own" on public.user_achievements
  for select to authenticated
  using ((select auth.uid()) = user_id);

create policy "achievements: read all" on public.achievements
  for select to authenticated
  using (true);

-- Hàm: chỉ is_username_available được gọi từ client.
revoke execute on function public.handle_new_user()                from public, anon, authenticated;
revoke execute on function public.username_from_email(text)        from public, anon, authenticated;
revoke execute on function public.is_username_available(text)      from public;
grant  execute on function public.is_username_available(text)      to anon, authenticated;

-- ---------------------------------------------------------------------
-- 9. Storage: bucket avatars (ảnh ≤ 2MB, user chỉ ghi vào thư mục {user_id}/)
-- ---------------------------------------------------------------------
insert into storage.buckets (id, name, public, file_size_limit, allowed_mime_types)
values ('avatars', 'avatars', true, 2097152, array['image/png', 'image/jpeg', 'image/webp'])
on conflict (id) do nothing;

create policy "avatars: read own" on storage.objects
  for select to authenticated
  using (bucket_id = 'avatars' and (storage.foldername(name))[1] = (select auth.uid())::text);

create policy "avatars: upload own" on storage.objects
  for insert to authenticated
  with check (bucket_id = 'avatars' and (storage.foldername(name))[1] = (select auth.uid())::text);

create policy "avatars: update own" on storage.objects
  for update to authenticated
  using (bucket_id = 'avatars' and (storage.foldername(name))[1] = (select auth.uid())::text);

create policy "avatars: delete own" on storage.objects
  for delete to authenticated
  using (bucket_id = 'avatars' and (storage.foldername(name))[1] = (select auth.uid())::text);

commit;
