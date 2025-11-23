using System.Globalization;
using Avalonia.Controls;

namespace cafeInformationSystem.Views.Waiter;

public partial class CompleteOrderWindow : Window
{
    public CompleteOrderWindow()
    {
        InitializeComponent();
    }

    private void MyDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.Column.Header.ToString() == "Сумма оплаты")
        {
            var textBox = e.EditingElement as TextBox;
            
            if (textBox is null)
            {
                return;
            }
            
            var currentValue = textBox.Text;
            
            if (string.IsNullOrEmpty(currentValue))
            {
                textBox.Text = 0.01m.ToString("0.00");
                return;
            }
            
            if (!decimal.TryParse(currentValue, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal numericValue))
            {
                textBox.Text = 0.01m.ToString("0.00");
                return;
            }
            
            if (numericValue < 0.01m || numericValue > 100000m)
            {
                textBox.Text = 0.01m.ToString("0.00");
                return;
            }
            
            textBox.Text = numericValue.ToString("0.00");
        }
    }
}