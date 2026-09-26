using BooksShoppingProjectMVC.Constant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BooksShoppingProjectMVC.Controllers
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class AdminOperationsController : Controller
    {
        private readonly IUserOrderRepository _userOrderRepository;

        public AdminOperationsController(IUserOrderRepository userOrderRepository)
        {
            _userOrderRepository= userOrderRepository;
        }
        public async Task<IActionResult> AllOrders()
        {
            var order = await _userOrderRepository.UserOrder(true);
            return View(order);
        }
        public async Task<IActionResult> TogglePaymentStatus(int orderId)
        {
            try
            {
                await _userOrderRepository.TogglePaymentStatus(orderId);

            }
            catch (Exception)
            {
                // log exception here 
            }
            return RedirectToAction(nameof(AllOrders));
        }

        public async Task<IActionResult> UpdateOrderStatus(int orderId)
        {
            var order = await _userOrderRepository.GetOrderById(orderId);
            if (order == null)
            {
                throw new InvalidOperationException($"Order with id: {orderId} does not found");
            }

            var orderStatusList = (await _userOrderRepository.GetOrderStatuses())
                                                           .Select(
                orderStatus =>
                {
                    return new SelectListItem
                    {
                        Value=orderStatus.Id.ToString(),
                        Text=orderStatus.StatusName,
                        Selected=order.OrderStatusId==order.OrderStatus.Id

                    };
                });

            var data = new UpdateOrderStatusModel
            {
                OrderId = orderId,
                OrderStatusId = order.OrderStatusId,
                OrderStatusList = orderStatusList
            };

            return View(data);
                
        }

        [HttpPost]

        public async Task<IActionResult> UpdateOrderStatus(UpdateOrderStatusModel data){
            try
            {
                if (!ModelState.IsValid)
                {
                    data.OrderStatusList = (await
                        _userOrderRepository.GetOrderStatuses())
                                            .Select
                        (orderStatus =>
                        {
                            return new SelectListItem
                            {
                                Value = orderStatus.Id.ToString(),
                                Text = orderStatus.StatusName,
                                Selected = orderStatus.Id == data.OrderStatusId
                            };
                        });
                    return View(data);
                }

                await _userOrderRepository.ChangeOrderStatus(data);
                TempData["msg"] = "Updated Sucessfully";
            }
            catch (Exception)
            {

                TempData["msg"] = "Something went wrong";
                
            }
            return RedirectToAction(nameof(UpdateOrderStatus), new { orderId=data.OrderId});
        }


        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
