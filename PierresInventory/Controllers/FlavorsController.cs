using Microsoft.AspNetCore.Mvc;
using PierresInventory.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Security.Claims;

namespace PierresInventory.Controllers
{
  public class FlavorsController : Controller
  {
    private readonly PierresInventoryContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    
    public FlavorsController(UserManager<ApplicationUser> userManager, PierresInventoryContext db)
    {
      _userManager = userManager;
      _db = db;
    }

    public async Task<ActionResult> Index()
    {
        var userId = this._userManager.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var currentUser = await _userManager.FindByIdAsync(userId);
        var userFlavors = _db.Flavors.Where(entry => entry.User.Id == currentUser.Id).ToList();
        return View(userFlavors);
    }

    public ActionResult SearchFlavor(string title)
    {
      var thisFlavor = _db.Flavors
          .Include(flavor => flavor.JoinEntries)
          .ThenInclude(join => join.Treat)
          .FirstOrDefault(flavor => flavor.FlavorName == title);
      return View(thisFlavor);
    }
    public ActionResult Create()
    {
      ViewBag.TreatId = new SelectList(_db.Treats, "TreatId", "TreatName");
      return View();
    }

    [HttpPost]
    public async Task<ActionResult> Create(Flavor flavor, int TreatId)
    {
        var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var currentUser = await _userManager.FindFirst(userId);
        flavor.User = currentUser;
        _db.Flavors.Add(flavor);
        if (TreatId != 0)
        {
          _db.TreatFlavor.Add(new TreatFlavor() { TreatId = TreatId, FlavorId = flavor.FlavorId });
        }
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

  }
}