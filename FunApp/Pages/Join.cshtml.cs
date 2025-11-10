using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FunApp.Services;
using FunApp.Models;

namespace FunApp.Pages
{
    public class JoinModel : PageModel
    {
        private readonly QuizService _quizService;

        public JoinModel(QuizService quizService)
        {
            _quizService = quizService;
        }

        public void OnGet()
        {
        }
    }
}