using API_Practise.Data;
using API_Practise.Entities;
using API_Practise.Models;
using API_Practise.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Practise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController(IBookService bookService, IAuthService authService) : ControllerBase
    {

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(userDto request)
        {
            var user = await authService.RegisterAync(request);
            if (user == null)
            {
                return BadRequest("User already exists.");
            }
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(userDto request)
        {
            var toekn = await authService.LoginAsync(request);

            if (toekn == null)
            {
                return BadRequest("Invalid Username or Password!");
            }

            return Ok(toekn);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<BooksDto>>>GetBooks()
        {
            var books = await bookService.GetBooksAsync();
            if(books is null || books.Count == 0)
                return NotFound("No books are present in the database!");
            return Ok(books);
        }

        [Authorize]
        [HttpGet("{Id}")]
        public async Task<ActionResult<BooksDto>> GetBookById(int Id)
        {
            var book = await bookService.GetBookByIdAsync(Id);
            if(book is null)
                return NotFound("The book your looking for is not present!");
            return Ok(book);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<BooksDto>> AddBook(newBookDto book)
        {
            if (book is null)
                return BadRequest("Please provide a proper values");

            var addBook = await bookService.AddBookAsync(book);
            return CreatedAtAction(nameof(GetBookById), new { id = addBook.Id }, addBook);
        }

        [Authorize]
        [HttpPut("{Id}")]
        public async Task<ActionResult<BooksDto>> UpdateBook(int Id, newBookDto book)
        {
            var updatebook = await bookService.UpdateBookAsync(Id, book);

            if(updatebook is null)
                return NotFound("The book your looking for is not present!");

            return Ok(updatebook);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{Id}")]
        public async Task<ActionResult<string>> DeleteBok(int id)
        {
            var book = await bookService.DeleteBookAsync(id);

            if (book == false)
                return NotFound("The book you are looking for is not present!");

            return "The book has been successfully deleted..";
        }
    }
}
