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
using System.IO;
using System.Data.Entity.Validation;
using System.Text.RegularExpressions;
using System.Text;

namespace ExpenseManager.Controllers
{
    public class PaymentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Payments
        [Authorize]
        public async Task<ActionResult> Index()
        {
            ViewBag.Card = GetUserCardId();
            var cardId = GetUserCardId();
            return View(await db.Payments.Where(x => x.CardId == cardId).ToListAsync());
        }

        // GET: Payments/Incomes
        public async Task<ActionResult> Incomes()
        {
            ViewBag.Card = GetUserCardId();
            var cardId = GetUserCardId();
            return View(await db.Payments.Where(x => x.CardId == cardId & x.Price >= 0).ToListAsync());
        }

        // GET: Payments/Expenses
        public async Task<ActionResult> Expenses()
        {
            ViewBag.Card = GetUserCardId();
            var cardId = GetUserCardId();
            return View(await db.Payments.Where(x => x.CardId == cardId & x.Price < 0).ToListAsync());
        }

        // GET: Payments/Details/5
        [Authorize]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = await db.Payments.FindAsync(id);
            Dispose();
            if (payment == null)
            {
                return HttpNotFound();
            }
            return View(payment);
        }

        // GET: Payments/CreateIncome
        [Authorize]
        public ActionResult CreateIncome(int? id)
        {
            if (id == null)
            {
                id = GetUserCardId();
            }
            return View();
        }
        // GET: Payments/Import
        [Authorize]
        public ActionResult Import()
        {

            return View();
        }
        // GET: Payments/Import
        [Authorize]
        public ActionResult Export()
        {
            return View();
        }
        [Authorize]
        // GET: Payments/CreateExpense
        public ActionResult CreateExpense(int? id)
        {
            if (id == null)
            {
                id = GetUserCardId();
            }
            return View();
        }

        // POST: Payments/CreateIncome
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateIncome([Bind(Include = "Id,Name,Date,Price,Card")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                var card = GetUserCard();
                payment.CardId = GetUserCardId();
                UpdateCardbalance(payment.Price);
                db.Payments.Add(payment);
                await db.SaveChangesAsync();
                Dispose();
                return RedirectToAction("Index");
            }

            return View(payment);
        }
        // POST: Payments/CreateExpense
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateExpense([Bind(Include = "Id,Name,Date,Price,Card")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                payment.Price = (-1) * payment.Price;
                payment.CardId = GetUserCardId();

                if (!CardHasSufficient(payment.Price))
                {
                    ModelState.AddModelError("Price", "Your Card balance is not enough for this expense.");
                    return View(payment);
                }
                UpdateCardbalance(payment.Price);
                db.Payments.Add(payment);
                await db.SaveChangesAsync();
                Dispose();
                return RedirectToAction("Index");
            }

            return View(payment);
        }
        // POST: Payments/Import
        public async Task<ActionResult> ImportAsync()
        {
            var file = Request.Files[0];
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                file.SaveAs(path);
                using (var reader = new StreamReader(path))
                {
                    while (!reader.EndOfStream)
                    {
                        var columns = reader.ReadLine().Split(",".ToCharArray(), 3);
                        if (columns.Length == 3)
                        {

                            if (!double.TryParse(columns[2], out _))
                            {
                                ViewBag.ErrorMessage = "Failed to parse Price  : " + columns[2];
                                return View("Import");
                            }
                            if (Regex.Match(columns[1], "[0-9]{2}.[0-9]{2}.[0-9]{4}") == null)
                            {
                                ViewBag.ErrorMessage = "failed to parse date ( format is DD.MM.YYYY) : " + columns[1];
                                return View("Import");
                            }
                            var payment = new Payment { Name = columns[0], Date = columns[1], Price = double.Parse(columns[2]), CardId = GetUserCardId() };

                            UpdateCardbalance(payment.Price);
                            db.Payments.Add(payment);
                        }
                    }
                }

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbEntityValidationException e)
                {
                    string error = "";
                    foreach (var eve in e.EntityValidationErrors)
                    {

                        error += ("Entity of type {0} in state {1} has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State).ToString();
                        foreach (var ve in eve.ValidationErrors)
                        {
                            error += ("- Property: {0}, Error: {1}", ve.PropertyName, ve.ErrorMessage).ToString();
                        }
                    }
                    Dispose();
                    throw new DbEntityValidationException(error);
                }
                Dispose();
                return RedirectToAction("Index");
            }
            Dispose();

            ViewBag.ErrorMessage = "Empty or null File uploaded";
            return View("Import");
        }

        //POST: /Payments/Export
        [HttpPost]
        public async Task<FileResult> ExportAsync()
        {
            StringBuilder builder = new StringBuilder();
            var cardId = GetUserCardId();
            var payments = await db.Payments.Where(x => x.CardId == cardId).ToListAsync();
            for (int i = 0; i < payments.Count; i++)
            {
                builder.Append(payments[i]);
                builder.Append("\r\n");

            }
            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "export.csv");
        }

        private int GetUserCardId()
        {
            var userId = User.Identity.GetUserId();
            return db.Cards.Where(x => x.OwnerId == userId).First().Id;
        }

        private Card GetUserCard()
        {
            var userId = User.Identity.GetUserId();
            return db.Cards.Where(x => x.OwnerId == userId).First();

        }

        private bool CardHasSufficient(double price)
        {
            var card = GetUserCard();
            return card.Balance > price;
        }

        private void UpdateCardbalance(double price)
        {
            GetUserCard().Balance += price;
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
