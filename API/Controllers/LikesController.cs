using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	public class LikesController : BaseApiController
	{
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;
        
		public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
		{
            this._likesRepository = likesRepository;
            this._userRepository = userRepository;
		}

        [HttpPost("{username}")]
        public async Task<ActionResult> AddRemoveLike(string username) {
            var sourceUserId = User.GetUserId();
            var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);
            var likedUser = await _userRepository.GetUserByUsernameAsync(username);

            if (likedUser == null)
                return NotFound();
            if (sourceUser.Username == username)
                return BadRequest("A user cannot like himself");
            
            var alreadyExist = await _likesRepository.GetUserLike(sourceUserId, likedUser.Id);
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

            if (await _userRepository.SaveAllAsync())
                return Ok();
            
            return BadRequest("Failed to like the user");
        }
		
		[HttpGet]
		public async Task<ActionResult<IEnumerable<LikeDTO>>> GetUserLikes([FromQuery] string predicate) {
			if (predicate != "liked" && predicate != "likedby")
				return BadRequest("Wrong or empty predicate given");
			return Ok(await _likesRepository.GetUserLikes(predicate, User.GetUserId()));
		}
	}
}
