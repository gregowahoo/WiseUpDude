using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.JSInterop;
using WiseUpDude.Services;

public class CustomCircuitHandler : CircuitHandler
{
    private readonly IJSRuntime _jsRuntime;
    private readonly QuizStateService _quizStateService;
    private static bool _isFirstConnection = true;

    public CustomCircuitHandler(IJSRuntime jsRuntime, QuizStateService quizStateService)
    {
        _jsRuntime = jsRuntime;
        _quizStateService = quizStateService;
    }

    public override async Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        Console.WriteLine("Connection lost. Saving Quiz ID to sessionStorage...");

        if (_quizStateService.CurrentQuiz != null)
        {
            // Save only the Quiz ID to sessionStorage
            try
            {
                await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "quizId", _quizStateService.CurrentQuiz.Id.ToString());
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("Task was canceled: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        await base.OnConnectionDownAsync(circuit, cancellationToken);
    }

    public override async Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        if (_isFirstConnection)
        {
            Console.WriteLine("Initial connection established. Skipping sessionStorage loading.");
            _isFirstConnection = false; // Mark the first connection as handled
            return;
        }

        Console.WriteLine("Connection restored. Loading Quiz ID from sessionStorage...");

        var quizIdString = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "quizId");
        if (!string.IsNullOrEmpty(quizIdString) && int.TryParse(quizIdString, out int quizId))
        {
            var loadedQuiz = await _quizStateService.LoadQuizFromDatabaseAsync(quizId);
            if (loadedQuiz != null)
            {
                _quizStateService.UpdateCurrentQuiz(loadedQuiz); // Use the new method
            }
            else
            {
                Console.WriteLine($"Quiz with ID {quizId} not found in the database.");
            }
        }

        await base.OnConnectionUpAsync(circuit, cancellationToken);
    }
}
