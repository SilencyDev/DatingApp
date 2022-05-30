using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	public class LikesController : BaseApiController
	{
        private readonly IUnitOfWork _unitOfWork;
		public LikesController(IUnitOfWork unitOfWork)
		{
            _unitOfWork = unitOfWork;
		}

        [HttpPost("{username}")]
        public async Task<ActionResult> AddRemoveLike(string username) {
            var sourceUserId = User.GetUserId();
            var sourceUser = await _unitOfWork.LikesRepository.GetUserWithLikes(sourceUserId);
            var likedUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

            if (likedUser == null)
                return NotFound();
            if (sourceUser.UserName == username)
                return BadRequest("A user cannot like himself");
            
            var alreadyExist = await _unitOfWork.LikesRepository.GetUserLike(sourceUserId, likedUser.Id);
            if (alreadyExist != null) {
				sourceUser.LikedUser.Remove(alreadyExist);
			}
            else {
				alreadyExist = new UserLike {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id
            	};
           		sourceUser.LikedUser.Add(alreadyExist);	
			}

            if (await _unitOfWork.Complete())
                return Ok();
            
            return BadRequest("Failed to like the user");
        }
		
		[HttpGet]
		public async Task<ActionResult<PagedList<LikeDTO>>> GetUserLikes([FromQuery] LikesParams likesParams) {
			likesParams.UserId = User.GetUserId();
			if (likesParams.Predicate != "liked" && likesParams.Predicate != "likedby")
				return BadRequest("Wrong or empty predicate given");
			var users = await _unitOfWork.LikesRepository.GetUserLikes(likesParams);
			Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
			return Ok(users);
		}
	}
}
