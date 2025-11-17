using System;
using cafeInformationSystem.ViewModels.Administrator;

namespace cafeInformationSystem.Views.Administrator;

public partial class ChangeEmployeeWindow : BaseEmployeeImageWindow
{
    public ChangeEmployeeWindow()
    {
        InitializeComponent();

        PhotoFileSelected += OnPhotoFileSelected;
        ContractFileSelected += OnContractFileSelected;
    }

    protected override void UpdateDropZoneText(TypeImage type, string fileName)
    {
        if (type == TypeImage.Photo)
        {
            PhotoText.Text = fileName;
            PhotoText.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.LightGreen);
        }
        else if (type == TypeImage.Contract)
        {
            ContractText.Text = fileName;
            ContractText.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.LightGreen);
        }
    }

    private async void SelectPhotoButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await SelectImage(TypeImage.Photo, "Выберите фото сотрудника");
    }

    private async void SelectContractButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await SelectImage(TypeImage.Contract, "Выберите скан договора");
    }

    private void OnPhotoFileSelected(string filePath, string fileName)
    {
        if (DataContext is ChangeEmployeeViewModel vm)
        {
            vm.LoadPhoto(filePath, fileName);
        }
    }

    private void OnContractFileSelected(string filePath, string fileName)
    {
        if (DataContext is ChangeEmployeeViewModel vm)
        {
            vm.LoadScanEmploymentContract(filePath, fileName);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        PhotoFileSelected -= OnPhotoFileSelected;
        ContractFileSelected -= OnContractFileSelected;
        base.OnClosed(e);
    }
}
