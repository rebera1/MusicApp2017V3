using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MusicApp2017.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace MusicApp2017.Controllers
{
    public class AlbumsController : Controller
    {
        private readonly MusicDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public AlbumsController(MusicDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Albums
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var musicDbContext = _context.Albums.Include(a => a.Artist).Include(a => a.Genre).OrderByDescending(a => a.GenreID == currentUser.GenreID);

                return View(await musicDbContext.ToListAsync());
            }
            else
            {
                var musicDbContext = _context.Albums.Include(a => a.Artist).Include(a => a.Genre);
                return View(await musicDbContext.ToListAsync());
            }
        }


        // GET: Albums/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                Response.StatusCode = 404;
                return NotFound();
            }

            var albumContext = _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Genre);
            var album = await albumContext.Where(m => m.AlbumID == id).ToListAsync();
            if (album == null)
            {
                Response.StatusCode = 404;
                return NotFound();
            }
            var ratingContext = _context.AlbumRatings
             .Include(a => a.Album)
             .Include(a => a.User);
            var ratings = ratingContext.Where(a => a.AlbumID == (int)id);
            if (ratings.ToList().Count != 0)
            {
                var rating = 0;
                foreach (var r in ratings.ToList())
                {
                    rating += r.Rating;
                }
                ViewData["Rating"] = rating / ratings.ToList().Count + "/5";
            }
            else
            {
                ViewData["Rating"] = "Not Rated";
            }
            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        // GET: Albums/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["ArtistID"] = new SelectList(_context.Artists, "ArtistID", "Name");
            ViewData["GenreID"] = new SelectList(_context.Genres, "GenreID", "Name");
            return View();
        }

        // POST: Albums/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlbumID,Title,ArtistID,GenreID,Likes")] Album album)
        {
            if (ModelState.IsValid)
            {
                _context.Add(album);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["ArtistID"] = new SelectList(_context.Artists, "ArtistID", "Name", album.ArtistID);
            ViewData["GenreID"] = new SelectList(_context.Genres, "GenreID", "Name", album.GenreID);
            return View(album);
        }

        // GET: Albums/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albums.SingleOrDefaultAsync(m => m.AlbumID == id);
            if (album == null)
            {
                return NotFound();
            }
            ViewData["ArtistID"] = new SelectList(_context.Artists, "ArtistID", "Name", album.ArtistID);
            ViewData["GenreID"] = new SelectList(_context.Genres, "GenreID", "Name", album.GenreID);
            return View(album);
        }

        // POST: Albums/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlbumID,Title,ArtistID,GenreID,Likes")] Album album)
        {
            if (id != album.AlbumID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(album);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlbumExists(album.AlbumID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            ViewData["ArtistID"] = new SelectList(_context.Artists, "ArtistID", "Name", album.ArtistID);
            ViewData["GenreID"] = new SelectList(_context.Genres, "GenreID", "Name", album.GenreID);
            return View(album);
        }

        // GET: Albums/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Genre)
                .SingleOrDefaultAsync(m => m.AlbumID == id);
            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        // POST: Albums/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var album = await _context.Albums.SingleOrDefaultAsync(m => m.AlbumID == id);
            _context.Albums.Remove(album);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool AlbumExists(int id)
        {
            return _context.Albums.Any(e => e.AlbumID == id);
        }

        private bool AlbumExists(string title)
        {
            return _context.Albums.Any(e => e.Title == title);
        }

        public async Task<IActionResult> Rate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var userID = _userManager.GetUserId(User);
            if (_context.AlbumRatings.SingleOrDefault(m => (m.UserID == userID) && (m.AlbumID == id)) == null)
            {
                var album = await _context.Albums.SingleOrDefaultAsync(m => m.AlbumID == id);
                if (album == null)
                {
                    return NotFound();
                }
                ViewData["AlbumID"] = album.AlbumID;
                return View();
            }
            return NotFound("You've already rated this album.");
        }

        [HttpPost]
        public IActionResult Rate(int id, int rating)
        {
            var userID = _userManager.GetUserId(User);
            if (_context.AlbumRatings.SingleOrDefault(m => (m.UserID == userID) && (m.AlbumID == id)) == null)
            {
                var Rating = new AlbumRating { UserID = userID, AlbumID = id, Rating = rating };
                _context.Add(Rating);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            var album = _context.Albums.SingleOrDefault(m => m.AlbumID == id);
            if (album == null)
            {
                return NotFound();
            }
            ViewData["Title"] = album.Title;
            return View(rating);
        }
    }
}
