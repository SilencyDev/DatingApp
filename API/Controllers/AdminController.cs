namespace API.Controllers;

public class AdminController : BaseApiController
{
    public readonly UserManager<AppUser> _userManager;

	public AdminController(UserManager<AppUser> userManager)
	{
        _userManager = userManager;
	}

	[Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
	public async Task<ActionResult> GetUsersWithRoles() {
		return Ok(await _userManager.Users
			.Include(u => u.UserRoles)
			.ThenInclude(u => u.Role)
			.OrderBy(u => u.UserName)
			.Select(u => new{
				u.Id,
				Username = u.UserName,
				Roles = u.UserRoles.Select(u => u.Role.Name).ToList()
			})
			.ToListAsync());
			
	}
	
	[HttpPost("edit-roles/{username}")]
	public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles) {
		var selectedRoles = roles.Split(",").ToArray();
		var user = await _userManager.FindByNameAsync(username);
		if (user == null)
			return NotFound();
		var userRoles = await _userManager.GetRolesAsync(user);
		var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
		if (!result.Succeeded)
			return BadRequest("Failed to add roles");
		result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
		if (!result.Succeeded)
			return BadRequest("Failed to remove roles");
		return Ok(await _userManager.GetRolesAsync(user));
	}
	
	[Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
	public ActionResult GetPhotosForModeration() {
		return Ok("Only Admins can see this");
	}
}

