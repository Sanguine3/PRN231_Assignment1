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

    // ... Other actions ...

    [HttpGet]
    public async Task<IActionResult> Report(DateTime? fromDate, DateTime? toDate)
    {
        fromDate ??= DateTime.Today.AddMonths(-1);
        toDate ??= DateTime.Today;

        var orderDetails = await _context.OrderDetails
            .Include(od => od.Product)
            .Where(od => od.Order.OrderDate >= fromDate && od.Order.OrderDate <= toDate)
            .ToListAsync();

        ViewBag.OrderCount = orderDetails.Count();
        ViewBag.FromDate = fromDate.Value;
        ViewBag.ToDate = toDate.Value;

        return View(orderDetails);
    }
}
