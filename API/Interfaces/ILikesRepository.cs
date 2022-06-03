namespace API.Interfaces;

public interface ILikesRepository
{
    Task<UserLike> GetUserLike(int SourceUserId, int likedUsers);
    Task<AppUser> GetUserWithLikes(int userId);
    Task<PagedList<LikeDTO>>GetUserLikes(LikesParams likesParams);
}
