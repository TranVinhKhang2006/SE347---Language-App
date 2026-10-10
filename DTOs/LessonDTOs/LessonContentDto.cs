using System.Text.Json.Serialization;

namespace DTOs.Lesson
{
    public class LessonContentDto
    {
        /// <summary>
        /// Danh sách các mục nội dung của bài học, được ánh xạ từ JSON.
        /// * Giải thích:
        ///   - Key: tên của mục nội dung, ví dụ: "Introduction", "Vocabulary", v.v.
        ///  - Value: đối tượng LessonContentItemDto chứa thông tin chi tiết về mục nội dung đó.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, LessonContentItemDto> Items { get; set; } = new();
    }
}