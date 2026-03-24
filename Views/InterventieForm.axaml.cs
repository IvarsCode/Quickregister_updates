using Avalonia.Controls;
using Avalonia.Input;
using QuickRegister.Models;
using QuickRegister.ViewModels;

namespace QuickRegister.Views
{
    public partial class InterventieForm : UserControl
    {
        public InterventieForm()
        {
            InitializeComponent();
        }

        private void CallButton_PointerEntered(object? sender, PointerEventArgs e)
        {
            if (sender is Button button &&
                button.DataContext is InterventieCallDisplay display &&
                DataContext is InterventieFormViewModel viewModel)
            {
                viewModel.HoveredCall = display;
            }
        }

        private void CallButton_PointerExited(object? sender, PointerEventArgs e)
        {
            if (DataContext is InterventieFormViewModel viewModel)
            {
                viewModel.HoveredCall = null;
            }
        }
    }
}
