using AutoMapper;
using CMS.Interfaces;
using CMS.Models;
using CMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Slugify;

namespace CMS.Controllers
{
    [Authorize]
    [Route("admin/categorias")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _repository;
        private readonly IMapper _mapper;
        private readonly ISlugHelper _slugHelper;

        public CategoriesController(ICategoryRepository repository, IMapper mapper, ISlugHelper slugHelper)
        {
            _repository = repository;
            _mapper = mapper;
            _slugHelper = slugHelper;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var categories = await _repository.List();
            var vielModels = _mapper.Map<IEnumerable<CategoryViewModel>>(categories);

            return View(vielModels);
        }

        [HttpGet("detalhes/{id:int}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _repository == null)
            {
                return NotFound();
            }

            var category = await _repository.GetById(id);

            if (category == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<CategoryViewModel>(category);

            return View(viewModel);
        }

        [HttpGet("novo")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("novo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Slug")] CategoryViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var category = _mapper.Map<Category>(viewModel);

                if(category.Slug == null)
                {
                    category.Slug = _slugHelper.GenerateSlug(category.Name);
                }
                else
                {
                    category.Slug = _slugHelper.GenerateSlug(category.Slug);
                }

                if (!CheckNameAndSlug(category)) return View(viewModel);

                await _repository.Create(category);
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        [HttpGet("editar/{id:int}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _repository == null)
            {
                return NotFound();
            }

            var category = await _repository.GetById(id);
            if (category == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<CategoryViewModel>(category);

            return View(viewModel);
        }

        [HttpPost("editar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Slug")] CategoryViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var category = _mapper.Map<Category>(viewModel);

                    if (category.Slug == null)
                    {
                        category.Slug = _slugHelper.GenerateSlug(category.Name);
                    }
                    else
                    {
                        category.Slug = _slugHelper.GenerateSlug(category.Slug);
                    }

                    if (!CheckNameAndSlug(category)) return View(viewModel);

                    await _repository.Update(category);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(viewModel.Id))
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

        [HttpGet("excluir/{id:int}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _repository == null)
            {
                return NotFound();
            }

            var category = await _repository.GetById(id);
            if (category == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<CategoryViewModel>(category);

            return View(viewModel);
        }

        [HttpPost("excluir/{id:int}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_repository == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
            }
            var category = await _repository.GetById(id);
            if (category != null)
            {
                await _repository.Delete(category);

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _repository.CategoryExists(id);
        }

        private bool CheckNameAndSlug(Category category)
        {
            if (_repository.NameExists(category))
            {
                ModelState.AddModelError(string.Empty, "Já existe uma categoria com este nome");
                return false;
            }

            if (_repository.SlugExists(category))
            {
                ModelState.AddModelError(string.Empty, "Já existe uma categoria com este slug.");
                return false;
            }

            return true;
        }
    }
}
