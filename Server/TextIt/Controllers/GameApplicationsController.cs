using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TextIt.Models;
#pragma warning disable 1591
namespace TextIt.Controllers
{
    public class GameApplicationsController : Controller
    {
        private readonly ApplicationDbContext _dbContext = new ApplicationDbContext();

        // GET: GameApplications
        public async Task<ActionResult> Index()
        {
            return View(await _dbContext.GameApplications.OrderBy(x=> x.Order).ToListAsync());
        }

        // GET: GameApplications/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GameApplication gameApplication = await _dbContext.GameApplications.FindAsync(id);
            if (gameApplication == null)
            {
                return HttpNotFound();
            }
            return View(gameApplication);
        }

        // GET: GameApplications/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: GameApplications/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Order,Name,MinPlayers,MaxPlayers,GameStateType,CategoriesList,Flow")] GameApplication gameApplication)
        {
            if (ModelState.IsValid)
            {
                gameApplication.Flow.Id = Guid.NewGuid().ToString();
                gameApplication.Flow.Style = new Dictionary<string, string>
                {
                    ["background-color"] = "#555",
                };
                _dbContext.GameApplications.Add(gameApplication);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(gameApplication);
        }

        // GET: GameApplications/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var gameApplication = await _dbContext.GameApplications.Include(x => x.Flow).FirstOrDefaultAsync(x => x.Id == id.Value);
            if (gameApplication?.Flow == null)
            {
                return HttpNotFound();
            }
            return View(gameApplication);
        }

        // POST: GameApplications/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Order,MinPlayers,MaxPlayers,GameStateType,CategoriesList,Flow")] GameApplication gameApplication)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Entry(gameApplication).State = EntityState.Modified;
                _dbContext.Entry(gameApplication.Flow).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(gameApplication);
        }

        // GET: GameApplications/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GameApplication gameApplication = await _dbContext.GameApplications.FindAsync(id);
            if (gameApplication == null)
            {
                return HttpNotFound();
            }
            return View(gameApplication);
        }

        // POST: GameApplications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            GameApplication gameApplication = await _dbContext.GameApplications.FindAsync(id);
            _dbContext.GameApplications.Remove(gameApplication);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
