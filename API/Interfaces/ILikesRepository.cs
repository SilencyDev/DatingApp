using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using API.DTOs;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike> GetUserLike(int SourceUserId, int likedUsers);
        Task<AppUser> GetUserWithLikes(int userId);
        Task<IEnumerable<LikeDTO>>GetUserLikes(string predicate, int userId);

    }
}