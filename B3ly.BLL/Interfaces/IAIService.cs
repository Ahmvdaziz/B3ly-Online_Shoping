namespace B3ly.BLL.Interfaces
{
    public interface IAIService
    {
        Task<string> AskAsync(string question);

        /// <summary>
        /// Sends a trivial prompt to Ollama so the model is loaded into memory
        /// before the first real user request. Call once on application startup.
        /// </summary>
        Task WarmUpAsync();
    }
}
