using System.ComponentModel.DataAnnotations.Schema;

namespace SE347.Models
{
    /// <summary>
    /// Hoạt động theo ngày — mỗi ngày học 1 dòng.
    /// Dùng cho lịch streak, XP tuần, bảng xếp hạng.
    /// Composite PK: (user_id, activity_date).
    /// </summary>
    [Table("user_daily_activity")]
    public class UserDailyActivity
    {
        /// <summary>FK → profiles.id.</summary>
        [Column("user_id")]
        public Guid UserId { get; set; }

        /// <summary>Ngày hoạt động.</summary>
        [Column("activity_date")]
        public DateOnly ActivityDate { get; set; }

        /// <summary>
        /// XP kiếm được trong ngày.
        /// Constraint: ck_daily_non_negative (>= 0).
        /// </summary>
        [Column("xp_earned")]
        public int XpEarned { get; set; }

        /// <summary>
        /// Số bài học hoàn thành trong ngày.
        /// Constraint: ck_daily_non_negative (>= 0).
        /// </summary>
        [Column("lessons_completed")]
        public int LessonsCompleted { get; set; }

        /// <summary>
        /// Số từ vựng học được trong ngày.
        /// Constraint: ck_daily_non_negative (>= 0).
        /// </summary>
        [Column("words_learned")]
        public int WordsLearned { get; set; }

        /// <summary>
        /// Tổng giây luyện tập trong ngày.
        /// Constraint: ck_daily_non_negative (>= 0).
        /// </summary>
        [Column("practice_seconds")]
        public int PracticeSeconds { get; set; }

        /// <summary>Có dùng streak freeze trong ngày này không.</summary>
        [Column("streak_freeze_used")]
        public bool StreakFreezeUsed { get; set; }

        // ── Navigation property ──────────────────────────────────────

        /// <summary>Hồ sơ người dùng.</summary>
        [ForeignKey(nameof(UserId))]
        public Profile Profile { get; set; } = null!;
    }
}
