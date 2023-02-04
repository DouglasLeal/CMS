using CMS.Data;
using CMS.Interfaces;
using CMS.Models;
using Microsoft.EntityFrameworkCore;

namespace CMS.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task Create(Category category)
        {
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
        }

        public async Task<Category> GetById(int? id)
        {
            return await _db.Categories.FindAsync(id);
        }

        public async Task<IList<Category>> List()
        {
            return await _db.Categories.ToListAsync();
        }

        public async Task Update(Category category)
        {
            _db.Update(category);
            await _db.SaveChangesAsync();
        }

        public async Task Delete(Category? category)
        {
            _db.Remove(category);
            await _db.SaveChangesAsync();
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public bool CategoryExists(int id)
        {
            return _db.Categories.Any(e => e.Id == id);
        }
    }
}
