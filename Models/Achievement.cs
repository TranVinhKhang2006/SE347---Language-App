using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SE347.Models
{
    /// <summary>
    /// Danh sách huy hiệu (seed data).
    /// PK là code (varchar), không dùng auto-increment.
    /// </summary>
    [Table("achievements")]
    public class Achievement
    {
        /// <summary>
        /// Mã huy hiệu duy nhất, ví dụ 'streak_7', 'xp_1000'.
        /// </summary>
        [Key]
        [MaxLength(50)]
        [Column("code")]
        public string Code { get; set; } = null!;

        /// <summary>
        /// Phân loại: 'streak' | 'xp' | 'lesson' | 'vocab' | 'quiz' | 'exam'.
        /// Constraint: ck_achievements_category.
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Column("category")]
        public string Category { get; set; } = null!;

        /// <summary>
        /// Ngưỡng đạt huy hiệu (> 0).
        /// Constraint: ck_achievements_threshold.
        /// </summary>
        [Column("threshold")]
        public int Threshold { get; set; }

        /// <summary>Tên huy hiệu tiếng Việt.</summary>
        [Required]
        [MaxLength(80)]
        [Column("name_vi")]
        public string NameVi { get; set; } = null!;

        /// <summary>Tên huy hiệu tiếng Anh.</summary>
        [Required]
        [MaxLength(80)]
        [Column("name_en")]
        public string NameEn { get; set; } = null!;

        /// <summary>Mô tả tiếng Việt.</summary>
        [Required]
        [MaxLength(200)]
        [Column("description_vi")]
        public string DescriptionVi { get; set; } = null!;

        /// <summary>Mô tả tiếng Anh.</summary>
        [Required]
        [MaxLength(200)]
        [Column("description_en")]
        public string DescriptionEn { get; set; } = null!;

        /// <summary>Path icon huy hiệu. Nullable.</summary>
        [MaxLength(255)]
        [Column("icon_path")]
        public string? IconPath { get; set; }

        /// <summary>
        /// Phần thưởng XP khi nhận huy hiệu (>= 0).
        /// Constraint: ck_achievements_reward.
        /// </summary>
        [Column("xp_reward")]
        public int XpReward { get; set; }

        /// <summary>Thứ tự sắp xếp hiển thị.</summary>
        [Column("sort_order")]
        public short SortOrder { get; set; }

        // ── Navigation property ──────────────────────────────────────

        /// <summary>Danh sách user đã nhận huy hiệu này.</summary>
        public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    }
}
