using CMS.Data;
using CMS.Interfaces;
using CMS.Models;
using Microsoft.EntityFrameworkCore;

namespace CMS.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext _db;

        public PostRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task Create(Post post)
        {
            _db.Posts.Add(post);
            await _db.SaveChangesAsync();
        }

        public async Task<Post> GetById(int? id)
        {
            return await _db.Posts.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IList<Post>> List()
        {
            return await _db.Posts.Include(p => p.Category).ToListAsync();
        }

        public async Task<IList<Post>> List(string categorySlug)
        {
            return await _db.Posts.Include(p => p.Category).Where(p => p.Category.Slug == categorySlug).ToListAsync();
        }

        public async Task Update(Post post)
        {
            _db.Update(post);
            await _db.SaveChangesAsync();
        }

        public async Task Delete(Post post)
        {
            _db.Remove(post);
            await _db.SaveChangesAsync();
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public bool PostExists(int id)
        {
            return _db.Posts.Any(e => e.Id == id);
        }
    }
}
