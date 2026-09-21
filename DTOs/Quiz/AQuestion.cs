namespace DTOs.Quiz
{
    public abstract class BaseQuestion
    {
        public int Id { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}