using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
	public class MessageRepository : IMessageRepository
	{
        public readonly DataContext _context;
		public readonly IMapper _mapper;
		public MessageRepository(DataContext context, IMapper mapper)
		{
            _context = context;
			_mapper = mapper;
		}

		public IMapper Mapper { get; }

		public void AddMessage(Message message)
		{
			_context.Messages.Add(message);
		}

		public void DeleteMessage(Message message)
		{
			_context.Messages.Remove(message);
		}

		public async Task<Message> GetMessage(int id)
		{
			return await _context.Messages
				.Include(u => u.Sender)
				.Include(u => u.Recipient)
				.SingleOrDefaultAsync(m =>
				m.Id == id);
		}

		public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUsername, string recipientUsername)
		{
			var messages = await _context.Messages
				.Include(u => u.Sender).ThenInclude(p => p.Photos)
				.Include(u => u.Recipient).ThenInclude(p => p.Photos)
				.Where(m =>
					m.Recipient.Username == currentUsername &&
					m.Sender.Username == recipientUsername &&
					m.RecipientDeleted == false ||
					m.Recipient.Username == recipientUsername &&
					m.Sender.Username == currentUsername &&
					m.SenderDeleted == false)
				.OrderBy(m => m.MessageSent)
				.ToListAsync();
				
			var unseenMessage = messages
				.Where(m => m.DateRead == null
					&& m.Recipient.Username == currentUsername)
				.ToList();
			if (unseenMessage.Any()) {
				foreach(var message in unseenMessage) {
					message.DateRead = DateTime.Now;
				}
				await _context.SaveChangesAsync();
			}

			return _mapper.Map<IEnumerable<MessageDTO>>(messages);
		}

		public async Task<PagedList<MessageDTO>> GetUserMessages(MessageParams messageParams)
		{
			var query = _context.Messages
				.OrderByDescending(m => m.MessageSent)
				.AsQueryable();
			
			query = messageParams.Container switch {
				"Inbox"	=> query.Where(m =>
					m.Recipient.Username == messageParams.Username &&
					m.RecipientDeleted == false),
				"Outbox" => query.Where(m =>
					m.Sender.Username == messageParams.Username &&
					m.SenderDeleted == false),
				_ => query.Where(m =>
					m.Recipient.Username == messageParams.Username &&
					m.DateRead == null &&
					m.RecipientDeleted == false),
			};
			
			var message = query.ProjectTo<MessageDTO>(_mapper.ConfigurationProvider);
			
			return await PagedList<MessageDTO>.CreateAsync(message, messageParams.PageNumber, messageParams.PageSize);
		}

		public async Task<bool> SaveAllAsync()
		{
			return await _context.SaveChangesAsync() > 0;
		}
	}
}
