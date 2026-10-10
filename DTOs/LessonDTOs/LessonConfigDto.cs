namespace DTOs.Lesson
{
    /// <summary>
    /// DTO cho việc cấu hình bài học.
    /// * Giải thích:
    ///    - Name: tên bài học, phải duy nhất trong hệ thống.
    ///    - Category: chủ đề của bài học, ví dụ: "Ngành nghề", "Con vật", v.v.
    ///    - Description: mô tả ngắn gọn về bài học.
    ///    - Level: mức độ khó của bài học, ví dụ: "Dễ", "Trung bình", "Khó".
    ///    - Content: nội dung chi tiết của bài học, đây là đường dẫn đến tệp tin nội dung.
    ///    - Quiz: đường dẫn đến tệp tin câu hỏi trắc nghiệm của bài học, có thể là null nếu bài học không có câu hỏi.
    /// </summary>
    public class LessonConfigDto
    {
        public string Name { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Level { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? Quiz { get; set; } = null;
    }
}