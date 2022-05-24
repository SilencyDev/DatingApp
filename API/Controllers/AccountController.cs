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
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace API.Controllers
{
	public class AccountController : BaseApiController
	{
		private readonly ITokenService _tokenService;
		private readonly IMapper _mapper;
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;

		public AccountController(
			UserManager<AppUser> userManager,
			SignInManager<AppUser> signInManager,
			ITokenService tokenService,
			IMapper mapper)
		{
            _userManager = userManager;
			_signInManager = signInManager;
			_tokenService = tokenService;
			_mapper = mapper;
		}

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO) {
			var user = _mapper.Map<AppUser>(registerDTO);

			if (await UserExist(registerDTO.Username))
				return BadRequest("Username is taken");
			user.UserName = registerDTO.Username.ToLower();

			var result = await _userManager.CreateAsync(user, registerDTO.Password);
			
			if (!result.Succeeded)
				return BadRequest(result.Errors);
				
			var roleResult = await _userManager.AddToRoleAsync(user, "Member");
			
			if (!roleResult.Succeeded)
				return BadRequest(result.Errors);

			return new UserDTO{
				Username = user.UserName,
				Token = await _tokenService.CreateToken(user),
				Pseudo = user.Pseudo,
			};
        }

		[HttpPost("login")]
		public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO) {
			var user = await _userManager.Users
			.Include(user => user.Photos)
			.SingleOrDefaultAsync( user => user.UserName == loginDTO.Username.ToLower());

			if (user == null)
				return Unauthorized("Invalid Username");
			
			var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);
			
			if (!result.Succeeded)
				return Unauthorized();

			return new UserDTO{
				Username = user.UserName,
				Token = await _tokenService.CreateToken(user),
				Pseudo = user.Pseudo,
				PhotoUrl = user.Photos.FirstOrDefault(photo => photo.IsMain == true)?.Url
			};
		}

		private async Task<bool> UserExist(string username) {
			return await _userManager.Users.AnyAsync(user =>  user.UserName == username.ToLower());
		}
	}
}
