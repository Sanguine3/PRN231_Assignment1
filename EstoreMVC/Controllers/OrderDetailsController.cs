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
    public async Task<IActionResult> Report(DateTime? fromDate = null, DateTime? toDate = null)
    {
        // Check user role for authorization
        int userRole = HttpContext.Session.GetInt32("Role") ?? -1;
        if (userRole != 1)
        {
            // User does not have permission to view the order details report
            return Forbid();
        }

        IQueryable<OrderDetail> orderDetailsQuery = _context.OrderDetails
            .Include(od => od.Product);

        // Filter by date range if fromDate and toDate are provided
        if (fromDate != null && toDate != null)
        {
            orderDetailsQuery = orderDetailsQuery
                .Where(od => od.Order.OrderDate >= fromDate && od.Order.OrderDate <= toDate);
        }

        // Execute the query
        var orderDetails = await orderDetailsQuery.ToListAsync();

        ViewBag.OrderCount = orderDetails.Count();
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd"); // Convert to string to set the input value
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");     // Convert to string to set the input value

        // Calculate total order price
        decimal totalOrderPrice = orderDetails.Sum(od => od.Quantity * od.UnitPrice * (1 - od.Discount / 100m));
        ViewBag.TotalOrderPrice = totalOrderPrice;

        return View(orderDetails);
    }
}
