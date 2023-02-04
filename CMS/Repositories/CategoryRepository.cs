using CMS.Data;
using CMS.Interfaces;
using CMS.Models;
using Microsoft.EntityFrameworkCore;

namespace CMS.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly DbSet<Category> _dbSet;

        public CategoryRepository(ApplicationDbContext db)
        {
            _db = db;
            _dbSet = db.Set<Category>();
        }

        public async Task Create(Category category)
        {
            _dbSet.Add(category);
            await _db.SaveChangesAsync();
        }

        public async Task<Category> GetById(int? id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IList<Category>> List()
        {
            return await _dbSet.ToListAsync();
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
            return _dbSet.Any(c => c.Id == id);
        }

        public bool NameExists(string name) 
        {
            return _dbSet.Any(c => c.Name == name);
        }

        public bool SlugExists(string slug)
        {
            return _dbSet.Any(c => c.Slug == slug);
        }
    }
}
