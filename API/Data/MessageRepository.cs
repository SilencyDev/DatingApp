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

		public void AddGroup(Group group)
		{
			_context.Groups.Add(group);
		}

		public void AddMessage(Message message)
		{
			_context.Messages.Add(message);
		}

		public void DeleteMessage(Message message)
		{
			_context.Messages.Remove(message);
		}

		public async Task<Connection> GetConnection(string connectionId)
		{
			return await _context.Connections.FindAsync(connectionId);
		}

		public async Task<Group> GetGroupForConnection(string connectionId)
		{
			return await _context.Groups
				.Include(c => c.Connections)
				.Where(c => c.Connections.Any( x => x.ConnectionId == connectionId))
				.FirstOrDefaultAsync();
		}

		public async Task<Message> GetMessage(int id)
		{
			return await _context.Messages
				.Include(u => u.Sender)
				.Include(u => u.Recipient)
				.SingleOrDefaultAsync(m =>
				m.Id == id);
		}

		public async Task<Group> GetMessageGroup(string groupName)
		{
			return await _context.Groups
				.Include(x => x.Connections)
				.FirstOrDefaultAsync(x => x.Name == groupName);
		}

		public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUsername, string recipientUsername)
		{
			var messages = await _context.Messages
				.Include(u => u.Sender).ThenInclude(p => p.Photos)
				.Include(u => u.Recipient).ThenInclude(p => p.Photos)
				.Where(m =>
					m.Recipient.UserName == currentUsername &&
					m.Sender.UserName == recipientUsername &&
					m.RecipientDeleted == false ||
					m.Recipient.UserName == recipientUsername &&
					m.Sender.UserName == currentUsername &&
					m.SenderDeleted == false)
				.OrderBy(m => m.MessageSent)
				.ToListAsync();
				
			var unseenMessage = messages
				.Where(m => m.DateRead == null
					&& m.Recipient.UserName == currentUsername)
				.ToList();
			if (unseenMessage.Any()) {
				foreach(var message in unseenMessage) {
					message.DateRead = DateTime.UtcNow;
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
					m.Recipient.UserName == messageParams.Username &&
					m.RecipientDeleted == false),
				"Outbox" => query.Where(m =>
					m.Sender.UserName == messageParams.Username &&
					m.SenderDeleted == false),
				_ => query.Where(m =>
					m.Recipient.UserName == messageParams.Username &&
					m.DateRead == null &&
					m.RecipientDeleted == false),
			};
			
			var message = query.ProjectTo<MessageDTO>(_mapper.ConfigurationProvider);
			
			return await PagedList<MessageDTO>.CreateAsync(message, messageParams.PageNumber, messageParams.PageSize);
		}

		public void RemoveConnection(Connection connection)
		{
			_context.Connections.Remove(connection);
		}

		public async Task<bool> SaveAllAsync()
		{
			return await _context.SaveChangesAsync() > 0;
		}
	}
}
