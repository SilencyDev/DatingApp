using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            this._mapper = mapper;
            this._userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers() {
            return Ok(await _userRepository.GetMembersAsync());
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDTO>> GetUsers(string username) {
            return await _userRepository.GetMemberAsync(username);
        }
		
		[HttpPut]
		public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO) {
			var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var user = await _userRepository.GetUserByUsernameAsync(username);
			
			_mapper.Map(memberUpdateDTO, user);
			
			_userRepository.update(user);
			
			if (await _userRepository.SaveAllAsync()) return NoContent();
			
			return BadRequest("Failed to update user");
		}
    }
}
