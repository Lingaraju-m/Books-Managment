using API_Practise.Data;
using API_Practise.Entities;
using API_Practise.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Practise.Service
{
    public class BookSerice(APIContext context) : IBookService
    {
        public async Task<BooksDto> AddBookAsync(newBookDto book)
        {
            var newbook = new Book
            {
                Title = book.Title,
                Author = book.Author,
                YearPublished = book.YearPublished
            };  
            context.Books.Add(newbook);
            await context.SaveChangesAsync();

            return new BooksDto
            {
                Id = newbook.Id,
                Title = newbook.Title,
                Author = newbook.Author,
                YearPublished = newbook.YearPublished
            };
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            var book = await context.Books.FindAsync(id);

            if (book is null)
                return false;

            context.Books.Remove(book);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<BooksDto> GetBookByIdAsync(int id)
        {
            var book = await context.Books.FindAsync(id);
            if (book is null)
                return null;
            return new BooksDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                YearPublished = book.YearPublished
            };
        }

        public async Task<List<BooksDto>> GetBooksAsync()
        {
            var books = await context.Books.ToListAsync();
            return books.Select(book => new BooksDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                YearPublished = book.YearPublished
            }).ToList();
        }

        public async Task<BooksDto> UpdateBookAsync(int id, newBookDto book)
        {
            var foundbok = await context.Books.FindAsync(id);
            if (foundbok is null)
                return null;

            foundbok.Title = book.Title;
            foundbok.Author = book.Author;
            foundbok.YearPublished = book.YearPublished;

            await context.SaveChangesAsync();

            return new BooksDto
            {
                Id = foundbok.Id,
                Title = foundbok.Title,
                Author = foundbok.Author,
                YearPublished = foundbok.YearPublished
            };
        }

       
    }
}
