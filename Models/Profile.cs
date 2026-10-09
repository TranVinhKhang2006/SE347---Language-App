using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SE347.Models
{
    /// <summary>
    /// Hồ sơ người học; 1-1 với auth.users.
    /// Email/mật khẩu/session do Supabase Auth quản lý.
    /// Cấu hình Code-First: xem ApplicationDbContext.
    /// </summary>
    [Table("profiles")]
    public class Profile
    {
        /// <summary>Khóa chính, trùng auth.users.id (uuid).</summary>
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Tên đăng nhập duy nhất, 3–30 ký tự (a-z 0-9 . _).
        /// Constraint: uq_profiles_username, ck_profiles_username.
        /// </summary>
        [Required]
        [MaxLength(30)]
        [Column("username")]
        public string Username { get; set; } = null!;

        /// <summary>
        /// Tên hiển thị, 1–80 ký tự, đã trim.
        /// Constraint: ck_profiles_display_name.
        /// </summary>
        [Required]
        [MaxLength(80)]
        [Column("display_name")]
        public string DisplayName { get; set; } = null!;

        /// <summary>
        /// Path trong bucket avatars ({user_id}/...); không lưu signed URL.
        /// Nullable — null nếu chưa upload avatar.
        /// </summary>
        [MaxLength(512)]
        [Column("avatar_path")]
        public string? AvatarPath { get; set; }

        /// <summary>Bio ngắn, tối đa 160 ký tự. Nullable.</summary>
        [MaxLength(160)]
        [Column("bio")]
        public string? Bio { get; set; }

        /// <summary>
        /// Ngôn ngữ giao diện: 'vi' hoặc 'en'.
        /// Constraint: ck_profiles_locale.
        /// </summary>
        [Required]
        [MaxLength(10)]
        [Column("ui_locale")]
        public string UiLocale { get; set; } = "vi";

        /// <summary>
        /// Múi giờ IANA, mặc định 'Asia/Ho_Chi_Minh'.
        /// Constraint: ck_profiles_timezone.
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("timezone")]
        public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";

        /// <summary>
        /// Quyền xem hồ sơ: 'public' hoặc 'private'.
        /// Constraint: ck_profiles_visibility.
        /// </summary>
        [Required]
        [MaxLength(10)]
        [Column("profile_visibility")]
        public string ProfileVisibility { get; set; } = "private";

        /// <summary>
        /// Vai trò: 'learner' hoặc 'admin'.
        /// Constraint: ck_profiles_role.
        /// </summary>
        [Required]
        [MaxLength(16)]
        [Column("role")]
        public string Role { get; set; } = "learner";

        /// <summary>
        /// Trạng thái tài khoản: 'active' | 'suspended' | 'deactivated' | 'deleted'.
        /// Constraint: ck_profiles_status, ck_profiles_deletion, ck_profiles_deleted_private.
        /// </summary>
        [Required]
        [MaxLength(16)]
        [Column("status")]
        public string Status { get; set; } = "active";

        /// <summary>Thời điểm hoàn thành onboarding. Nullable.</summary>
        [Column("onboarding_completed_at")]
        public DateTimeOffset? OnboardingCompletedAt { get; set; }

        /// <summary>Thời điểm tạo, SaveChanges tự gán khi thêm mới.</summary>
        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>Thời điểm cập nhật cuối, SaveChanges tự gán.</summary>
        [Column("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>Thời điểm xoá mềm. Nullable — chỉ có khi status = 'deleted'.</summary>
        [Column("deleted_at")]
        public DateTimeOffset? DeletedAt { get; set; }

        /// <summary>
        /// Optimistic locking: tăng mỗi UPDATE bởi SaveChanges.
        /// Constraint: ck_profiles_version (>= 1).
        /// </summary>
        [ConcurrencyCheck]
        [Column("row_version")]
        public long RowVersion { get; set; } = 1;

        // ── Navigation properties ────────────────────────────────────

        /// <summary>Tùy chỉnh app (1-1).</summary>
        public UserSetting? Setting { get; set; }

        /// <summary>Trình độ & mục tiêu học (1-1).</summary>
        public UserLearningProfile? LearningProfile { get; set; }

        /// <summary>Tổng XP & chuỗi ngày học (1-1).</summary>
        public UserStat? Stat { get; set; }

        /// <summary>Lịch sử hoạt động theo ngày (1-N).</summary>
        public ICollection<UserDailyActivity> DailyActivities { get; set; } = new List<UserDailyActivity>();

        /// <summary>Huy hiệu đã nhận (1-N).</summary>
        public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    }
}
