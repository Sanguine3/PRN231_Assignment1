using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstoreAPI.Models;

namespace EstoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly EStoreContext _context;

        public ProductsController(EStoreContext context)
        {
            _context = context;
        }
        [HttpGet("search")]
        public async Task<ActionResult<List<Product>>> searchByNameAndPrice([FromQuery] string? name, [FromQuery] int price)
        {
            List<Product> products = new List<Product>();
            if (!String.IsNullOrEmpty(name))
            {
                products = _context.Products.Where(p=>p.ProductName.Contains(name)).ToList();
                if (price > 0)
                {
                    products = products.Where(p=>p.UnitPrice <= price).ToList();
                }
            }
            else
            {
                products = _context.Products.ToList();
                if(price > 0)
                {
                    products = products.Where(p => p.UnitPrice <= price).ToList();
                }
            }
            return products;

        }
        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
          if (_context.Products == null)
          {
              return NotFound();
          }
            return await _context.Products.ToListAsync();
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
          if (_context.Products == null)
          {
              return NotFound();
          }
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {

            Product proUpdate = _context.Products.Find(id);
            if (proUpdate == null)
            {
                return NotFound();
            }

            if (proUpdate.ProductName != product.ProductName && _context.Products.Any(m => m.ProductName == product.ProductName))
            {
                return BadRequest("Product already exists");
            }

            proUpdate.CategoryId = product.CategoryId;
            proUpdate.ProductName = product.ProductName;
            proUpdate.Weight = product.Weight;
            proUpdate.UnitPrice = product.UnitPrice;
            proUpdate.UnitInStock = product.UnitInStock;

            _context.Products.Update(proUpdate);
            _context.SaveChanges();
            return Ok();
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
          if (_context.Products == null)
          {
              return Problem("Entity set 'EStoreContext.Products'  is null.");
          }
            
            Product product1 = new Product
            {
                ProductName = product.ProductName,
                CategoryId = product.CategoryId,
                UnitPrice = product.UnitPrice,
                UnitInStock = product.UnitInStock,
                Weight = product.Weight
            };
            _context.Products.Add(product1);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return (_context.Products?.Any(e => e.ProductId == id)).GetValueOrDefault();
        }
    }
}
