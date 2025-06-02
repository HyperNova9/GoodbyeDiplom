using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
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
            var isDark = ThemeToggle.IsChecked ?? false;
            var resources = main.Resources;
            if (isDark)
            {
                resources["BackgroundColor"] = Color.Parse("#2D2D2D");
                resources["ForegroundColor"] = Color.Parse("#FFFFFF");
                resources["PanelBackgroundColor"] = Color.Parse("#3D3D3D");
                resources["BorderColor"] = Color.Parse("#4D4D4D");
                resources["StatusBarColor"] = Color.Parse("#1D1D1D");

                // Цвета TextBox
                resources["TextBoxBackgroundColor"] = Color.Parse("#3D3D3D");
                resources["TextBoxForegroundColor"] = Color.Parse("#FFFFFF");
                resources["TextBoxWatermarkColor"] = Color.Parse("#AAAAAA");

                this.Resources["ExpanderHeaderBackground"] = Color.Parse("#3D3D3D");
                this.Resources["ExpanderHeaderForeground"] = Color.Parse("#FFFFFF");
                this.Resources["ExpanderContentBackground"] = Color.Parse("#2D2D2D");
                this.Resources["ExpanderBorderColor"] = Color.Parse("#4D4D4D");
            }
            else
            {
                resources["BackgroundColor"] = Color.Parse("#F5F5F5");
                resources["ForegroundColor"] = Color.Parse("#212121");
                resources["PanelBackgroundColor"] = Color.Parse("#FFFFFF");
                resources["BorderColor"] = Color.Parse("#E3E3E3");
                resources["StatusBarColor"] = Color.Parse("#EEEEEE");

                // Цвета TextBox
                resources["TextBoxBackgroundColor"] = Color.Parse("#FFFFFF");
                resources["TextBoxForegroundColor"] = Color.Parse("#000000");
                resources["TextBoxWatermarkColor"] = Color.Parse("#808080");
                
                this.Resources["ExpanderHeaderBackground"] = Color.Parse("#F5F5F5");
                this.Resources["ExpanderHeaderForeground"] = Color.Parse("#212121");
                this.Resources["ExpanderContentBackground"] = Color.Parse("#FFFFFF");
                this.Resources["ExpanderBorderColor"] = Color.Parse("#E3E3E3");
            }
        }
    }
}