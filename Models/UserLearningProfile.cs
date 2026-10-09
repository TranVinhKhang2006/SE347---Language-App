using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SE347.Models
{
    /// <summary>
    /// Trình độ & mục tiêu học (onboarding + bài kiểm tra đầu vào).
    /// 1-1 với profiles qua user_id.
    /// </summary>
    [Table("user_learning_profiles")]
    public class UserLearningProfile
    {
        /// <summary>Khóa chính, đồng thời là FK → profiles.id.</summary>
        [Key]
        [Column("user_id")]
        public Guid UserId { get; set; }

        /// <summary>
        /// Lý do học, chọn ở màn hình lần đầu.
        /// Nullable — null nếu chưa chọn.
        /// Constraint: ck_learning_reason ('travel'|'work'|'school'|'exam'|'fun'|'other').
        /// </summary>
        [MaxLength(20)]
        [Column("learning_reason")]
        public string? LearningReason { get; set; }

        /// <summary>
        /// Trình độ CEFR do bài kiểm tra đầu vào xếp.
        /// Nullable — null nếu chưa làm bài kiểm tra.
        /// Constraint: ck_learning_cefr ('A1'|'A2'|'B1'|'B2'|'C1').
        /// </summary>
        [MaxLength(2)]
        [Column("cefr_level")]
        public string? CefrLevel { get; set; }

        /// <summary>
        /// Thời điểm hoàn thành bài kiểm tra xếp lớp.
        /// Nullable — null nếu chưa làm (ck_learning_placement: phải có cefr_level).
        /// </summary>
        [Column("placement_completed_at")]
        public DateTimeOffset? PlacementCompletedAt { get; set; }

        /// <summary>Thời điểm cập nhật cuối, SaveChanges tự gán.</summary>
        [Column("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        // ── Navigation property ──────────────────────────────────────

        /// <summary>Hồ sơ người dùng.</summary>
        [ForeignKey(nameof(UserId))]
        public Profile Profile { get; set; } = null!;
    }
}
