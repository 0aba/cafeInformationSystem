using CommunityToolkit.Mvvm.Input;
using cafeInformationSystem.Models.Entities;
using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using cafeInformationSystem.Views.Administrator;
using cafeInformationSystem.Models.DataBase;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;
using Avalonia.Platform.Storage;
using System.IO;
using System.Threading.Tasks;

namespace cafeInformationSystem.ViewModels.Administrator;

public class OrderStatusFilterItem
{
    public string Name { get; set; } = string.Empty;
    public OrderStatus? Status { get; set; }
}

public class OrderCookingStatusFilterItem
{
    public string Name { get; set; } = string.Empty;
    public bool? Cooked { get; set; }
}

public partial class OrderReportViewModel : ViewModelBase
{
    public OrderReportViewModel()
    {
        SelectedStatusOrderFilter = AvailableStatusOrder[0];
        SelectedStatusCookingOrderFilter = AvailableStatusCookingOrder[0];

        BackToAdministratorMenuCommand = new RelayCommand(ExecuteBackToAdministratorMenu);
        GetPDFReportCommand = new RelayCommand(ExecuteGetPDFReport);
        GetXLSXReportCommand = new RelayCommand(ExecuteGetXLSXReport);

        MinCreatedAtFilter = DateTimeOffset.Now.AddMonths(-1);
        MaxCreatedAtFilter = DateTimeOffset.Now.AddMonths(1);
        MinClosedAtFilter = DateTimeOffset.Now.AddMonths(-1);
        MaxClosedAtFilter = DateTimeOffset.Now.AddMonths(1);
    }

    private string _orderCodeFilter = string.Empty;
    private DateTimeOffset _minCreatedAtFilter = new();
    private DateTimeOffset _maxCreatedAtFilter = new();
    private DateTimeOffset _minClosedAtFilter = new();
    private DateTimeOffset _maxClosedAtFilter = new();
    public decimal? _minCostFilter = null;
    public decimal? _maxCostFilter = null;
    public int? _minAmountClientsFilter = null;
    public int? _maxAmountClientsFilter = null;
    private string _waiterLoginFilter = string.Empty;
    private string _tableCodeFilter = string.Empty;
    private string _chefLoginFilter = string.Empty;

    public List<OrderStatusFilterItem> AvailableStatusOrder { get; } = new()
    {
        new OrderStatusFilterItem { Name = "Все", Status = null },
        new OrderStatusFilterItem { Name = "Принят", Status = OrderStatus.Accepted },
        new OrderStatusFilterItem { Name = "Оплачен", Status = OrderStatus.Paid },
        new OrderStatusFilterItem { Name = "Отменен", Status = OrderStatus.Cancelled },
    };
    private OrderStatusFilterItem? _selectedStatusOrderFilter;
    public List<OrderCookingStatusFilterItem> AvailableStatusCookingOrder { get; } = new()
    {
        new OrderCookingStatusFilterItem { Name = "Все", Cooked = null },
        new OrderCookingStatusFilterItem { Name = "Готов", Cooked = true },
        new OrderCookingStatusFilterItem { Name = "Готовится", Cooked = false }
    };
    private OrderCookingStatusFilterItem? _selectedStatusCookingOrderFilter;

    private string _errorMessage = string.Empty;

    public string OrderCodeFilter
    {
        get => _orderCodeFilter;
        set => SetProperty(ref _orderCodeFilter, value);
    }

    public DateTimeOffset MinCreatedAtFilter
    {
        get => _minCreatedAtFilter;
        set => SetProperty(ref _minCreatedAtFilter, value);
    }

    public DateTimeOffset MaxCreatedAtFilter
    {
        get => _maxCreatedAtFilter;
        set => SetProperty(ref _maxCreatedAtFilter, value);
    }

    public DateTimeOffset MinClosedAtFilter
    {
        get => _minClosedAtFilter;
        set => SetProperty(ref _minClosedAtFilter, value);
    }

    public DateTimeOffset MaxClosedAtFilter
    {
        get => _maxClosedAtFilter;
        set => SetProperty(ref _maxClosedAtFilter, value);
    }

    public decimal? MinCostFilter
    {
        get => _minCostFilter;
        set => SetProperty(ref _minCostFilter, value);
    }

    public decimal? MaxCostFilter
    {
        get => _maxCostFilter;
        set => SetProperty(ref _maxCostFilter, value);
    }

    public int? MinAmountClientsFilter
    {
        get => _minAmountClientsFilter;
        set => SetProperty(ref _minAmountClientsFilter, value);
    }

    public int? MaxAmountClientsFilter
    {
        get => _maxAmountClientsFilter;
        set => SetProperty(ref _maxAmountClientsFilter, value);
    }

    public string WaiterLoginFilter
    {
        get => _waiterLoginFilter;
        set => SetProperty(ref _waiterLoginFilter, value);
    }

    public string TableCodeFilter
    {
        get => _tableCodeFilter;
        set => SetProperty(ref _tableCodeFilter, value);
    }

    public string ChefLoginFilter
    {
        get => _chefLoginFilter;
        set => SetProperty(ref _chefLoginFilter, value);
    }

    public OrderStatusFilterItem? SelectedStatusOrderFilter
    {
        get => _selectedStatusOrderFilter;
        set => SetProperty(ref _selectedStatusOrderFilter, value);
    }

    public OrderCookingStatusFilterItem? SelectedStatusCookingOrderFilter
    {
        get => _selectedStatusCookingOrderFilter;
        set => SetProperty(ref _selectedStatusCookingOrderFilter, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand BackToAdministratorMenuCommand { get; }
    public ICommand GetPDFReportCommand { get; }
    public ICommand GetXLSXReportCommand { get; }

    private void ExecuteBackToAdministratorMenu()
    {
        Window window = new AdministratorMenuWindow()
        {
            DataContext = new AdministratorMenuViewModel()
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var currentWindow = desktop.MainWindow;

            desktop.MainWindow = window;
            desktop.MainWindow.Show();

            currentWindow?.Close();
        }
    }

    private async void ExecuteGetPDFReport()
    {
        if (!ValidateInput())
        {
            return;
        }

        var filteredOrders = GetFilteredOrders();

        if (filteredOrders.Count == 0)
        {
            ErrorMessage = "Список пуст";
            return;
        }

        try
        {
            var document = CreatePdfDocument(filteredOrders);

            byte[] pdfBytes;
            using (var stream = new MemoryStream())
            {
                document.GeneratePdf(stream);
                pdfBytes = stream.ToArray();
            }

            await SaveFileAsync(pdfBytes, "отчет_заказы", "pdf", "PDF документ", ["*.pdf"]);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ErrorMessage = "Ошибка при создании PDF";
        }
    }

    private async void ExecuteGetXLSXReport()
    {
        if (!ValidateInput())
        {
            return;
        }

        var filteredOrders = GetFilteredOrders();

        if (filteredOrders.Count == 0)
        {
            ErrorMessage = "Список пуст";
            return;
        }

        try
        {
            byte[] xlsxBytes = CreateXlsxDocument(filteredOrders);

            await SaveFileAsync(xlsxBytes, "отчет_заказы", "xlsx", "Excel документ", ["*.xlsx"]);
        }
        catch (Exception)
        {
            ErrorMessage = "Ошибка при создании XLSX";
        }
    }

    private Document CreatePdfDocument(List<Order> orders)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Header()
                    .AlignCenter()
                    .Text("Отчет по заказам")
                    .SemiBold().FontSize(14);

                page.Content()
                    .PaddingVertical(0.5f, Unit.Centimetre)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(70);  // INFO! Код заказа
                            columns.ConstantColumn(90);  // INFO! Дата создания
                            columns.ConstantColumn(90);  // INFO! Дата закрытия
                            columns.ConstantColumn(70);  // INFO! Итоговая сумма
                            columns.ConstantColumn(50);  // INFO! Клиенты
                            columns.ConstantColumn(80);  // INFO! Официант
                            columns.ConstantColumn(60);  // INFO! Код стола
                            columns.ConstantColumn(80);  // INFO! Повар
                            columns.ConstantColumn(70);  // INFO! Статус заказа
                            columns.ConstantColumn(70);  // INFO! Статус готовки
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Код заказа").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Дата создания").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Дата закрытия").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Итоговая сумма").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Клиенты").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Официант").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Код стола").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Повар").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Статус заказа").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Статус готовки").SemiBold();
                        });

                        foreach (var order in orders)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(order.OrderCode);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(order.CreatedAt.ToString("dd.MM.yy HH:mm"));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(order.ClosedAt?.ToString("dd.MM.yy HH:mm") ?? "");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(order.TotalCost?.ToString("C") ?? "");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(order.AmountClients.ToString());
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(order.Waiter?.Username ?? "");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(order.Table?.TableCode ?? "");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(order.Chef?.Username ?? "");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(GetOrderStatusText(order.Status));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(GetCookingStatusText(order.CookingStatus));
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Страница ");
                        text.CurrentPageNumber();
                        text.Span(" из ");
                        text.TotalPages();
                        text.Span($" | Сгенерировано: {DateTime.Now:dd.MM.yyyy HH:mm}");
                    });
            });
        });
    }

    private byte[] CreateXlsxDocument(List<Order> orders)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Заказы");

        var headers = new[]
        {
            "Код заказа", "Дата создания", "Дата закрытия", "Итоговая сумма",
            "Количество клиентов", "Обслужил официант", "Код стола", 
            "Обслужил повар", "Статус заказа", "Статус готовки"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var order in orders)
        {
            worksheet.Cell(row, 1).Value = order.OrderCode;
            worksheet.Cell(row, 2).Value = order.CreatedAt;
            worksheet.Cell(row, 2).Style.NumberFormat.Format = "dd.MM.yyyy HH:mm";
            worksheet.Cell(row, 3).Value = order.ClosedAt;
            if (order.ClosedAt.HasValue)
            {
                worksheet.Cell(row, 3).Style.NumberFormat.Format = "dd.MM.yyyy HH:mm";
            }
            worksheet.Cell(row, 4).Value = order.TotalCost;
            if (order.TotalCost.HasValue)
            {
                worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00\"р\"";
            }
            worksheet.Cell(row, 5).Value = order.AmountClients;
            worksheet.Cell(row, 6).Value = order.Waiter?.Username ?? "";
            worksheet.Cell(row, 7).Value = order.Table?.TableCode ?? "";
            worksheet.Cell(row, 8).Value = order.Chef?.Username ?? "";
            worksheet.Cell(row, 9).Value = GetOrderStatusText(order.Status);
            worksheet.Cell(row, 10).Value = GetCookingStatusText(order.CookingStatus);
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private async Task SaveFileAsync(byte[] fileBytes, string fileName, string extension, string fileTypeName, string[] patterns)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
            
            if (topLevel is not null)
            {
                var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Сохранить отчет",
                    SuggestedFileName = $"{fileName}_{DateTime.Now:yyyy-MM-dd_HH-mm}.{extension}",
                    DefaultExtension = extension,
                    FileTypeChoices =
                    [
                        new FilePickerFileType(fileTypeName)
                        {
                            Patterns = patterns
                        }
                    ]
                });

                if (file is not null)
                {
                    await using var stream = await file.OpenWriteAsync();
                    await stream.WriteAsync(fileBytes);
                    await stream.FlushAsync();
                }
            }
        }
    }

    private string GetOrderStatusText(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Accepted => "Принят",
            OrderStatus.Paid => "Оплачен", 
            OrderStatus.Cancelled => "Отменен",
            _ => "Неизвестен"
        };
    }

    private string GetCookingStatusText(bool? cookingStatus)
    {
        return cookingStatus switch
        {
            true => "Готов",
            false => "Готовится",
            null => ""
        };
    }

    private List<Order> GetFilteredOrders()
    {
        try
        {
            var context = DatabaseService.GetContext();

            var query = context.Order.Include(o => o.Waiter).Include(o => o.Chef).Include(o => o.Table)
                                     .AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(OrderCodeFilter))
            {
                query = query.Where(o => o.OrderCode.Contains(OrderCodeFilter));
            }

            query = query.Where(o => o.CreatedAt >= MinCreatedAtFilter.UtcDateTime);
            query = query.Where(o => o.CreatedAt <= MaxCreatedAtFilter.UtcDateTime);

            if (SelectedStatusOrderFilter?.Status is not null 
            && (SelectedStatusOrderFilter.Status == OrderStatus.Paid 
            || SelectedStatusOrderFilter.Status == OrderStatus.Cancelled))
            {
                query = query.Where(o => o.ClosedAt >= MinClosedAtFilter.UtcDateTime);
                query = query.Where(o => o.ClosedAt <= MaxClosedAtFilter.UtcDateTime);
            }

            if (MinCostFilter is not null)
            {
                query = query.Where(o => o.TotalCost >= MinCostFilter);
            }

            if (MaxCostFilter is not null)
            {
                query = query.Where(o => o.TotalCost <= MaxCostFilter);
            }

            if (MinAmountClientsFilter is not null)
            {
                query = query.Where(o => o.AmountClients >= MinAmountClientsFilter);
            }

            if (MaxAmountClientsFilter is not null)
            {
                query = query.Where(o => o.AmountClients <= MaxAmountClientsFilter);
            }

            if (!string.IsNullOrWhiteSpace(WaiterLoginFilter))
            {
                var waiter = context.Employee.AsNoTracking().FirstOrDefault(e => e.Username == WaiterLoginFilter);

                query = query.Where(o => o.WaiterId == waiter!.Id);
            }

            if (!string.IsNullOrWhiteSpace(TableCodeFilter))
            {
                var table = context.Table.AsNoTracking().FirstOrDefault(t => t.TableCode == TableCodeFilter);

                query = query.Where(o => o.TableId == table!.Id);
            }

            if (!string.IsNullOrWhiteSpace(ChefLoginFilter))
            {
                var chef = context.Employee.AsNoTracking().FirstOrDefault(e => e.Username == ChefLoginFilter);

                query = query.Where(o => o.ChefId == chef!.Id);
            }

            if (SelectedStatusOrderFilter?.Status is not null)
            {
                query = query.Where(o => o.Status == SelectedStatusOrderFilter.Status);
            }

            if (SelectedStatusCookingOrderFilter?.Cooked is not null)
            {
                query = query.Where(o => o.CookingStatus == SelectedStatusCookingOrderFilter.Cooked);
            }

            return query.ToList();
        }
        catch (Exception)
        {
            ErrorMessage = "Ошибка получения заказов";
            return new();
        }
    }

    private bool ValidateInput()
    {
        if (!string.IsNullOrWhiteSpace(OrderCodeFilter) && OrderCodeFilter.Length > 256)
        {
            ErrorMessage = "Поле код заказа должно быть длинной не более 256 символов";
            return false;
        }

        var context = DatabaseService.GetContext();

        if (!string.IsNullOrWhiteSpace(WaiterLoginFilter))
        {
            var waiter = context.Employee.AsNoTracking().FirstOrDefault(e => e.Username == WaiterLoginFilter);

            if (waiter is null)
            {
                ErrorMessage = "Официанта с таким логином не существует";
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(TableCodeFilter))
        {
            var table = context.Table.AsNoTracking().FirstOrDefault(t => t.TableCode == TableCodeFilter);

            if (table is null)
            {
                ErrorMessage = "Столик с таким кодом не существует";
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(ChefLoginFilter))
        {
            var chef = context.Employee.AsNoTracking().FirstOrDefault(e => e.Username == ChefLoginFilter);

            if (chef is null)
            {
                ErrorMessage = "Повар с таким логином не существует";
                return false;
            }
        }

        ErrorMessage = string.Empty;
        return true;
    }
}
