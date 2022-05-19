using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
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
		
		[HttpGet]
		public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessagesAsync([FromQuery] MessageParams messageParams) {
			messageParams.Username = User.GetUsername();
			
			var messages = await _messageRepository.GetUserMessages(messageParams);
			Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.Count, messages.TotalPages);
			
			return messages;
		}
		
		[HttpGet("thread/{username}")]
		public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessageThread(string username) {
		
			return Ok(await _messageRepository.GetMessageThread(User.GetUsername(), username));
		}
		
		[HttpDelete("{id}")]
		public async Task<ActionResult> DeleteMessage(int id) {
			var username = User.GetUsername();
			var message = await _messageRepository.GetMessage(id);
			if (message.RecipientUsername != username && message.SenderUsername != username)
				return Unauthorized();
			if (message.Recipient.Username == username)
				message.RecipientDeleted = true;
			else if (message.Sender.Username == username)
				message.SenderDeleted = true;
			if (message.RecipientDeleted && message.SenderDeleted)
				_messageRepository.DeleteMessage(message);
			if (await _messageRepository.SaveAllAsync())
				return Ok();
			
			return BadRequest("Failed to delete");
		}
	}
}
