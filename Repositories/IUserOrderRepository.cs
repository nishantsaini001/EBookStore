namespace BooksShoppingProjectMVC
{
    public interface IUserOrderRepository
    {
        Task<IEnumerable<Order>> UserOrder(bool getall=false);
        Task ChangeOrderStatus(UpdateOrderStatusModel data);
        Task TogglePaymentStatus(int orderId);
        Task<Order?> GetOrderById(int id);
        Task<IEnumerable<OrderStatus>> GetOrderStatuses();
    }
}