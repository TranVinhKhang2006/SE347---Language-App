namespace SE347.DTOs.Users
{
    /// <summary>
    /// Hồ sơ của chính user đang đăng nhập (GET /api/users/me).
    /// </summary>
    public record MyProfileDto(
        Guid Id,
        string Username,
        string DisplayName,
        string? AvatarPath,
        string? Bio,
        string UiLocale,
        string Timezone,
        string ProfileVisibility,
        string Role,
        DateTimeOffset? OnboardingCompletedAt);
}
