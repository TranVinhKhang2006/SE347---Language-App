using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SE347.Models
{
    /// <summary>
    /// Tùy chỉnh app: theme, mục tiêu ngày, nhắc học.
    /// 1-1 với profiles qua user_id.
    /// </summary>
    [Table("user_settings")]
    public class UserSetting
    {
        /// <summary>Khóa chính, đồng thời là FK → profiles.id.</summary>
        [Key]
        [Column("user_id")]
        public Guid UserId { get; set; }

        /// <summary>
        /// Giao diện: 'light' | 'dark' | 'system'.
        /// Constraint: ck_settings_theme.
        /// </summary>
        [Required]
        [MaxLength(16)]
        [Column("theme")]
        public string Theme { get; set; } = "system";

        /// <summary>
        /// Mục tiêu XP hàng ngày: 10 | 20 | 30 | 50.
        /// Constraint: ck_settings_daily_goal.
        /// </summary>
        [Column("daily_goal_xp")]
        public short DailyGoalXp { get; set; } = 20;

        /// <summary>Bật/tắt âm thanh.</summary>
        [Column("sound_enabled")]
        public bool SoundEnabled { get; set; } = true;

        /// <summary>Bật/tắt nhắc học.</summary>
        [Column("reminder_enabled")]
        public bool ReminderEnabled { get; set; }

        /// <summary>
        /// Giờ nhắc học (giờ địa phương theo profiles.timezone).
        /// Nullable — bắt buộc khi reminder_enabled = true (ck_settings_reminder).
        /// </summary>
        [Column("reminder_time")]
        public TimeOnly? ReminderTime { get; set; }

        /// <summary>Thời điểm cập nhật cuối, SaveChanges tự gán.</summary>
        [Column("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        // ── Navigation property ──────────────────────────────────────

        /// <summary>Hồ sơ người dùng sở hữu setting này.</summary>
        [ForeignKey(nameof(UserId))]
        public Profile Profile { get; set; } = null!;
    }
}
