using CMS.Models;

namespace CMS.Interfaces
{
    public interface IPostRepository : IDisposable
    {
        Task Create(Post post);
        Task<IList<Post>> List();
        Task<IList<Post>> List(string categorySlug);
        Task<Post> GetById(int? id);
        Task Update(Post post);
        Task Delete(Post post);
        bool PostExists(int id);
    }
}
