namespace API.Data;
public class LikesRepository : ILikesRepository
{
    private readonly DataContext _context;
	public LikesRepository(DataContext context)
	{
		_context = context;
	}

	public async Task<UserLike> GetUserLike(int SourceUserId, int likedUserId)
	{
		return await _context.Likes.FindAsync(SourceUserId, likedUserId);
	}

	public async Task<PagedList<LikeDTO>> GetUserLikes([FromQuery] LikesParams likesParams)
	{
		var users = _context.Users.OrderBy(user => user.UserName).AsQueryable();
		var likes = _context.Likes.AsQueryable();

		if (likesParams.Predicate == "liked") {
			likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
			users = likes.Select(like => like.LikedUser);
		}
		else if (likesParams.Predicate == "likedby") {
			likes = likes.Where(like => like.LikedUserId == likesParams.UserId);
			users = likes.Select(like => like.SourceUser);
		}

		var list =  users.Select(user => new LikeDTO {
			Username = user.UserName,
			Pseudo = user.Pseudo,
			Age = user.DateOfBirth.CalculateAge(),
			PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
			City = user.City,
			Id = user.Id
		});
		
		return await PagedList<LikeDTO>.CreateAsync(list, likesParams.PageNumber, likesParams.PageSize);
	}

	public async Task<AppUser> GetUserWithLikes(int userId)
	{
		return await _context.Users
			.Include(user => user.LikedUser)
			.FirstOrDefaultAsync(user => user.Id == userId);
	}
}
