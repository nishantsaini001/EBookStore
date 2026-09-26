using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksShoppingProjectMVC.Controllers
{
    public class UserOrderController : Controller
    {
        private readonly IUserOrderRepository _userOrderRepo;
        public UserOrderController(IUserOrderRepository userOrderRepo)
        {
            _userOrderRepo = userOrderRepo;
        }
        [Authorize]
        public async Task<IActionResult> UserOrders()
        {
            var orders = await _userOrderRepo.UserOrder();

            return View(orders);
        }
    }
}
