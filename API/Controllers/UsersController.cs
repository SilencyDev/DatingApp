using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
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
        private readonly IMapper _mapper;
		private readonly IPhotoService _photoService;
        private readonly IUnitOfWork _unitOfWork;

		public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
			_photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers([FromQuery] UserParams userParams) {
			userParams.CurrentUsername = User.GetUsername();
			var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);
			Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(users);
        }

        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDTO>> GetUsers(string username) {
            return await _unitOfWork.UserRepository.GetMemberAsync(username);
        }
		
		[HttpPut]
		public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO) {
			var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
			
			_mapper.Map(memberUpdateDTO, user);
			
			_unitOfWork.UserRepository.update(user);
			
			if (await _unitOfWork.Complete()) return NoContent();
			
			return BadRequest("Failed to update user");
		}
		
		[HttpPost("add-photo")]
		public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file) {
			var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
			
			var result = await this._photoService.AddPhotoAsync(file);
			
			if (result.Error != null) return BadRequest(result.Error.Message);
			
			var photo = new Photo {
				Url = result.SecureUrl.AbsoluteUri,
				PublicId = result.PublicId
			};
			
			if (user.Photos.Count == 0) {
				photo.IsMain = true;
			}
			
			user.Photos.Add(photo);
			
			if (await _unitOfWork.Complete()) {
				return CreatedAtRoute("GetUser", new {username = user.UserName}, _mapper.Map<PhotoDTO>(photo));
			}
			return BadRequest("Uploading the photo failed");
		}
		
		[HttpPut("set-main-photo/{photoId}")]
		public async Task<ActionResult> setMainPhoto(int photoId) {
			var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
			
			var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);
			
			if (photo == null) return NotFound();
			
			if (photo.IsMain) return BadRequest("The photo is already main");
			
			var previousMain = user.Photos.FirstOrDefault(photo => photo.IsMain);
			
			if (previousMain != null) previousMain.IsMain = false;
			
			photo.IsMain = true;
			
			if (await _unitOfWork.Complete()) return NoContent();
			
			return BadRequest("Failed to set main photo");
		}
		
		[HttpDelete("delete-photo/{photoId}")]
		public async Task<ActionResult> deletePhoto(int photoId) {
			var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
			
			var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);
			
			if (photo == null) return NotFound();
			
			if (photo.IsMain) return BadRequest("You cannot remove main photo");
			
			if (photo.PublicId != null) {
				var result = await _photoService.DeletePhotoAsync(photo.PublicId);
				if (result.Error != null) return BadRequest(result.Error.Message);
			}
			
			user.Photos.Remove(photo);
			
			if (await _unitOfWork.Complete()) return Ok();
			
			return BadRequest("The photo couldn't be removed");
		}
    }
}
