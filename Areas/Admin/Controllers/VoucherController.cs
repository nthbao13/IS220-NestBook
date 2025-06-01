using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookNest.Models.MappingDBModel;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using BookNest.Data;
using Microsoft.AspNetCore.Authorization;

namespace BookNest.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class VoucherController : Controller
    {
        private readonly ApplicationDbContext _context; // Thay YourDbContext bằng DbContext của bạn

        public VoucherController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Voucher
        public async Task<IActionResult> Index(string searchString, string status, string type, int page = 1)
        {
            int pageSize = 10;
            int pageNumber = page > 0 ? page : 1;

            var vouchers = _context.Vouchers.AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(searchString))
            {
                vouchers = vouchers.Where(v => v.VoucherCode.Contains(searchString));
                ViewBag.SearchString = searchString;
            }

            // Filter by status
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "valid")
                {
                    vouchers = vouchers.Where(v => v.ExpiredAt >= DateTime.Now && v.UsedCount < v.UsageLimit);
                }
                else if (status == "expired")
                {
                    vouchers = vouchers.Where(v => v.ExpiredAt < DateTime.Now);
                }
                else if (status == "usedUp")
                {
                    vouchers = vouchers.Where(v => v.UsedCount >= v.UsageLimit);
                }
                ViewBag.Status = status;
            }

            // Filter by type
            if (!string.IsNullOrEmpty(type) && bool.TryParse(type, out bool voucherType))
            {
                vouchers = vouchers.Where(v => v.Type == voucherType);
                ViewBag.Type = type;
            }

            // Pagination
            var totalItems = await vouchers.CountAsync();
            var pagedVouchers = await vouchers
                .OrderBy(v => v.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            return View(pagedVouchers);
        }

        // GET: Voucher/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(m => m.Id == id);

            if (voucher == null)
            {
                return NotFound();
            }

            return View(voucher);
        }

        // POST: Voucher/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Voucher voucher)
        {
            if (id != voucher.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validate ExpiredAt > CreateAt
                    if (voucher.ExpiredAt <= voucher.CreateAt)
                    {
                        ModelState.AddModelError("ExpiredAt", "Ngày hết hạn phải lớn hơn ngày tạo.");
                        return View("Details", voucher);
                    }

                    // Validate Value based on Type
                    if (voucher.Type == true && (voucher.Value < 0 || voucher.Value > 100))
                    {
                        ModelState.AddModelError("Value", "Giá trị giảm phần trăm phải từ 0 đến 100.");
                        return View("Details", voucher);
                    }

                    _context.Update(voucher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoucherExists(voucher.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View("Details", voucher);
        }

        // GET: Voucher/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Voucher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Voucher voucher)
        {
            if (ModelState.IsValid)
            {
                // Validate ExpiredAt > CreateAt
                if (voucher.ExpiredAt <= voucher.CreateAt)
                {
                    ModelState.AddModelError("ExpiredAt", "Ngày hết hạn phải lớn hơn ngày tạo.");
                    return View(voucher);
                }

                // Validate Value based on Type
                if (voucher.Type == true && (voucher.Value < 0 || voucher.Value > 100))
                {
                    ModelState.AddModelError("Value", "Giá trị giảm phần trăm phải từ 0 đến 100.");
                    return View(voucher);
                }

                voucher.UsedCount = 0; // Default value
                voucher.CreateAt = voucher.CreateAt ?? DateTime.Now; // Default to now
                _context.Add(voucher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(voucher);
        }

        // POST: Voucher/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var voucher = await _context.Vouchers.Include(v => v.Orders).FirstOrDefaultAsync(v => v.Id == id);
            if (voucher == null)
            {
                return NotFound();
            }
            if (voucher.Orders.Any())
            {
                TempData["Error"] = "Không thể xóa voucher đã được sử dụng trong đơn hàng.";
                return RedirectToAction(nameof(Index));
            }
            _context.Vouchers.Remove(voucher);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VoucherExists(int id)
        {
            return _context.Vouchers.Any(e => e.Id == id);
        }
    }
}