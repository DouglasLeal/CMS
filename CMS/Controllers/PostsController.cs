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
using Microsoft.AspNetCore.Authorization;
using CMS.Interfaces;

namespace CMS.Controllers
{
    [Authorize]
    [Route("")]
    [Route("posts")]
    public class PostsController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public PostsController(IPostRepository postRepository, ICategoryRepository categoryRepository, IMapper mapper)
        {
            _postRepository = postRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [AllowAnonymous]
        // GET: Posts
        [HttpGet("/")]
        public async Task<IActionResult> Posts()
        {
            var posts = await _postRepository.List();
            var viewModels = _mapper.Map<IEnumerable<PostViewModel>>(posts);
            return View(viewModels);
        }

        [AllowAnonymous]
        [HttpGet("/post/{id:int}")]
        public async Task<IActionResult> Post(int? id)
        {
            if (id == null || _postRepository == null)
            {
                return NotFound();
            }

            var post = await _postRepository.GetById(id);

            if (post == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<PostViewModel>(post);

            return View(viewModel);
        }

        // GET: Posts
        [HttpGet("posts/")]
        public async Task<IActionResult> Index()
        {
            var posts = await _postRepository.List();
            var viewModels = _mapper.Map<IEnumerable<PostViewModel>>(posts);

            return View(viewModels);
        }

        // GET: Posts/Details/5
        [HttpGet("posts/detalhes/{id:int}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _postRepository == null)
            {
                return NotFound();
            }

            var post = await _postRepository.GetById(id);

            if (post == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<PostViewModel>(post);

            return View(viewModel);
        }

        // GET: Posts/Create
        [HttpGet("posts/criar")]
        public async Task<IActionResult> Create()
        {
            ViewData["CategoryId"] = new SelectList(await _categoryRepository.List(), "Id", "Name");
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("posts/criar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,ImageFile,CategoryId")] PostViewModel viewModel)
        {
            ViewData["CategoryId"] = new SelectList(await _categoryRepository.List(), "Id", "Name", viewModel.CategoryId);

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
                await _postRepository.Create(post);
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: Posts/Edit/5
        [HttpGet("posts/editar/{id:int}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _postRepository == null)
            {
                return NotFound();
            }

            var post = await _postRepository.GetById(id);
            if (post == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(await _categoryRepository.List(), "Id", "Name", post.CategoryId);

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
            ViewData["CategoryId"] = new SelectList(await _categoryRepository.List(), "Id", "Name", viewModel.CategoryId);

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

                    await _postRepository.Update(post);
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
            if (id == null || _postRepository == null)
            {
                return NotFound();
            }

            var post = await _postRepository.GetById(id);
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
            if (_postRepository == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Posts'  is null.");
            }
            var post = await _postRepository.GetById(id);
            if (post != null)
            {
                await _postRepository.Delete(post);
            }

            if(post.ImageUrl != null)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", post.ImageUrl);
                System.IO.File.Delete(path);
            }           
                       
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _postRepository.PostExists(id);
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
