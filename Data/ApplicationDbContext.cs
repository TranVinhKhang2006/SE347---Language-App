using Microsoft.EntityFrameworkCore;
using SE347.Models;

namespace SE347.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ── DbSets ──────────────────────────────────────────────────

        public DbSet<Profile> Profiles { get; set; } = null!;
        public DbSet<UserSetting> UserSettings { get; set; } = null!;
        public DbSet<UserLearningProfile> UserLearningProfiles { get; set; } = null!;
        public DbSet<UserStat> UserStats { get; set; } = null!;
        public DbSet<UserDailyActivity> UserDailyActivities { get; set; } = null!;
        public DbSet<Achievement> Achievements { get; set; } = null!;
        public DbSet<UserAchievement> UserAchievements { get; set; } = null!;

        // =================================================================
        // OnModelCreating — Fluent API toàn bộ cấu hình Code-First
        // =================================================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tất cả bảng nằm trong schema "public" (mặc định PostgreSQL).
            modelBuilder.HasDefaultSchema("public");

            ConfigureProfile(modelBuilder);
            ConfigureUserSetting(modelBuilder);
            ConfigureUserLearningProfile(modelBuilder);
            ConfigureUserStat(modelBuilder);
            ConfigureUserDailyActivity(modelBuilder);
            ConfigureAchievement(modelBuilder);
            ConfigureUserAchievement(modelBuilder);
            SeedAchievements(modelBuilder);
        }

        // ─────────────────────────────────────────────────────────────
        // 1. profiles
        // ─────────────────────────────────────────────────────────────
        private static void ConfigureProfile(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Profile>(entity =>
            {
                // ── Table & check constraints ────────────────────────
                entity.ToTable("profiles", tb =>
                {
                    tb.HasCheckConstraint("ck_profiles_username",
                        "username ~ '^[a-z0-9][a-z0-9._]{1,28}[a-z0-9]$' AND username !~ '[._]{2}'");

                    tb.HasCheckConstraint("ck_profiles_display_name",
                        "display_name = btrim(display_name) AND char_length(display_name) BETWEEN 1 AND 80");

                    tb.HasCheckConstraint("ck_profiles_avatar_path",
                        "avatar_path IS NULL OR (avatar_path = btrim(avatar_path) AND char_length(avatar_path) > 0)");

                    tb.HasCheckConstraint("ck_profiles_bio",
                        "bio IS NULL OR (bio = btrim(bio) AND char_length(bio) > 0)");

                    tb.HasCheckConstraint("ck_profiles_locale",
                        "ui_locale IN ('vi', 'en')");

                    tb.HasCheckConstraint("ck_profiles_timezone",
                        "timezone = btrim(timezone) AND char_length(timezone) > 0");

                    tb.HasCheckConstraint("ck_profiles_visibility",
                        "profile_visibility IN ('public', 'private')");

                    tb.HasCheckConstraint("ck_profiles_role",
                        "role IN ('learner', 'admin')");

                    tb.HasCheckConstraint("ck_profiles_status",
                        "status IN ('active', 'suspended', 'deactivated', 'deleted')");

                    tb.HasCheckConstraint("ck_profiles_deletion",
                        "(status = 'deleted' AND deleted_at IS NOT NULL) OR (status <> 'deleted' AND deleted_at IS NULL)");

                    tb.HasCheckConstraint("ck_profiles_deleted_private",
                        "status <> 'deleted' OR profile_visibility = 'private'");

                    tb.HasCheckConstraint("ck_profiles_version",
                        "row_version >= 1");

                    tb.HasCheckConstraint("ck_profiles_timestamps",
                        "updated_at >= created_at");

                    tb.HasComment("Hồ sơ người học; 1-1 với auth.users. Email/mật khẩu/session do Supabase Auth quản lý.");
                });


                // ── Key & indexes ────────────────────────────────────
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Username)
                      .IsUnique()
                      .HasDatabaseName("uq_profiles_username");

                entity.HasIndex(e => e.DeletedAt)
                      .HasDatabaseName("idx_profiles_deleted_at")
                      .HasFilter("deleted_at IS NOT NULL");

                // ── Columns — default values ─────────────────────────
                entity.Property(e => e.UiLocale)
                      .HasDefaultValue("vi");

                entity.Property(e => e.Timezone)
                      .HasDefaultValue("Asia/Ho_Chi_Minh");

                entity.Property(e => e.ProfileVisibility)
                      .HasDefaultValue("private");

                entity.Property(e => e.Role)
                      .HasDefaultValue("learner");

                entity.Property(e => e.Status)
                      .HasDefaultValue("active");

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("now()");

                entity.Property(e => e.UpdatedAt)
                      .HasDefaultValueSql("now()");

                entity.Property(e => e.RowVersion)
                      .HasDefaultValue(1L)
                      .IsConcurrencyToken();

                // ── Column comments ──────────────────────────────────
                entity.Property(e => e.AvatarPath)
                      .HasComment("Path trong bucket avatars ({user_id}/...); không lưu signed URL.");

                entity.Property(e => e.RowVersion)
                      .HasComment("Tăng mỗi UPDATE, dùng optimistic locking.");

                // ── Relationships ────────────────────────────────────
                entity.HasOne(e => e.Setting)
                      .WithOne(s => s.Profile)
                      .HasForeignKey<UserSetting>(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.LearningProfile)
                      .WithOne(lp => lp.Profile)
                      .HasForeignKey<UserLearningProfile>(lp => lp.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Stat)
                      .WithOne(s => s.Profile)
                      .HasForeignKey<UserStat>(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.DailyActivities)
                      .WithOne(da => da.Profile)
                      .HasForeignKey(da => da.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.UserAchievements)
                      .WithOne(ua => ua.Profile)
                      .HasForeignKey(ua => ua.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        // ─────────────────────────────────────────────────────────────
        // 2. user_settings
        // ─────────────────────────────────────────────────────────────
        private static void ConfigureUserSetting(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserSetting>(entity =>
            {
                entity.ToTable("user_settings", tb =>
                {
                    tb.HasCheckConstraint("ck_settings_theme",
                        "theme IN ('light', 'dark', 'system')");

                    tb.HasCheckConstraint("ck_settings_daily_goal",
                        "daily_goal_xp IN (10, 20, 30, 50)");

                    tb.HasCheckConstraint("ck_settings_reminder",
                        "NOT reminder_enabled OR reminder_time IS NOT NULL");
                });

                entity.HasKey(e => e.UserId);

                // Defaults
                entity.Property(e => e.Theme)
                      .HasDefaultValue("system");

                entity.Property(e => e.DailyGoalXp)
                      .HasDefaultValue((short)20);

                entity.Property(e => e.SoundEnabled)
                      .HasDefaultValue(true);

                entity.Property(e => e.ReminderEnabled)
                      .HasDefaultValue(false);

                entity.Property(e => e.UpdatedAt)
                      .HasDefaultValueSql("now()");
            });
        }

        // ─────────────────────────────────────────────────────────────
        // 3. user_learning_profiles
        // ─────────────────────────────────────────────────────────────
        private static void ConfigureUserLearningProfile(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserLearningProfile>(entity =>
            {
                entity.ToTable("user_learning_profiles", tb =>
                {
                    tb.HasCheckConstraint("ck_learning_reason",
                        "learning_reason IS NULL OR learning_reason IN ('travel', 'work', 'school', 'exam', 'fun', 'other')");

                    tb.HasCheckConstraint("ck_learning_cefr",
                        "cefr_level IS NULL OR cefr_level IN ('A1', 'A2', 'B1', 'B2', 'C1')");

                    tb.HasCheckConstraint("ck_learning_placement",
                        "placement_completed_at IS NULL OR cefr_level IS NOT NULL");
                });

                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UpdatedAt)
                      .HasDefaultValueSql("now()");
            });
        }

        // ─────────────────────────────────────────────────────────────
        // 4. user_stats
        // ─────────────────────────────────────────────────────────────
        private static void ConfigureUserStat(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserStat>(entity =>
            {
                entity.ToTable("user_stats", tb =>
                {
                    tb.HasCheckConstraint("ck_stats_xp",
                        "total_xp >= 0");

                    tb.HasCheckConstraint("ck_stats_streak",
                        "current_streak >= 0 AND longest_streak >= current_streak");

                    tb.HasCheckConstraint("ck_stats_freezes",
                        "streak_freezes BETWEEN 0 AND 2");
                });

                entity.HasKey(e => e.UserId);

                // Defaults
                entity.Property(e => e.TotalXp)
                      .HasDefaultValue(0L);

                entity.Property(e => e.CurrentStreak)
                      .HasDefaultValue(0);

                entity.Property(e => e.LongestStreak)
                      .HasDefaultValue(0);

                entity.Property(e => e.StreakFreezes)
                      .HasDefaultValue((short)0);

                entity.Property(e => e.UpdatedAt)
                      .HasDefaultValueSql("now()");
            });
        }

        // ─────────────────────────────────────────────────────────────
        // 5. user_daily_activity
        // ─────────────────────────────────────────────────────────────
        private static void ConfigureUserDailyActivity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserDailyActivity>(entity =>
            {
                entity.ToTable("user_daily_activity", tb =>
                {
                    tb.HasCheckConstraint("ck_daily_non_negative",
                        "xp_earned >= 0 AND lessons_completed >= 0 AND words_learned >= 0 AND practice_seconds >= 0");
                });

                // Composite PK
                entity.HasKey(e => new { e.UserId, e.ActivityDate });

                entity.HasIndex(e => e.ActivityDate)
                      .HasDatabaseName("idx_daily_activity_date");

                // Defaults
                entity.Property(e => e.XpEarned)
                      .HasDefaultValue(0);

                entity.Property(e => e.LessonsCompleted)
                      .HasDefaultValue(0);

                entity.Property(e => e.WordsLearned)
                      .HasDefaultValue(0);

                entity.Property(e => e.PracticeSeconds)
                      .HasDefaultValue(0);

                entity.Property(e => e.StreakFreezeUsed)
                      .HasDefaultValue(false);
            });
        }

        // ─────────────────────────────────────────────────────────────
        // 6. achievements
        // ─────────────────────────────────────────────────────────────
        private static void ConfigureAchievement(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Achievement>(entity =>
            {
                entity.ToTable("achievements", tb =>
                {
                    tb.HasCheckConstraint("ck_achievements_category",
                        "category IN ('streak', 'xp', 'lesson', 'vocab', 'quiz', 'exam')");

                    tb.HasCheckConstraint("ck_achievements_threshold",
                        "threshold > 0");

                    tb.HasCheckConstraint("ck_achievements_reward",
                        "xp_reward >= 0");
                });

                entity.HasKey(e => e.Code);

                // Defaults
                entity.Property(e => e.XpReward)
                      .HasDefaultValue(0);

                entity.Property(e => e.SortOrder)
                      .HasDefaultValue((short)0);

                // Relationship
                entity.HasMany(e => e.UserAchievements)
                      .WithOne(ua => ua.Achievement)
                      .HasForeignKey(ua => ua.AchievementCode)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        // ─────────────────────────────────────────────────────────────
        // 7. user_achievements
        // ─────────────────────────────────────────────────────────────
        private static void ConfigureUserAchievement(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAchievement>(entity =>
            {
                entity.ToTable("user_achievements");

                // Composite PK
                entity.HasKey(e => new { e.UserId, e.AchievementCode });

                entity.Property(e => e.AwardedAt)
                      .HasDefaultValueSql("now()");
            });
        }

        // ─────────────────────────────────────────────────────────────
        // Seed data — huy hiệu mặc định
        // ─────────────────────────────────────────────────────────────
        private static void SeedAchievements(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Achievement>().HasData(
                new Achievement
                {
                    Code = "first_lesson", Category = "lesson", Threshold = 1,
                    NameVi = "Bước đầu tiên", NameEn = "First Step",
                    DescriptionVi = "Hoàn thành bài học đầu tiên", DescriptionEn = "Complete your first lesson",
                    XpReward = 10, SortOrder = 1
                },
                new Achievement
                {
                    Code = "streak_3", Category = "streak", Threshold = 3,
                    NameVi = "Khởi động", NameEn = "Warming Up",
                    DescriptionVi = "Học 3 ngày liên tiếp", DescriptionEn = "Reach a 3-day streak",
                    XpReward = 10, SortOrder = 2
                },
                new Achievement
                {
                    Code = "streak_7", Category = "streak", Threshold = 7,
                    NameVi = "Một tuần", NameEn = "One Week",
                    DescriptionVi = "Học 7 ngày liên tiếp", DescriptionEn = "Reach a 7-day streak",
                    XpReward = 20, SortOrder = 3
                },
                new Achievement
                {
                    Code = "streak_30", Category = "streak", Threshold = 30,
                    NameVi = "Bền bỉ", NameEn = "Unstoppable",
                    DescriptionVi = "Học 30 ngày liên tiếp", DescriptionEn = "Reach a 30-day streak",
                    XpReward = 50, SortOrder = 4
                },
                new Achievement
                {
                    Code = "xp_100", Category = "xp", Threshold = 100,
                    NameVi = "Tập sự", NameEn = "Apprentice",
                    DescriptionVi = "Đạt 100 XP", DescriptionEn = "Earn 100 XP",
                    XpReward = 0, SortOrder = 5
                },
                new Achievement
                {
                    Code = "xp_1000", Category = "xp", Threshold = 1000,
                    NameVi = "Chăm chỉ", NameEn = "Hard Worker",
                    DescriptionVi = "Đạt 1.000 XP", DescriptionEn = "Earn 1,000 XP",
                    XpReward = 0, SortOrder = 6
                },
                new Achievement
                {
                    Code = "words_100", Category = "vocab", Threshold = 100,
                    NameVi = "100 từ đầu tiên", NameEn = "First 100 Words",
                    DescriptionVi = "Học 100 từ vựng", DescriptionEn = "Learn 100 words",
                    XpReward = 20, SortOrder = 7
                },
                new Achievement
                {
                    Code = "words_500", Category = "vocab", Threshold = 500,
                    NameVi = "Kho từ vựng", NameEn = "Word Collector",
                    DescriptionVi = "Học 500 từ vựng", DescriptionEn = "Learn 500 words",
                    XpReward = 50, SortOrder = 8
                },
                new Achievement
                {
                    Code = "perfect_quiz", Category = "quiz", Threshold = 1,
                    NameVi = "Không sai câu nào", NameEn = "Flawless",
                    DescriptionVi = "Làm đúng 100% một bài quiz", DescriptionEn = "Score 100% on a quiz",
                    XpReward = 10, SortOrder = 9
                },
                new Achievement
                {
                    Code = "first_exam", Category = "exam", Threshold = 1,
                    NameVi = "Thí sinh", NameEn = "Test Taker",
                    DescriptionVi = "Hoàn thành bài thi thử đầu tiên", DescriptionEn = "Finish your first mock exam",
                    XpReward = 20, SortOrder = 10
                }
            );
        }

        // =================================================================
        // SaveChanges — tự động gán created_at, updated_at, row_version
        // (thay thế trigger SQL trong mô hình Code-First)
        // =================================================================

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// Gán timestamp và row_version trước khi lưu, thay thế
        /// các trigger set_updated_at() và profiles_before_write() trong SQL.
        /// </summary>
        private void OnBeforeSaving()
        {
            var now = DateTimeOffset.UtcNow;

            // ── Profile: created_at (immutable), updated_at, row_version ──
            foreach (var entry in ChangeTracker.Entries<Profile>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.UpdatedAt = now;
                        entry.Entity.RowVersion = 1;
                        break;

                    case EntityState.Modified:
                        // Bảo vệ trường bất biến: Id và CreatedAt
                        entry.Property(e => e.Id).IsModified = false;
                        entry.Property(e => e.CreatedAt).IsModified = false;
                        entry.Entity.UpdatedAt = now;
                        entry.Entity.RowVersion++;
                        break;
                }
            }

            // ── UserSetting: updated_at ──
            foreach (var entry in ChangeTracker.Entries<UserSetting>()
                .Where(e => e.State is EntityState.Added or EntityState.Modified))
            {
                entry.Entity.UpdatedAt = now;
            }

            // ── UserLearningProfile: updated_at ──
            foreach (var entry in ChangeTracker.Entries<UserLearningProfile>()
                .Where(e => e.State is EntityState.Added or EntityState.Modified))
            {
                entry.Entity.UpdatedAt = now;
            }

            // ── UserStat: updated_at ──
            foreach (var entry in ChangeTracker.Entries<UserStat>()
                .Where(e => e.State is EntityState.Added or EntityState.Modified))
            {
                entry.Entity.UpdatedAt = now;
            }

            // ── UserAchievement: awarded_at khi thêm mới ──
            foreach (var entry in ChangeTracker.Entries<UserAchievement>()
                .Where(e => e.State == EntityState.Added))
            {
                if (entry.Entity.AwardedAt == default)
                    entry.Entity.AwardedAt = now;
            }
        }
    }
}
