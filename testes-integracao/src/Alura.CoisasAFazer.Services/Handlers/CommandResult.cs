namespace Alura.CoisasAFazer.Services.Handlers
{
    public class CommandResult
    {
        public bool Success { get; set; }
        
        public CommandResult(bool success)
        {
            Success = success;
        }
    }
}
