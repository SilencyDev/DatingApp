using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using API.DTOs;
using API.Helpers;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike> GetUserLike(int SourceUserId, int likedUsers);
        Task<AppUser> GetUserWithLikes(int userId);
        Task<PagedList<LikeDTO>>GetUserLikes(LikesParams likesParams);
    }
}
