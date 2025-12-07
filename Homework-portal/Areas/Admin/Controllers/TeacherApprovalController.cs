using Homework_portal.Models;
using Homework_portal.Models.ViewModels;
using Homework_portal.Repository;
using Homework_portal.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Homework_portal.Hubs;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Homework_portal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Role_Admin)]
    public class TeacherApprovalController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _hub; // bildirim

        public TeacherApprovalController(IUnitOfWork unitOfWork, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager, IHubContext<NotificationHub> hub)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _hub = hub;
        }

        [HttpGet]
        public async Task<IActionResult> Pending()
        {
            // EF çevrim hatasýný engellemek için doðrudan UserManager API'sini kullan
            var list = await _userManager.GetUsersInRoleAsync(AppRoles.Role_OgretmenAday);
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string id)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) { TempData["admin_error"] = "Kullanýcý bulunamadý."; return RedirectToAction(nameof(Pending)); }
            if (!await _userManager.IsInRoleAsync(u, AppRoles.Role_OgretmenAday)) { TempData["admin_error"] = "Kullanýcý onay beklemiyor."; return RedirectToAction(nameof(Pending)); }

            await _userManager.RemoveFromRoleAsync(u, AppRoles.Role_OgretmenAday);
            await _userManager.AddToRoleAsync(u, AppRoles.Role_Ogretmen);

            // Canlý bildirim
            var fullName = ($"{u.FirstName} {u.LastName}").Trim();
            await _hub.Clients.All.SendAsync("ReceiveNotification", "Öðretmen Onayý", $"{fullName} öðretmen olarak onaylandý.");

            TempData["admin_success"] = "Öðretmen onaylandý.";
            return RedirectToAction(nameof(Pending));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string id)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) { TempData["admin_error"] = "Kullanýcý bulunamadý."; return RedirectToAction(nameof(Pending)); }
            if (!await _userManager.IsInRoleAsync(u, AppRoles.Role_OgretmenAday)) { TempData["admin_error"] = "Kullanýcý onay beklemiyor."; return RedirectToAction(nameof(Pending)); }

            var fullName = ($"{u.FirstName} {u.LastName}").Trim();
            await _userManager.DeleteAsync(u);

            // Canlý bildirim
            await _hub.Clients.All.SendAsync("ReceiveNotification", "Öðretmen Baþvurusu Reddedildi", $"{fullName} adlý kullanýcýnýn öðretmen baþvurusu reddedildi.");

            TempData["admin_success"] = "Öðretmen baþvurusu reddedildi ve kullanýcý silindi.";
            return RedirectToAction(nameof(Pending));
        }
    }
}
