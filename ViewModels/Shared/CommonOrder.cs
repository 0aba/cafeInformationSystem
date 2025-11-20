using cafeInformationSystem.Models.Entities;

namespace cafeInformationSystem.ViewModels.Shared;

public class OrderItemItem
{
    public string Name { get; set; } = string.Empty;
    public decimal? CostItem { get; set; } // INFO! Может отсутствововать при создании и изменении, но при просомотре подгружается
    public short AmountItems { get; set; } = 1;
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

public class AcceptedChefStatusFilterItem
{
    public string Name { get; set; } = string.Empty;
    public bool? Accepted { get; set; }
}
