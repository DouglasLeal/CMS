using AutoMapper;
using CMS.Interfaces;
using CMS.Models;
using CMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Slugify;

namespace CMS.Controllers
{
    [Authorize]
    [Route("")]
    public class PostsController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ISlugHelper _slugHelper;

        public PostsController(
            IPostRepository postRepository, 
            ICategoryRepository categoryRepository, 
            IMapper mapper,
            ISlugHelper slugHelper)
        {
            _postRepository = postRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _slugHelper = slugHelper;
        }

        [AllowAnonymous]
        [HttpGet("/")]
        [HttpGet("/posts")]
        [HttpGet("/posts/categorias/{slug}")]
        public async Task<IActionResult> Posts(string? slug)
        {
            IList<Post> posts;

            if(slug != null)
            {
                posts = await _postRepository.List(slug);
            }
            else
            {
                posts = await _postRepository.List();
            }

            var viewModels = _mapper.Map<IEnumerable<PostViewModel>>(posts);

            var categories = await _categoryRepository.List();
            var categoriesViewModels = _mapper.Map<IEnumerable<CategoryViewModel>>(categories);
            ViewData["Categories"] = categoriesViewModels;

            return View(viewModels);
        }

        [AllowAnonymous]
        [HttpGet("/post/{slug}")]
        public async Task<IActionResult> Post(string slug)
        {
            if (slug == null || _postRepository == null)
            {
                return NotFound();
            }

            var post = await _postRepository.GetBySlug(slug);

            if (post == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<PostViewModel>(post);

            return View(viewModel);
        }

        [HttpGet("/admin/posts/")]
        public async Task<IActionResult> Index()
        {
            var posts = await _postRepository.List();
            var viewModels = _mapper.Map<IEnumerable<PostViewModel>>(posts);

            return View(viewModels);
        }

        [HttpGet("/admin/posts/detalhes/{id:int}")]
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

        [HttpGet("/admin/posts/criar")]
        public async Task<IActionResult> Create()
        {
            ViewData["CategoryId"] = new SelectList(await _categoryRepository.List(), "Id", "Name");
            return View();
        }

        [HttpPost("/admin/posts/criar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,ImageFile,CategoryId")] PostViewModel viewModel)
        {
            ViewData["CategoryId"] = new SelectList(await _categoryRepository.List(), "Id", "Name", viewModel.CategoryId);

            Post post;

            string imageUrl = "";

            if(viewModel.ImageFile != null)
            {
                var guid = Guid.NewGuid();
                imageUrl = $"{guid}_{viewModel.ImageFile.FileName}";

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/", imageUrl);

                if (!await UploadFile(viewModel.ImageFile, path))
                {
                    return View(viewModel);
                }
            }            

            post = _mapper.Map<Post>(viewModel);
            post.ImageUrl = viewModel.ImageFile != null ? imageUrl : null;
            post.Slug = _slugHelper.GenerateSlug(post.Title);

            if (ModelState.IsValid)
            {
                await _postRepository.Create(post);
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        [HttpGet("/admin/posts/editar/{id:int}")]
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

        [HttpPost("/admin/posts/editar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,ImageFile,ImageUrl,CategoryId")] PostViewModel viewModel)
        {
            ViewData["CategoryId"] = new SelectList(await _categoryRepository.List(), "Id", "Name", viewModel.CategoryId);

            if (id != viewModel.Id)
            {
                return NotFound();
            }

            var post = _mapper.Map<Post>(viewModel);

            string imageUrl;

            if (ModelState.IsValid)
            {
                try
                {
                    if (viewModel.ImageFile != null)
                    {
                        var guid = Guid.NewGuid();
                        imageUrl = $"{guid}_{viewModel.ImageFile.FileName}";

                        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/", imageUrl);

                        if (!await UploadFile(viewModel.ImageFile, path)) return View(viewModel);

                        post.ImageUrl = imageUrl;
                        post.Slug = _slugHelper.GenerateSlug(post.Title);
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

        [HttpGet("/admin/posts/excluir/{id:int}")]
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

        [HttpPost("/admin/posts/excluir/{id:int}"), ActionName("Delete")]
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

        private async Task<bool> UploadFile(IFormFile file, string path)
        {
            

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
