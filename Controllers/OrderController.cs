using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;
using System.Drawing.Printing;
using TequilasRestaurant.Data;
using TequilasRestaurant.Models;

namespace TequilasRestaurant.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private Repository<Product> _product;
        private Repository<Order> _order;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this._context = context;
            this._userManager = userManager;
            this._product = new Repository<Product>(context);
            this._order = new Repository<Order>(context);

        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel") ?? new OrderViewModel
            {
                OrderItems = new List<OrderItemViewModel>(),
                Products = await _product.GetAllAsync()
            };

            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> AddItem(int prodId, int prodQty)
        {
            Product prod = await _product.GetByIdAsync(prodId, new QueryOptions<Product>());
            if (prod == null)
            {
                return NotFound();
            }

            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel") ?? new OrderViewModel
            {
                OrderItems = new List<OrderItemViewModel>(),
                Products = await _product.GetAllAsync()
            };
            var existingProd = model.OrderItems.FirstOrDefault(i => i.ProductId == prodId);

            if (existingProd != null)
            {
                existingProd.Quantity += prodQty;
            }
            else
            {
                model.OrderItems.Add(new OrderItemViewModel
                {
                    ProductId = prodId,
                    Quantity = prodQty,
                    Price = prod.Price,
                    ProductName = prod.Name,
                });
            }
            model.TotalAmount = model.OrderItems.Sum(i => i.Quantity * i.Price);
            HttpContext.Session.Set("OrderViewModel", model);
            return RedirectToAction("Create", model);
        }

        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel");
            if (model == null || model.OrderItems.Count == 0)
            {
                return RedirectToAction("Create");
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            var model = HttpContext.Session.Get<OrderViewModel>("OrderViewModel");
            if (model == null || model.OrderItems.Count == 0)
            {
                return RedirectToAction("Create");
            }
            Order order = new Order
            {
                OrderDate = DateTime.Now,
                TotalAmount = model?.TotalAmount ?? 0,
                UserId = _userManager.GetUserId(User)
            };

            foreach(var item in model.OrderItems)
            {
                order.OrderItem.Add(new OrderItem
                {
                    ProductId = item.ProductId, 
                    Quantity = item.Quantity,
                    Price = item.Price,
                });
            }

            await _order.AddAsync(order);

            HttpContext.Session.Remove("OrderViewModel");
            return RedirectToAction("ViewOrders");
        }

        [HttpGet]
        public async Task<IActionResult> ViewOrders()
        {
            var userId = _userManager.GetUserId(User);
            var userOrders = await _order.GetAllByIdAsync(userId,"UserId",new QueryOptions<Order> { Includes = "OrderItem.Product"});
            return View(userOrders);

        }
    }
}
