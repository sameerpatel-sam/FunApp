using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using FunApp.Services;
using FunApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FunApp.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly PersistentQuizService _persistent;
        private readonly QuizService _quizMemory; // still needed for current mode context

        public IndexModel(PersistentQuizService persistent, QuizService quizMemory)
        {
            _persistent = persistent;
            _quizMemory = quizMemory;
        }

        public List<Question> IndividualQuestions { get; private set; } = new();
        public List<Question> CoupleQuestions { get; private set; } = new();

        [BindProperty] public string NewIndividualQuestion { get; set; } = string.Empty;
        [BindProperty] public string NewCoupleQuestion { get; set; } = string.Empty;

        private async Task LoadQuestionsAsync()
        {
            IndividualQuestions = await _persistent.GetQuestionsAsync(GameMode.Individual);
            IndividualQuestions = IndividualQuestions.OrderBy(q => q.Id).ToList();
            CoupleQuestions = await _persistent.GetQuestionsAsync(GameMode.Couple);
            CoupleQuestions = CoupleQuestions.OrderBy(q => q.Id).ToList();
        }

        public async Task OnGetAsync()
        {
            await LoadQuestionsAsync();
        }

        public async Task<IActionResult> OnPostAddAsync(string mode, string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                var parsed = mode?.Equals("couple", System.StringComparison.OrdinalIgnoreCase) == true ? GameMode.Couple : GameMode.Individual;
                await _persistent.AddQuestionAsync(text.Trim(), parsed);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(int id, string text)
        {
            if (id > 0 && !string.IsNullOrWhiteSpace(text))
            {
                await _persistent.UpdateQuestionAsync(id, text.Trim());
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (id > 0)
            {
                await _persistent.DeleteQuestionAsync(id);
            }
            return RedirectToPage();
        }
    }
}
