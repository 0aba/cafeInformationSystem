using Avalonia.Controls;
using Avalonia.Interactivity;

namespace cafeInformationSystem.Views.Administrator;

public partial class NewOrderItemWindow : Window
{
    public NewOrderItemWindow()
    {
        InitializeComponent();
    }

    private decimal? previousValue = null;

    private void NumericUpDown_ValueChanged(object sender, RoutedEventArgs e)
    {
        var numericUpDown = sender as NumericUpDown;
        
        if (numericUpDown is null)
        {
            return;
        }
        
        var currentValue = numericUpDown.Value;
        
        if (currentValue is null)
        {
            if (previousValue is not null)
            {
                numericUpDown.Value = previousValue;
            }
            else
            {
                numericUpDown.Value = 0.01m;
            }
            return;
        }
        
        previousValue = currentValue;
    }
}