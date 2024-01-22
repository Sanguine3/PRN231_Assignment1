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
using Microsoft.AspNetCore.Identity;

namespace EstoreMVC.Controllers
{
    public class MembersController : Controller
    {
        private readonly EStoreContext _context;
        private readonly HttpClient client;
        private string MemberUrl = "http://localhost:5105/api/Members";


        public MembersController(EStoreContext context)
        {
            _context = context;
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);

        }

        // GET: Members
        public async Task<IActionResult> Index()
        {

            try
            {
                HttpResponseMessage response = await client.GetAsync(MemberUrl);
                response.EnsureSuccessStatusCode();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                string strData = await response.Content.ReadAsStringAsync();
                List<Member> members = JsonSerializer.Deserialize<List<Member>>(strData, options);
                return View(members);
            }
            catch
            {
                return NoContent();
            }

        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            string email, pass;
            var conf = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            email = conf.GetSection("Admin").GetSection("Email").Value.ToString();
            pass = conf.GetSection("Admin").GetSection("Password").Value.ToString();

            if (email == Email && pass == Password)
            {
                HttpContext.Session.SetInt32("Role", 1);
                HttpContext.Session.SetString("Email", email);
                return RedirectToAction("Index", "Home");
            }
            try
            {
                HttpResponseMessage response = await client.GetAsync(MemberUrl);
                response.EnsureSuccessStatusCode();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                string strData = await response.Content.ReadAsStringAsync();
                List<Member> members = JsonSerializer.Deserialize<List<Member>>(strData, options);
                List<Member> users = members.Where(m => m.Email == Email
                    && m.Password == Password).ToList();
                if (users.Count == 0) return View();
                else
                {
                    HttpContext.Session.SetInt32("Role", 0);
                    HttpContext.Session.SetString("Email", users[0].Email);
                    HttpContext.Session.SetInt32("MemberId", users[0].MemberId);
                    return RedirectToAction("Index", "Home");
                }
            }
            catch
            {
                return View();
            }

        }

        [HttpPost]
        public IActionResult Logout()
        {
            // Clear user-related session variables
            HttpContext.Session.Remove("Role");
            HttpContext.Session.Remove("Email");
            HttpContext.Session.Remove("MemberId");

            // Redirect to the home page or login page
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Details(int? id)
        {
            HttpResponseMessage httpResponseMessage = await client.GetAsync($"{MemberUrl}/{id}");
            var strData = await httpResponseMessage.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var member = JsonSerializer.Deserialize<Member>(strData, options);
            httpResponseMessage.EnsureSuccessStatusCode();
            return View(member);
        }

        // GET: Members/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Email,CompanyName,City,Country,Password")] Member member)
        {
            try
            {
                // Kiểm tra quyền truy cập (role) trước khi thực hiện tạo mới
                int userRole = HttpContext.Session.GetInt32("Role") ?? -1;
                if (userRole != 1)
                {
                    // Người dùng không có quyền truy cập để tạo mới thành viên
                    return Forbid();
                }

                HttpResponseMessage response = await client.PostAsJsonAsync(MemberUrl, member);
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

        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            HttpResponseMessage response = await client.GetAsync($"{MemberUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var member = await response.Content.ReadFromJsonAsync<Member>();
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Email,CompanyName,City,Country,Password")] Member member)
        {
            try
            {
                // Kiểm tra quyền truy cập (role) trước khi thực hiện chỉnh sửa
                int userRole = HttpContext.Session.GetInt32("Role") ?? -1;
                if (userRole != 1)
                {
                    // Người dùng không có quyền truy cập để chỉnh sửa thành viên
                    return Forbid();
                }

                HttpResponseMessage response = await client.PutAsJsonAsync($"{MemberUrl}/{id}", member);
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

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            HttpResponseMessage response = await client.GetAsync($"{MemberUrl}/{id}");
            var strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };
            var member = JsonSerializer.Deserialize<Member>(strData, options);
            return View(member);
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
                    // Người dùng không có quyền truy cập để xóa
                    return Forbid();
                }

                HttpResponseMessage response = await client.DeleteAsync($"{MemberUrl}/{id}");
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

        private bool MemberExists(int id)
        {
            return (_context.Members?.Any(e => e.MemberId == id)).GetValueOrDefault();
        }
    }
}
