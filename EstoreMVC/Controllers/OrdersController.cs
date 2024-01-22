using EstoreMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace EstoreMVC.Controllers
{
    public class OrdersController : Controller
    {
        private HttpClient client;
        private string OrderURL;
        public OrdersController()
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            OrderURL = "http://localhost:5105/api/Orders";
        }
        public async Task<IActionResult> Index()
        {
            HttpResponseMessage response = await client.GetAsync(OrderURL);
            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<Order> listOrder = JsonSerializer.Deserialize<List<Order>>(strData, options);
            return View(listOrder);
        }
        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            HttpResponseMessage response = await client.GetAsync($"{OrderURL}/{id}");
            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            var product = JsonSerializer.Deserialize<Order>(strData, options);
            return View(product);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            try
            {
                HttpResponseMessage responseMember = await client.GetAsync($"http://localhost:5105/api/Members/{order.MemberId}");
                var strData = await responseMember.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                var member = JsonSerializer.Deserialize<Member>(strData, options);
                Order newOrder = order;
                newOrder.Member = member;

                var orderJson = JsonSerializer.Serialize(newOrder);
                var content = new StringContent(orderJson, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(OrderURL, content);

                response.EnsureSuccessStatusCode(); // Ensure that the API request was successful

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Handle exceptions, log the error, or return an error view
                ModelState.AddModelError("", "An error occurred while creating the category.");
                return View(order);
            }
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            HttpResponseMessage httpResponseMessage = await client.GetAsync($"{OrderURL}/{id}");
            string strData = await httpResponseMessage.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            var product = JsonSerializer.Deserialize<Order>(strData, options);
            return View(product);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,MemberId,OrderDate,RequiredDate,ShippedDate,Freight")] Order order)
        {
            try
            {
                HttpResponseMessage responseMember = await client.GetAsync($"http://localhost:5105/api/Members/{order.MemberId}");
                var strData = await responseMember.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                var member = JsonSerializer.Deserialize<Member>(strData, options);
                Order newOrder = order;
                newOrder.Member = member;

                var orderData = JsonSerializer.Serialize<Order>(newOrder);
                var content = new StringContent(orderData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync($"{OrderURL}/{id}", content);
                response.EnsureSuccessStatusCode();
                return RedirectToAction("Index");

            }
            catch (Exception)
            {
                return View(order);
            }
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            HttpResponseMessage response = await client.GetAsync($"{OrderURL}/{id}");
            var strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };
            var category = JsonSerializer.Deserialize<Order>(strData, options);
            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                HttpResponseMessage response = await client.DeleteAsync($"{OrderURL}/{id}");
                response.EnsureSuccessStatusCode(); // Ensure that the API request was successful

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while deleting the category.");
                return RedirectToAction("Details", new { id }); // Redirect to Details page with the id parameter
            }
        }
    }
}
