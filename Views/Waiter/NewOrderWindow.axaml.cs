using Avalonia.Controls;

namespace cafeInformationSystem.Views.Waiter;

public partial class NewOrderWindow : Window
{
    public NewOrderWindow()
    {
        InitializeComponent();
    }

    // INFO! Немного упросил по сравнению с дуньгами у товара тут нет запоминания прошлого варианта 
    // так так поведение в таком случае немного странное при неправльном изменении других столбцов количества.
    private void MyDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.Column.Header.ToString() == "Количество")
        {
            var textBox = e.EditingElement as TextBox;
            
            if (textBox is null)
            {
                return;
            }
            
            var currentValue = textBox.Text;

            if (string.IsNullOrEmpty(currentValue))
            {
                textBox.Text = "1";
                return;
            }
            
            if (!short.TryParse(currentValue, out short numericValue))
            {
                textBox.Text = "1";
                return;
            }
            
            if (numericValue < 1 || numericValue > short.MaxValue)
            {
                textBox.Text = "1";
                return;
            }
        }
    }
}