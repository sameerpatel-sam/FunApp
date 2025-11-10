using Microsoft.AspNetCore.Mvc.RazorPages;
using FunApp.Services;
using FunApp.Models;

namespace FunApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly QuizService _quizService;

        public IndexModel(QuizService quizService)
        {
            _quizService = quizService;
        }

        public string JoinUrl => $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/join";
        
        public void OnGet()
        {
        }
    }
}