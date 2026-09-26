using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BooksShoppingProjectMVC.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartRepository(ApplicationDbContext db, UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> AddItem(int bookId, int qty)
        {
            string userId = GetUserId();
            using var transaction = _db.Database.BeginTransaction();

            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("User is not logged-in");
                }
                var cart = await GetCart(userId);
                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId,
                    };
                    _db.ShoppingCarts.Add(cart);
                }
                _db.SaveChanges();

                // cart detail

                var cartItem = _db.CartDetails.FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);

                if (cartItem is not null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    var book = _db.Books.Find(bookId);
                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty,
                        UnitPrice=book.Price
                    };

                    _db.CartDetails.Add(cartItem);
                }

                _db.SaveChanges();
                transaction.Commit();

            }
            catch (Exception ex)
            { }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }
        public async Task<int> RemoveItem(int bookId)
        {

            string userId = GetUserId();
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("user is not logged in");
                }
                var cart = await GetCart(userId);
                if (cart is null)
                {
                    throw new InvalidOperationException("Invalid Cart");
                }


                // cart detail

                var cartItem = _db.CartDetails.FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);

                if (cartItem is null)
                    throw new InvalidOperationException("No item in Cart");

                else if (cartItem.Quantity == 1)
                {
                    _db.CartDetails.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity -= 1;
                }

                _db.SaveChanges();


            }
            catch (Exception ex)
            {
            }

            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }

        public async Task<ShoppingCart> GetUserCart()
        {
            var userId = GetUserId();
            if (userId == null)
            {
                throw new UnauthorizedAccessException("Invalid User!!");
            }


            var shoppingCart = await _db.ShoppingCarts
                                        .Include(a => a.CartDetails)
                                        .ThenInclude(a=>a.Book)
                                        .ThenInclude(a=>a.Stock)
                                        .Include(a => a.CartDetails)
                                        .ThenInclude(a => a.Book)
                                        .ThenInclude(a => a.Genre)
                                        .Where(a => a.UserId == userId).FirstOrDefaultAsync();
            return shoppingCart;
        }

        public async Task<ShoppingCart> GetCart(string userId)
        {
            var result = await _db.ShoppingCarts.FirstOrDefaultAsync(a => a.UserId == userId);

            return result;

        }

        public async Task<int> GetCartItemCount(string userId = "")
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = GetUserId();
            }

            var data = await (from cart in _db.ShoppingCarts
                              join cartDetail in _db.CartDetails
                              on cart.Id equals cartDetail.ShoppingCartId
                              where cart.UserId==userId
                              select new { cartDetail.id }

                ).ToListAsync();

            return data.Count;
        }

        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            var userId = _userManager.GetUserId(principal);

            return userId;
        }

        public async Task<bool> DoCheckout(CheckoutModel model)
        {
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                //move data from cartdetails to order and order details then we will remove cart detail

                var userId = GetUserId();

                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("User is not logged in");
                }

                var cart = await GetCart(userId);
                if (cart is null)
                    throw new InvalidOperationException("Invalid Cart");

                var cartDetail = _db.CartDetails.Where(a=>a.ShoppingCartId==cart.Id).ToList();
                if (cartDetail.Count==0)
                {
                    throw new InvalidOperationException("Cart is Empty");
                }

                var pendingRecord = _db.OrderStatuses.FirstOrDefault(s=>s.StatusName=="pending");
                if (pendingRecord is null)
                {
                    throw new InvalidOperationException("Order status does not have Pending Status");
                }

                var order = new Order
                {
                    UserId = userId,
                    CreatedTime = DateTime.UtcNow,
                    Name = model.Name,
                    Email = model.Email,
                    Mobile = model.Mobile,
                    PaymentMethod = model.PaymentMethod,
                    Address = model.Address,
                    IsPaid =false,
                    OrderStatusId = pendingRecord.Id
                };
                _db.Orders.Add(order);
                _db.SaveChanges();

                foreach(var item in cartDetail)
                {
                    var orderDetail = new OrderDetail
                    {
                        BookId = item.BookId,
                        OrderId = order.Id,
                        Quantity = item.Quantity,
                        UnitPrice=item.UnitPrice
                    };


                    _db.OrderDetails.Add(orderDetail);


                    //update stock

                    var stock = await _db.Stocks.FirstOrDefaultAsync(a=>a.BookId==item.BookId);

                    if (stock == null)
                    {
                        throw new InvalidOperationException("Stock is null");
                    }
                    if (stock.Quantity < item.Quantity)
                    {
                        throw new InvalidOperationException($"Only {stock.Quantity} Items available in Stock");
                    }
                    //decrease
                    stock.Quantity -= item.Quantity;
                }

                _db.SaveChanges();


                _db.CartDetails.RemoveRange(cartDetail);
                _db.SaveChanges();
                transaction.Commit();
                return true;

            }
            catch (Exception)
            {

                return false;
            }
        }


    }
}
