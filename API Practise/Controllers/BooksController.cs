using API_Practise.Data;
using API_Practise.Entities;
using API_Practise.Models;
using API_Practise.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


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

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginChallengeDto>> Login(
            userDto request)
        {
            var result = await authService.LoginAsync(request);

            if (result.ChallengeToken is null)
                return Unauthorized(result);

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("verify-login-otp")]
        public async Task<IActionResult> VerifyLoginOtp(
        VerifyLoginOtpDto request)
        {
            var token = await authService.VerifyLoginOtpAsync(
                request.ChallengeToken,
                request.Otp);

            if (token is null)
                return Unauthorized(new
                {
                    message = "Invalid challenge or OTP."
                });

            return Ok(new { accessToken = token });
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<BooksDto>>> GetBooks()
        {
            var books = await bookService.GetBooksAsync();
            if (books is null || books.Count == 0)
                return NotFound("No books are present in the database!");
            return Ok(books);
        }

        [Authorize]
        [HttpGet("{Id}")]
        public async Task<ActionResult<BooksDto>> GetBookById(int Id)
        {
            var book = await bookService.GetBookByIdAsync(Id);
            if (book is null)
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

            if (updatebook is null)
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


        [Authorize]
        [HttpPost("setup-otp")]
        public async Task<ActionResult<TotpSetupResponseDto>> SetupOtp()
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var qrCode = await authService.GenerateTotpQrCodeAsync(userId);

            if (qrCode is null)
                return Conflict(
                    "User not found or TOTP is already enabled.");

            return Ok(new TotpSetupResponseDto
            {
                QrCodeBase64 = qrCode
            });
        }

        [Authorize]
        [HttpPost("verify-otp-setup")]
        public async Task<IActionResult> VerifyOtpSetup(
            VerifyTotpDto request)
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var isValid = await authService.VerifyTotpSetupAsync(
                userId, request.Otp);

            if (!isValid)
                return BadRequest("Invalid OTP or TOTP setup.");

            return Ok("TOTP enabled successfully.");
        }

    }
}
