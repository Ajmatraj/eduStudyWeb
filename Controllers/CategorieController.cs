using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EduStudyWeb.Data;
using EduStudyWeb.Models;
using EduStudyWeb.ViewModels;
namespace EduStudyWeb.Controllers
{
    public class CategorieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategorieController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Categorie
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.categories.Include(c => c.Image);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Categorie/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.categories == null)
            {
                return NotFound();
            }

            var category = await _context.categories
                .Include(c => c.Image)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categorie/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categorie/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Image")] CategoryViewModel category)
        {
            if(string.IsNullOrEmpty(category.Name) || category.Image == null)
            {
                return View(category);
            }
            else
            {
                var categoryModel = new Category()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = category.Name,
                };

                var imgPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                var uniqueName = $"{Guid.NewGuid()}_{category.Image.FileName}";
                var filePath = Path.Combine(imgPath, uniqueName);

                var file = new FileStream(filePath, FileMode.Create);
                file.Close();

                var img = new Image()
                {
                    Id = Guid.NewGuid().ToString(),
                    imagePath = $"images/{filePath}"
                };
                _context.Add(img);
                categoryModel.ImageId = img.Id;
                _context.Add(categoryModel);

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");

            }

        }

        // GET: Categorie/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.categories == null)
            {
                return NotFound();
            }

            var category = await _context.categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(new CategoryViewModel { Id = category.Id, Name = category.Name});
        }

        // POST: Categorie/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Name,Image")] CategoryViewModel category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(category.Name))
            {
                return View(category);
            }
            var categoryModel = await _context.categories.FindAsync(id);
            if (categoryModel == null)
            {
                return NotFound();
            }
            categoryModel.Name = category.Name;

            if (category.Image != null)
            {
                // Remove the old image if it exists
                if (!string.IsNullOrEmpty(categoryModel.Image?.imagePath))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, categoryModel.Image.imagePath);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                //file path manage for image upload and save in root file images.
                var imgPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");

                var uniqueName = $"{Guid.NewGuid()}_{category.Image.FileName}";
                var filePath = Path.Combine(imgPath, uniqueName);
                var file = new FileStream(filePath, FileMode.Create);

                //store the file path.
                await category.Image.CopyToAsync(file);

                file.Close();
                var img = new Image()
                {
                    Id = Guid.NewGuid().ToString(),
                    imagePath = $"images/{filePath}"
                };

                _context.Add(img);
                categoryModel.ImageId = img.Id;

            }
            _context.Update(categoryModel);
            await _context.SaveChangesAsync();
            return RedirectToAction("index");

        }

        // GET: Categorie/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.categories
                .Include(c => c.Image)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categorie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var category = await _context.categories.FindAsync(id);
            if (category != null)
            {
                _context.categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(string id)
        {
            return _context.categories.Any(e => e.Id == id);
        }
    }
}
