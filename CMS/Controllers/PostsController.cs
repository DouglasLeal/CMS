using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Models;
using CMS.ViewModels;
using AutoMapper;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Net.Mime;
using System.Xml.Linq;

namespace CMS.Controllers
{
    [Route("")]
    [Route("posts")]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PostsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: Posts
        [HttpGet("/")]
        public async Task<IActionResult> Posts()
        {
            var applicationDbContext = _context.Posts.Include(p => p.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Posts
        [HttpGet("posts/")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Posts.Include(p => p.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Posts/Details/5
        [HttpGet("posts/detalhes/{id:int}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<PostViewModel>(post);

            return View(viewModel);
        }

        // GET: Posts/Create
        [HttpGet("posts/criar")]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("posts/criar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,ImageFile,CategoryId")] PostViewModel viewModel)
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", viewModel.CategoryId);

            Post post;

            if(viewModel.ImageFile != null)
            {
                if (!await UploadFile(viewModel.ImageFile))
                {
                    return View(viewModel);
                }
            }            

            post = _mapper.Map<Post>(viewModel);
            post.ImageUrl = viewModel.ImageFile != null ? viewModel.ImageFile.FileName : null;

            if (ModelState.IsValid)
            {
                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: Posts/Edit/5
        [HttpGet("posts/editar/{id:int}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", post.CategoryId);

            var viewModel = _mapper.Map<PostViewModel>(post);

            return View(viewModel);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("posts/editar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,ImageFile,CategoryId")] PostViewModel viewModel)
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", viewModel.CategoryId);

            if (id != viewModel.Id)
            {
                return NotFound();
            }

            var post = _mapper.Map<Post>(viewModel);
           

            if (ModelState.IsValid)
            {
                try
                {
                    if (viewModel.ImageFile != null)
                    {
                        if (!await UploadFile(viewModel.ImageFile)) return View(viewModel);

                        post.ImageUrl = viewModel.ImageFile.FileName;                  
                    }                    

                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
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

            return View(viewModel);
        }

        // GET: Posts/Delete/5
        [HttpGet("posts/excluir/{id:int}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<PostViewModel>(post);

            return View(viewModel);
        }

        // POST: Posts/Delete/5
        [HttpPost("posts/excluir/{id:int}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Posts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Posts'  is null.");
            }
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }

            if(post.ImageUrl != null)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", post.ImageUrl);
                System.IO.File.Delete(path);
            }           
                       

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }

        private async Task<bool> UploadFile(IFormFile file)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/", file.FileName);

            if (System.IO.File.Exists(path))
            {
                ModelState.AddModelError(string.Empty, "Já existe um arquivo com este nome.");
                return false;
            }

            using var fileStream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(fileStream);

            return true;
        }
    }
}
