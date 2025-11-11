using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using FunApp.Services;
using FunApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace FunApp.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly QuizService _quizService;

        public IndexModel(QuizService quizService)
        {
            _quizService = quizService;
        }

        public List<Question> IndividualQuestions { get; private set; } = new();
        public List<Question> CoupleQuestions { get; private set; } = new();

        [BindProperty] public string NewIndividualQuestion { get; set; } = string.Empty;
        [BindProperty] public string NewCoupleQuestion { get; set; } = string.Empty;

        private void LoadQuestions()
        {
            IndividualQuestions = _quizService.GetQuestionsByMode(GameMode.Individual).OrderBy(q => q.Id).ToList();
            CoupleQuestions = _quizService.GetQuestionsByMode(GameMode.Couple).OrderBy(q => q.Id).ToList();
        }

        public void OnGet()
        {
            LoadQuestions();
        }

        public IActionResult OnPostAdd(string mode, string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                var parsed = mode?.Equals("couple", System.StringComparison.OrdinalIgnoreCase) == true ? GameMode.Couple : GameMode.Individual;
                _quizService.AddQuestion(text.Trim(), parsed);
            }
            return RedirectToPage();
        }

        public IActionResult OnPostUpdate(int id, string text)
        {
            if (id > 0 && !string.IsNullOrWhiteSpace(text))
            {
                _quizService.UpdateQuestion(id, text.Trim());
            }
            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int id)
        {
            if (id > 0)
            {
                _quizService.DeleteQuestion(id);
            }
            return RedirectToPage();
        }
    }
}
