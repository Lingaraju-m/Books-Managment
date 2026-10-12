using API_Practise.Entities;
using API_Practise.Models;

namespace API_Practise.Service
{
    public interface IBookService
    {
        Task<List<BooksDto>> GetBooksAsync();

        Task<BooksDto> GetBookByIdAsync(int id);

        Task<BooksDto> AddBookAsync(newBookDto book);

        Task<BooksDto> UpdateBookAsync(int id, newBookDto book);

        Task<bool> DeleteBookAsync(int id);
    }
}
