namespace DTOs.Quiz
{
    public class MultipleChoiceQuestion : BaseQuestion
    {
        public List<(int, string)> Options { get; set; } = new();
        public List<int> CorrectAnswerIds { get; set; } = new();
    }
}