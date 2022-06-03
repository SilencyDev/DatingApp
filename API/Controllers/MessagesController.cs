namespace API.Controllers;

public class MessagesController : BaseApiController
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMapper _mapper;

	public MessagesController(
		IUnitOfWork unitOfWork,
		IMapper mapper)
	{
		_unitOfWork = unitOfWork;
		_mapper = mapper;
	}
	
	[HttpPost]
	public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO) {
		var username = User.GetUsername();
		
		if (username == createMessageDTO.RecipientUsername.ToLower())
			return BadRequest("You cannot send messages to yourself");
		var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
		var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDTO.RecipientUsername);
		if (recipient == null)
			return NotFound();
		var message = new Message{
			Sender = sender,
			Recipient = recipient,
			SenderUsername = sender.UserName,
			RecipientUsername = recipient.UserName,
			Content = createMessageDTO.Content
		};
		_unitOfWork.MessageRepository.AddMessage(message);
		
		if (await _unitOfWork.Complete())
			return Ok(_mapper.Map<MessageDTO>(message));
		return BadRequest("Failed to add the message");
	}
	
	[HttpGet]
	public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessagesAsync([FromQuery] MessageParams messageParams) {
		messageParams.Username = User.GetUsername();
		
		var messages = await _unitOfWork.MessageRepository.GetUserMessages(messageParams);
		Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.Count, messages.TotalPages);
		
		return messages;
	}
	
	[HttpDelete("{id}")]
	public async Task<ActionResult> DeleteMessage(int id) {
		var username = User.GetUsername();
		var message = await _unitOfWork.MessageRepository.GetMessage(id);
		if (message.RecipientUsername != username && message.SenderUsername != username)
			return Unauthorized();
		if (message.Recipient.UserName == username)
			message.RecipientDeleted = true;
		else if (message.Sender.UserName == username)
			message.SenderDeleted = true;
		if (message.RecipientDeleted && message.SenderDeleted)
			_unitOfWork.MessageRepository.DeleteMessage(message);
		if (await _unitOfWork.Complete())
			return Ok();
		
		return BadRequest("Failed to delete");
	}
}
