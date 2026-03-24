namespace QuickRegister.ViewModels
{
    public interface IClosingGuard
    {
        /// Called when the user clicks the window X button.
        /// Return true to allow closing, false to block and show a warning.
        bool OnWindowCloseRequested();
    }
}
