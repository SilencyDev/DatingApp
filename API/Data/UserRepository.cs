using AutoMapper.QueryableExtensions;

namespace API.Data;
public class UserRepository : IUserRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public UserRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MemberDTO> GetMemberAsync(string username)
    {
        return await _context.Users
            .Where(p => p.UserName == username)
            .ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    public async Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams)
    {

        var query = _context.Users
			.Where(u => u.UserName != userParams.CurrentUsername)
			.Where(u =>
				u.DateOfBirth >= DateTime.Today.AddYears(-userParams.MaxAge - 1)
				&& u.DateOfBirth <= DateTime.Today.AddYears(-userParams.MinAge));
		query = userParams.OrderBy switch {
			"created" => query.OrderByDescending(u => u.Created),
			_ => query.OrderByDescending(u => u.LastActive)
		};
		if (userParams.gender != null)
			query = query.Where(u => u.Gender == userParams.gender);
			
		return await PagedList<MemberDTO>.CreateAsync(query.ProjectTo<MemberDTO>(_mapper.ConfigurationProvider)
            .AsNoTracking(), userParams.PageNumber, userParams.PageSize);
    }

    public async Task<IEnumerable<AppUser>> GetUserAsync()
    {
        return await _context.Users.Include(user => user.Photos).ToListAsync();
    }

    public async Task<AppUser> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
        return await _context.Users.Include(user => user.Photos).SingleOrDefaultAsync(user => user.UserName == username);
    }

    public void update(AppUser user)
    {
        _context.Entry(user).State = EntityState.Modified;
    }
}
