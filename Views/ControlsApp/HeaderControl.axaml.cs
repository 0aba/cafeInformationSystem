using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using cafeInformationSystem.ViewModels.ControlsApp;

namespace cafeInformationSystem.Views.ControlsApp;

public partial class HeaderControl : UserControl
{
    public HeaderControl()
    {
        InitializeComponent();
        DataContext = new HeaderViewModel();
    }
}