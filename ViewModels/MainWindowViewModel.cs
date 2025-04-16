using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI;
namespace GoodbyeDiplom.ViewModels;

public class MainWindowViewModel : ViewModelBase
{

    public MainWindowViewModel()
    {

    }
    private string _gridCoordinates = "X: —, Y: —, Z: —";
    public string GridCoordinates
    {
        get => _gridCoordinates;
        set => this.RaiseAndSetIfChanged(ref _gridCoordinates, value);            
    }


}
