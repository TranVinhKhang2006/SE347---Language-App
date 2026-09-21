namespace DTOs.Quiz
{
    public class SingleChoiceQuestion : BaseQuestion
    {
        public List<(int, string)> Options { get; set; } = new();
        public int CorrectAnswerId { get; set; }
    }
}