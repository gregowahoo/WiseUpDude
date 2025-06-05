using System;
using System.Threading.Tasks;

namespace WiseUpDude.Shared.Services
{
    public class ToastService
    {
        public event Func<string, ToastLevel, Task>? OnShow;

        public Task ShowToast(string message, ToastLevel level = ToastLevel.Info)
        {
            return OnShow?.Invoke(message, level) ?? Task.CompletedTask;
        }
    }

    public enum ToastLevel
    {
        Info,
        Success,
        Warning,
        Error
    }
}