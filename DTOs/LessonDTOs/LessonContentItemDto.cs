namespace DTOs.Lesson
{
    public class LessonContentItemDto
    {
        /// <summary>
        /// DTO cho một mục nội dung của bài học.
        /// * Giải thích:
        ///   - Content: văn bản chính của mục bài học, có thể là câu, đoạn văn, hoặc bất kỳ nội dung nào.
        ///   - Description: mô tả ngắn gọn về mục bài học.
        ///   - Translation: bản dịch tiếng Việt của nội dung.
        ///   - Attachments: danh sách các tệp đính kèm (hình ảnh, tài liệu, v.v.).
        /// </summary>
        public string Content { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Translation { get; set; } = null!;
        public List<string> Attachments { get; set; } = null!;
    }
}