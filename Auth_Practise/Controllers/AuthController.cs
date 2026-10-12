using Auth_Practise.Entities;
using Auth_Practise.Models;
using Auth_Practise.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth_Practise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authServie) : ControllerBase
    {
        public static User user = new User();

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDTO request)
        {
            var user = await authServie.RegisterAync(request);
            if(user == null)
            {
                return BadRequest("User already exists.");
            }
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDTO request)
        {
            var toekn = await authServie.LoginAsync(request);

            if(toekn == null)
            {
                return BadRequest("Invalid Username or Password!");
            }

            return Ok(toekn);
        }

        //[Authorize]
        //[HttpGet("Users")]
        //public async Task<ActionResult<List<UserDTO>>> GetUsers()
        //{
        //    var users = await authServie.GetAllUsersAsync();

        //    if (users == null)
        //        return NotFound("User databse is empty!!!");

        //    return Ok(users);
        //}

        [Authorize(Roles = "Admin")]
        [HttpGet("Users")]
        public async Task<ActionResult<List<UserDTO>>> GetUsers()
        {
            var users = await authServie.GetAllUsersAsync();

            if (users == null)
                return NotFound("No Users are there!!!");

            return Ok(users);
        }
    }

}
