using B3ly.BLL.Interfaces;
using B3ly.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace B3ly.PL.Controllers
{
    public class AIController : Controller
    {
        private readonly IAIService _ai;
        private readonly AuthService _auth;

        public AIController(IAIService ai, AuthService auth)
        {
            _ai   = ai;
            _auth = auth;
        }

        [HttpGet]
        public IActionResult Chat() => View();

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Question))
                return BadRequest(new { error = "Question cannot be empty." });

            var user = _auth.GetCurrentUser();
            if (user == null)
                return Unauthorized(new { error = "You must be logged in to use the chatbot." });

            try
            {
                var answer = await _ai.AskAsync(
                    request.Question.Trim(),
                    user.Id,
                    user.Role
                );

                return Ok(new { answer });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error: {ex.Message}" });
            }
        }

        public record AskRequest(string Question);
    }
}
