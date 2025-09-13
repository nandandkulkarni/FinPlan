// Add model for survey questions used by reusable components
namespace FinPlan.Web.Components.Models
{
    public class SurveyQuestion
    {
        public string QuestionText { get; set; }
        public List<string> Options { get; set; } = new List<string>();
        public bool IsEssay => Options == null || Options.Count == 0;

        public SurveyQuestion(string questionText, List<string> options = null)
        {
            QuestionText = questionText;
            Options = options ?? new List<string>();
        }
    }
}
