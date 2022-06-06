namespace API.Controllers;

public class AdminController : BaseApiController
{
    public readonly UserManager<AppUser> _userManager;
	private readonly IUnitOfWork _unitOfWork;

	public AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork)
	{
        _userManager = userManager;
		_unitOfWork = unitOfWork;
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
	public async Task<ActionResult<List<PhotoDTO>>> GetPhotosForModeration([FromQuery] UserParams userParams) {
		var photoList = await _unitOfWork.PhotoRepository.getUnvalidatedPhoto(userParams);
		Response.AddPaginationHeader(photoList.CurrentPage, photoList.PageSize, photoList.TotalCount, photoList.TotalPages);
		return photoList;
	}
	
	[Authorize(Policy = "ModeratePhotoRole")]
    [HttpDelete("photos-to-moderate/delete/{id}")]
	public async Task<ActionResult> DeletePhotoOnModeration(int id) {
		var photo = await _unitOfWork.PhotoRepository.getPhoto(id);
		if (photo == null)
			return NotFound();
		if (photo.IsValidated)
			return BadRequest("You can't delete a validated photo");
		_unitOfWork.PhotoRepository.DeletePhoto(photo);
		
		if (await _unitOfWork.Complete())
			return Ok();
		return BadRequest("Failed to delete photo on moderation");
		
	}
	
	[Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("photos-to-moderate/accept/{id}")]
	public async Task<ActionResult> AcceptPhotoOnModeration(int id) {
		var photo = await _unitOfWork.PhotoRepository.getPhoto(id);
		if (photo == null)
			return NotFound();
		if (photo.IsValidated)
			return BadRequest("You can't validate a validated photo");
		photo.IsValidated = true;
		if (await _unitOfWork.Complete())
			return Ok();
		return BadRequest("Failed to Accept photo on moderation");
		
	}
}

