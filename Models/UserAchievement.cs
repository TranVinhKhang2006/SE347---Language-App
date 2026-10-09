using System.ComponentModel.DataAnnotations.Schema;

namespace SE347.Models
{
    /// <summary>
    /// Bảng trung gian: user đã nhận huy hiệu nào, lúc nào.
    /// Composite PK: (user_id, achievement_code).
    /// </summary>
    [Table("user_achievements")]
    public class UserAchievement
    {
        /// <summary>FK → profiles.id.</summary>
        [Column("user_id")]
        public Guid UserId { get; set; }

        /// <summary>FK → achievements.code.</summary>
        [Column("achievement_code")]
        public string AchievementCode { get; set; } = null!;

        /// <summary>Thời điểm nhận huy hiệu.</summary>
        [Column("awarded_at")]
        public DateTimeOffset AwardedAt { get; set; }

        // ── Navigation properties ────────────────────────────────────

        /// <summary>Hồ sơ người dùng.</summary>
        [ForeignKey(nameof(UserId))]
        public Profile Profile { get; set; } = null!;

        /// <summary>Huy hiệu.</summary>
        [ForeignKey(nameof(AchievementCode))]
        public Achievement Achievement { get; set; } = null!;
    }
}
