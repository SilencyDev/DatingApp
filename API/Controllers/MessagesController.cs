using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	public class MessagesController : BaseApiController
	{
        public readonly IMessageRepository _messageRepository;
        public readonly IUserRepository _userRepository;
		private readonly IMapper _mapper;

		public MessagesController(
			IMessageRepository messageRepository,
			IUserRepository userRepository,
			IMapper mapper)
		{
            _userRepository = userRepository;
			_mapper = mapper;
			_messageRepository = messageRepository;
		}
		
		[HttpPost]
		public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO) {
			var username = User.GetUsername();
			
			if (username == createMessageDTO.RecipientUsername.ToLower())
				return BadRequest("You cannot send messages to yourself");
			var sender = await _userRepository.GetUserByUsernameAsync(username);
			var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDTO.RecipientUsername);
			if (recipient == null)
				return NotFound();
			var message = new Message{
				Sender = sender,
				Recipient = recipient,
				SenderUsername = sender.Username,
				RecipientUsername = recipient.Username,
				Content = createMessageDTO.Content
			};
			_messageRepository.AddMessage(message);
			
			if (await _messageRepository.SaveAllAsync())
				return Ok(_mapper.Map<MessageDTO>(message));
			return BadRequest("Failed to add the message");
		}
		
		// [HttpGet]
		// public async Task<Message> GetMessageAsync() {
		// 	return await _messageRepository.GetMessage();
		// }
	}
}
