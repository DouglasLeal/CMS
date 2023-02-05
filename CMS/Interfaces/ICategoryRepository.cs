using CMS.Models;
using Microsoft.EntityFrameworkCore;

namespace CMS.Interfaces
{
    public interface ICategoryRepository : IDisposable
    {
        Task Create(Category category);
        Task<IList<Category>> List();
        Task<Category> GetById(int? id);
        Task Update(Category category);
        Task Delete(Category category);
        bool CategoryExists(int id);
        bool NameExists(Category category);
        bool SlugExists(Category category);
    }
}
