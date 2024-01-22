using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EstoreMVC.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EstoreMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly EStoreContext _context;
        private readonly HttpClient client;
        private string ProductUrl = "http://localhost:5105/api/Products";
        private string CategoryUrl = "http://localhost:5105/api/Categories";
        public ProductsController(EStoreContext context)
        {
            _context = context;
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(ProductUrl);
                response.EnsureSuccessStatusCode();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                string strData = await response.Content.ReadAsStringAsync();
                List<Product> products = JsonSerializer.Deserialize<List<Product>>(strData, options);
                return View(products);
            }
            catch
            {
                return NoContent();
            }
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                HttpResponseMessage response = await client.GetAsync($"{ProductUrl}/{id}");
                response.EnsureSuccessStatusCode();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                string strData = await response.Content.ReadAsStringAsync();
                Product product = JsonSerializer.Deserialize<Product>(strData, options);

                if (product == null)
                {
                    return NotFound();
                }

                // Kiểm tra xem Category có giá trị null hay không
                if (product.Category == null)
                {
                    // Nếu Category là null, thực hiện yêu cầu riêng lẻ để lấy thông tin Category
                    HttpResponseMessage categoryResponse = await client.GetAsync($"{CategoryUrl}/{product.CategoryId}");
                    categoryResponse.EnsureSuccessStatusCode();
                    string categoryData = await categoryResponse.Content.ReadAsStringAsync();
                    Category category = JsonSerializer.Deserialize<Category>(categoryData, options);

                    // Gán thông tin Category vào sản phẩm
                    product.Category = category;
                }

                return View(product);
            }
            catch
            {
                return NotFound();
            }
        }

        // GET: Products/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // Lấy danh sách các danh mục từ API để hiển thị trong dropdownlist
                HttpResponseMessage categoryResponse = await client.GetAsync(CategoryUrl);
                categoryResponse.EnsureSuccessStatusCode();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                string categoryData = await categoryResponse.Content.ReadAsStringAsync();
                List<Category> categories = JsonSerializer.Deserialize<List<Category>>(categoryData, options);

                ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "CategoryName");
                return View();
            }
            catch (HttpRequestException)
            {
                // Xử lý lỗi HTTP request
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,ProductName,Weight,UnitPrice,UnitInStock")] Product product)
        {
            try
            {
                // Kiểm tra quyền truy cập (role) trước khi thực hiện tạo mới
                int userRole = HttpContext.Session.GetInt32("Role") ?? -1;
                if (userRole != 1)
                {
                    // Người dùng không có quyền truy cập để tạo mới sản phẩm
                    return Forbid();
                }

                // Lấy lại danh sách các danh mục từ API để hiển thị trong dropdownlist
                HttpResponseMessage categoryResponse = await client.GetAsync(CategoryUrl);
                categoryResponse.EnsureSuccessStatusCode();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                string categoryData = await categoryResponse.Content.ReadAsStringAsync();
                List<Category> categories = JsonSerializer.Deserialize<List<Category>>(categoryData, options);

                ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "CategoryName");

                HttpResponseMessage response = await client.PostAsJsonAsync(ProductUrl, product);
                response.EnsureSuccessStatusCode();

                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException)
            {
                // Xử lý lỗi HTTP request
                return View("Error");
            }
            catch (Exception ex)
            {
                // Xử lý lỗi khác
                return View("Error");
            }
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                HttpResponseMessage response = await client.GetAsync($"{ProductUrl}/{id}"); // Thay thế ProductUrl bằng đường dẫn thích hợp của API endpoint sản phẩm
                response.EnsureSuccessStatusCode();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                string strData = await response.Content.ReadAsStringAsync();
                Product product = JsonSerializer.Deserialize<Product>(strData, options);

                if (product == null)
                {
                    return NotFound();
                }

                ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", product.CategoryId);
                return View(product);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,ProductName,Weight,UnitPrice,UnitInStock")] Product product)
        {
            try
            {
                // Kiểm tra quyền truy cập (role) trước khi thực hiện chỉnh sửa
                int userRole = HttpContext.Session.GetInt32("Role") ?? -1;
                if (userRole != 1)
                {
                    // Người dùng không có quyền truy cập để chỉnh sửa sản phẩm
                    return Forbid();
                }

                HttpResponseMessage response = await client.PutAsJsonAsync($"{ProductUrl}/{id}", product); // Thay thế ProductUrl bằng đường dẫn thích hợp của API endpoint sản phẩm
                response.EnsureSuccessStatusCode();

                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException)
            {
                // Xử lý lỗi HTTP request
                return View("Error");
            }
            catch
            {
                // Xử lý lỗi khác
                return View("Error");
            }
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                HttpResponseMessage response = await client.GetAsync($"{ProductUrl}/{id}"); // Thay thế ProductUrl bằng đường dẫn thích hợp của API endpoint sản phẩm
                response.EnsureSuccessStatusCode();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                string strData = await response.Content.ReadAsStringAsync();
                Product product = JsonSerializer.Deserialize<Product>(strData, options);

                if (product == null)
                {
                    return NotFound();
                }

                return View(product);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Kiểm tra quyền truy cập (role) trước khi thực hiện xóa
                int userRole = HttpContext.Session.GetInt32("Role") ?? -1;
                if (userRole != 1)
                {
                    // Người dùng không có quyền truy cập để xóa sản phẩm
                    return Forbid();
                }

                HttpResponseMessage response = await client.DeleteAsync($"{ProductUrl}/{id}"); // Thay thế ProductUrl bằng đường dẫn thích hợp của API endpoint sản phẩm
                response.EnsureSuccessStatusCode();

                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException)
            {
                // Xử lý lỗi HTTP request
                return View("Error");
            }
            catch
            {
                // Xử lý lỗi khác
                return View("Error");
            }
        }

        private bool ProductExists(int id)
        {
          return (_context.Products?.Any(e => e.ProductId == id)).GetValueOrDefault();
        }
    }
}
