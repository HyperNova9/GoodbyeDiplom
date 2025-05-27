using Avalonia.Controls;
using Avalonia.Interactivity;
using GoodbyeDiplom.ViewModels;

namespace GoodbyeDiplom.Views
{
    public partial class ColorsScene : Window
    {
        MainWindow main;
        public ColorsScene(MainWindow main)
        {
            InitializeComponent();
            this.main = main;
        }
        private void OnColorChanged(object sender, ColorChangedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm && vm.SelectedFunction != null)
                main.UpdateGrid();
        }
        private void UpdateGridFromVM(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
                main.UpdateGrid();
        }
        private void DarkModeClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm) return;

        }
    }
}