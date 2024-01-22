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
    public class MembersController : ControllerBase
    {
        private readonly EStoreContext _context;

        public MembersController(EStoreContext context)
        {
            _context = context;
        }

        // GET: api/Members
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
        {
          if (_context.Members == null)
          {
              return NotFound();
          }
            return await _context.Members.ToListAsync();
        }

        // GET: api/Members/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Member>> GetMember(int id)
        {
          if (_context.Members == null)
          {
              return NotFound();
          }
            var member = await _context.Members.FindAsync(id);

            if (member == null)
            {
                return NotFound();
            }

            return member;
        }

        // PUT: api/Members/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMember(int id, Member member)
        {
            Member memberToUpdate = _context.Members.Find(id);
            if (memberToUpdate == null)
            {
                return NotFound();
            }

            if (memberToUpdate.Email != member.Email && _context.Members.Any(m => m.Email == member.Email))
            {
                return BadRequest("Email already exists");
            }

            memberToUpdate.Email = member.Email;
            memberToUpdate.CompanyName = member.CompanyName;
            memberToUpdate.City = member.City;
            memberToUpdate.Country = member.Country;
            memberToUpdate.Password = member.Password;

            _context.Members.Update(memberToUpdate);
            _context.SaveChanges();
            return Ok();
        }

        // POST: api/Members
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Member>> PostMember(Member member)
        {
          if (_context.Members == null)
          {
              return Problem("Entity set 'EStoreContext.Members'  is null.");
          }
            member.MemberId = 0;
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMember", new { id = member.MemberId }, member);
        }

        // DELETE: api/Members/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            if (_context.Members == null)
            {
                return NotFound();
            }
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            var associatedOrders = _context.Orders.Where(o => o.MemberId == id);
            var associatedOrderDetails = _context.OrderDetails.Where(od => associatedOrders.Any(o => o.OrderId == od.OrderId));

            if (associatedOrderDetails.Any())
            {
                return Problem("Không thể xóa thành viên có đơn đặt hàng và chi tiết đơn đặt hàng liên quan.");
            }

            // Xóa thành viên khỏi ngữ cảnh (context)
            _context.Members.Remove(member);

            // Lưu các thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool MemberExists(int id)
        {
            return (_context.Members?.Any(e => e.MemberId == id)).GetValueOrDefault();
        }
    }
}
