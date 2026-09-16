using System.Collections.Generic;
using System.Threading.Tasks;
using ComicSystem.Models;
using ComicSystem.Repositories;

namespace ComicSystem.Services
{
    public class ComicBookService : IComicBookService
    {
        private readonly IComicBookRepository _comicBookRepository;

        public ComicBookService(IComicBookRepository comicBookRepository)
        {
            _comicBookRepository = comicBookRepository;
        }

        public async Task<IEnumerable<ComicBook>> GetAllAsync()
        {
            return await _comicBookRepository.GetAllAsync();
        }

        public async Task<ComicBook?> GetByIdAsync(int id)
        {
            return await _comicBookRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(ComicBook comicBook)
        {
            await _comicBookRepository.AddAsync(comicBook);
        }

        public async Task UpdateAsync(ComicBook comicBook)
        {
            await _comicBookRepository.UpdateAsync(comicBook);
        }

        public async Task DeleteAsync(int id)
        {
            await _comicBookRepository.DeleteAsync(id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _comicBookRepository.ExistsAsync(id);
        }
    }
}
