using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Cafeteria.Data;
using Cafeteria.Models;

namespace Cafeteria.Controllers
{
    public class OrderController : Controller
    {
        private readonly CafeteriaContext _context;

        public OrderController(CafeteriaContext context)
        {
            _context = context;
        }

        // GET: Order
        public async Task<IActionResult> Index()
        {
            return View(await _context.Order.ToListAsync());
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Order/Create
        public async Task<IActionResult> Create()
        {   
            var products = await _context.Product.ToListAsync();
            products = products.Where(p => p.Quantity!=0).ToList();

            OrderCreateViewModel viewModel = new OrderCreateViewModel
            {
                ProductsSelectList = products.Select(p => new SelectListItem 
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }),
                Products = products
            };

            TempData.Clear();

            return View(viewModel);
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel viewModel)
        {
            // If user finish order without products, return to index
            if(TempData.IsNullOrEmpty())
            {
                return RedirectToAction(nameof(Index));
            }

            Order order = new Order
            {
                TimeStamp = DateTime.Now,
                TotalPrice = viewModel.TotalPrice
            };

            _context.Order.Add(order);
            await _context.SaveChangesAsync();

            Order ?o = await _context.Order.FindAsync(order.Id);

            if(o==null)
                return BadRequest();

            List<OrderItem>orderItems = new List<OrderItem>();

            foreach(string key in TempData.Keys)
            {
            
                string? stringValue = TempData[key]?.ToString();

                if(stringValue==null)
                    return BadRequest();

                OrderItem orderItem = new OrderItem           
                {
                    IdOrder = o.Id,
                    IdProduct = int.Parse(key),
                    Quantity = int.Parse(stringValue)
                };

                _context.OrderItem.Add(orderItem);
                orderItems.Add(orderItem);

                Product? productUpdate = await _context.Product.FindAsync(int.Parse(key));

                if(productUpdate==null)
                    return NotFound();

                productUpdate.Quantity -= int.Parse(stringValue);

                _context.Product.Update(productUpdate);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<ActionResult> AddProduct (OrderCreateViewModel viewModel)
        { 
            var products = await _context.Product.ToListAsync();
            products = products.Where(p => p.Quantity!=0).ToList();

            if(products==null)
                return NotFound();

            Product? stockCheck = products.Find(p=>p.Id==viewModel.SelectedProductId);
            if(stockCheck==null)
                return NotFound();

            if(stockCheck.Quantity >= viewModel.Quantity)
            {
                TempData[viewModel.SelectedProductId.ToString()]=viewModel.Quantity;
            }else
            {
                viewModel.Message = "There are not enough products in stock.";
            }

            viewModel.TotalPrice = 0;

            foreach (var key in TempData.Keys)
            {
                TempData.Keep(key);

                Product ?p = products.Find(p => p.Id == int.Parse(key));
                string ?q = TempData[key]?.ToString();
                if (p != null && q!=null)
                    viewModel.TotalPrice += p.Price * int.Parse(q);
            }

            viewModel.ProductsSelectList = products.Select(p => new SelectListItem 
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                });
            viewModel.Products = products;

            return View("Create",viewModel);
        }

        // GET: Order/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Order/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TimeStamp,TotalPrice")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // GET: Order/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order != null)
            {
                _context.Order.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.Id == id);
        }
    }
}
