using cafeInformationSystem.Models.Entities;

namespace cafeInformationSystem.ViewModels.Shared;

public class OrderItemItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public short AmountItems { get; set; }
}

public class CashReceiptOrderItem
{
    public decimal PaymentAmount { get; set; }
    public bool TypePay { get; set; }
}

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
