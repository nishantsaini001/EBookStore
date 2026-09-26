using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksShoppingProjectMVC.Controllers
{
        [Authorize(nameof(Roles.Admin))]
    public class StockController : Controller
    {
        private readonly IStockRepository _stockRepo;
        public StockController(IStockRepository stockRepository)
        {
            _stockRepo = stockRepository;
        }
        public async Task<IActionResult> Index(string sTerm="")
        {
            var stocks= await _stockRepo.GetStocks(sTerm);
            return View(stocks);
        }
        public async Task<IActionResult> ManageStock(int bookId)
        {
            var existingstocks= await _stockRepo.GetStockByBookId(bookId);
            var stock = new StockDTO
            {
                BookId=bookId,
                Quantity=existingstocks != null ? existingstocks.Quantity:0
            };
            return View(stock);
        }
        [HttpPost]
        public async Task<IActionResult> ManageStock(StockDTO stock)
        {
            if (!ModelState.IsValid)
            {
                return View(stock);
            }
            try
            {
                await _stockRepo.ManageStock(stock);
                TempData["successMessage"] = "stock is updated Sucessfully";
            }
            catch (Exception)
            {

                TempData["errorMessage"] = "Something went wrong";
                
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
