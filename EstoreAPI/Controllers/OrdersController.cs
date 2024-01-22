using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstoreAPI.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EstoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : Controller
    {
        private readonly EStoreContext _context;
        //private readonly HttpClient client;
        //private string OrderUrl = "http://localhost:5105/api/Orders/OrdersByMember";
        //public OrdersController(EStoreContext context)
        //{
        //    _context = context;
        //    client = new HttpClient();
        //    var contentType = new MediaTypeWithQualityHeaderValue("application/json");
        //    client.DefaultRequestHeaders.Accept.Add(contentType);
        //}

        //public async Task<IActionResult> Index(int? id)
        //{
        //    try
        //    {
        //        HttpResponseMessage response = await client.GetAsync($"{OrderUrl}/{id}");
        //        response.EnsureSuccessStatusCode();
        //        var options = new JsonSerializerOptions
        //        {
        //            PropertyNameCaseInsensitive = true
        //        };
        //        string strData = await response.Content.ReadAsStringAsync();
        //        List<OrderDTO> orders = JsonSerializer.Deserialize<List<OrderDTO>>(strData, options);
        //        return View(orders);
        //    }
        //    catch
        //    {
        //        return NoContent();
        //    }
        //}

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
          if (_context.Orders == null)
          {
              return NotFound();
          }
            return await _context.Orders.ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
          if (_context.Orders == null)
          {
              return NotFound();
          }
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // GET: api/Orders/5
        [HttpGet("OrdersByMember/{id}")]
        public async Task<ActionResult<List<OrderDTO>>> GetMembersOrder(int id)
        {
            List<Order> orders = new List<Order>();
            List<OrderDetail> details = new List<OrderDetail>();
            List<OrderDTO> model = new List<OrderDTO>();

            if (_context.Orders == null)
            {
                return NotFound();
            }
            foreach(Order order in _context.Orders)
            {
                if (order.MemberId == id)
                {
                    orders.Add(order);
                }
            }
            foreach(Order order in orders)
            {
                foreach (OrderDetail detail in _context.OrderDetails.Include(x => x.Product))
                {
                    model.Add(new OrderDTO
                    {
                        OrderDetailId = detail.OrderDetailId,
                        MemberId = order.MemberId,
                        ProductName = detail.Product.ProductName,
                        UnitPrice = detail.UnitPrice,
                        Quantity = detail.Quantity,
                        Discount = detail.Discount,
                        OrderDate = order.OrderDate,
                        RequiredDate = order.RequiredDate,
                        ShippedDate = order.ShippedDate,
                        Freight = order.Freight
                    });
                }
            }

            return model;
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.OrderId)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Orders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
          if (_context.Orders == null)
          {
              return Problem("Entity set 'EStoreContext.Orders'  is null.");
          }
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { id = order.OrderId }, order);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            if (_context.Orders == null)
            {
                return NotFound();
            }
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderExists(int id)
        {
            return (_context.Orders?.Any(e => e.OrderId == id)).GetValueOrDefault();
        }
    }
}
