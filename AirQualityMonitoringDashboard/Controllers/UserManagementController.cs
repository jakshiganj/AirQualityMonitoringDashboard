using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AirQualityMonitoringDashboard.Models; // Adjust namespace as needed
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class UserManagementController : Controller
{
    private readonly UserManager<User> _userManager;

    public UserManagementController(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    // GET: UserManagement/Index
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users;
        return View(await users.ToListAsync());
    }

    // GET: UserManagement/Edit/{id}
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        return View(user);
    }

    // POST: UserManagement/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, User user)
    {
        if (id != user.Id)
            return NotFound();

        var existingUser = await _userManager.FindByIdAsync(id);
        if (existingUser == null)
            return NotFound();

        existingUser.FullName = user.FullName;
        existingUser.Email = user.Email;

        var result = await _userManager.UpdateAsync(existingUser);

        if (result.Succeeded)
            return RedirectToAction(nameof(Index));

        return View(existingUser);
    }

    // POST: UserManagement/Delete/{id}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
            return RedirectToAction(nameof(Index));

        return View(user);
    }
}
