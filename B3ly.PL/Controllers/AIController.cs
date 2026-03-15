using B3ly.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace B3ly.PL.Controllers
{
    public class AIController : Controller
    {
        private readonly IAIService _ai;

        public AIController(IAIService ai) => _ai = ai;

        [HttpGet]
        public IActionResult Chat() => View();

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Question))
                return BadRequest(new { error = "Question cannot be empty." });

            var answer = await _ai.AskAsync(request.Question.Trim());
            return Ok(new { answer });
        }

        public record AskRequest(string Question);
    }
}
