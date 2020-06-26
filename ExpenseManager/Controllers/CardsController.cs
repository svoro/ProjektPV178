using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ExpenseManager.Models;
using Microsoft.AspNet.Identity;

namespace ExpenseManager.Controllers
{
    public class CardsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Cards
        [Authorize]
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();
            return View( db.Cards.Where(x => x.OwnerId == userId).First());
        }

        // GET: Cards/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Card card = await db.Cards.FindAsync(id);
            if (card == null)
            {
                return HttpNotFound();
            }
            return View(card);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
