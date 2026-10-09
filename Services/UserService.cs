using Microsoft.EntityFrameworkCore;
using SE347.Data;
using SE347.DTOs.Users;

namespace SE347.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _db;

        public UserService(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Dùng hàm public.is_username_available trong DB để giữ đúng một bộ luật username
        /// (định dạng, tên đặt trước, trùng lặp) với file SQL migration.
        /// </summary>
        public async Task<bool> IsUsernameAvailableAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _db.Database
                .SqlQuery<bool>($"select public.is_username_available({username}) as \"Value\"")
                .SingleAsync(cancellationToken);
        }

        /// <summary>
        /// Backend kết nối bằng role postgres nên RLS không chặn: phải tự lọc
        /// chỉ trả hồ sơ đang hoạt động và để chế độ công khai.
        /// </summary>
        public async Task<PublicProfileDto?> GetPublicProfileAsync(string username, CancellationToken cancellationToken = default)
        {
            var normalized = username.Trim().ToLowerInvariant();

            return await _db.Profiles
                .AsNoTracking()
                .Where(p => p.Username == normalized
                    && p.ProfileVisibility == "public"
                    && p.Status == "active")
                .Select(p => new PublicProfileDto(p.Username, p.DisplayName, p.AvatarPath, p.Bio))
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}
