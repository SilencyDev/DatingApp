using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using Microsoft.AspNetCore.Mvc;
using API.Entities;
using System.Security.Cryptography;
using System.Text;
using API.DTOs;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
	public class AccountController : BaseApiController
	{
        private readonly DataContext _context;
		private readonly ITokenService _tokenService;
		public AccountController(DataContext context, ITokenService tokenService)
		{
			_tokenService = tokenService;
            _context = context;
		}

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO) {
            using var hmac = new HMACSHA512();

			if (await UserExist(registerDTO.Username))
				return BadRequest("Username is taken");
			var user = new AppUser {
				Username = registerDTO.Username.ToLower(),
				PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
				PasswordSalt = hmac.Key
			};

			_context.Users.Add(user);

			await _context.SaveChangesAsync();

			return new UserDTO{
				Username = user.Username,
				Token = _tokenService.CreateToken(user),
			};
        }

		[HttpPost("login")]
		public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO) {
			var user = await _context.Users.SingleOrDefaultAsync( user => user.Username == loginDTO.Username);

			if (user == null)
				return Unauthorized("Invalid Username");
			
			using var hmac = new HMACSHA512(user.PasswordSalt);

			var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));
			
			for (var i = 0; i < computedHash.Length; i++) {
				if (computedHash[i] != user.PasswordHash[i])
					return Unauthorized("Invalid password");
			}

			return new UserDTO{
				Username = user.Username,
				Token = _tokenService.CreateToken(user),
			};
		}

		private async Task<bool> UserExist(string username) {
			return await _context.Users.AnyAsync(user =>  user.Username == username.ToLower());
		}
	}
}
