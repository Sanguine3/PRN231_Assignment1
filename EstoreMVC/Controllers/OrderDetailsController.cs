// EstoreMVC/Controllers/OrderDetailsController.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using EstoreMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class OrderDetailsController : Controller
{
    private readonly EStoreContext _context;

    public OrderDetailsController(EStoreContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Report(DateTime? fromDate, DateTime? toDate)
    {
        fromDate ??= DateTime.Today.AddMonths(-1);
        toDate ??= DateTime.Today;

        // Check user role for authorization
        int userRole = HttpContext.Session.GetInt32("Role") ?? -1;
        if (userRole != 1)
        {
            // User does not have permission to view the order details report
            return Forbid();
        }

        var orderDetails = await _context.OrderDetails
            .Include(od => od.Product)
            .Where(od => od.Order.OrderDate >= fromDate && od.Order.OrderDate <= toDate)
            .ToListAsync();

        ViewBag.OrderCount = orderDetails.Count();
        ViewBag.FromDate = fromDate.Value;
        ViewBag.ToDate = toDate.Value;

        // Calculate total order price
        decimal totalOrderPrice = orderDetails.Sum(od => od.Quantity * od.UnitPrice * (1 - od.Discount / 100m));
        ViewBag.TotalOrderPrice = totalOrderPrice;

        return View(orderDetails);
    }
}
