
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ComicSystem.Models;

public class ComicBooksController : Controller
{
    private readonly ComicDbContext _context;

    public ComicBooksController(ComicDbContext context)
    {
        _context = context;
    }

    // GET: COMICBOOKS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.ComicBooks.ToListAsync());
    }

    // GET: COMICBOOKS/Details/5
    public async Task<IActionResult> Details(int? comicbookid)
    {
        if (comicbookid == null)
        {
            return NotFound();
        }

        var comicbook = await _context.ComicBooks
            .FirstOrDefaultAsync(m => m.ComicBookID == comicbookid);
        if (comicbook == null)
        {
            return NotFound();
        }

        return View(comicbook);
    }

    // GET: COMICBOOKS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: COMICBOOKS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ComicBookID,Title,Author,PricePerDay,RentalDetails")] ComicBook comicbook)
    {
        if (ModelState.IsValid)
        {
            _context.Add(comicbook);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(comicbook);
    }

    // GET: COMICBOOKS/Edit/5
    public async Task<IActionResult> Edit(int? comicbookid)
    {
        if (comicbookid == null)
        {
            return NotFound();
        }

        var comicbook = await _context.ComicBooks.FindAsync(comicbookid);
        if (comicbook == null)
        {
            return NotFound();
        }
        return View(comicbook);
    }

    // POST: COMICBOOKS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? comicbookid, [Bind("ComicBookID,Title,Author,PricePerDay,RentalDetails")] ComicBook comicbook)
    {
        if (comicbookid != comicbook.ComicBookID)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(comicbook);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ComicBookExists(comicbook.ComicBookID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(comicbook);
    }

    // GET: COMICBOOKS/Delete/5
    public async Task<IActionResult> Delete(int? comicbookid)
    {
        if (comicbookid == null)
        {
            return NotFound();
        }

        var comicbook = await _context.ComicBooks
            .FirstOrDefaultAsync(m => m.ComicBookID == comicbookid);
        if (comicbook == null)
        {
            return NotFound();
        }

        return View(comicbook);
    }

    // POST: COMICBOOKS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? comicbookid)
    {
        var comicbook = await _context.ComicBooks.FindAsync(comicbookid);
        if (comicbook != null)
        {
            _context.ComicBooks.Remove(comicbook);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ComicBookExists(int? comicbookid)
    {
        return _context.ComicBooks.Any(e => e.ComicBookID == comicbookid);
    }
}
