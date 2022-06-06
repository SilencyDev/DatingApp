using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;

namespace API.Data
{
	public class PhotoRepository : IPhotoRepository
	{
		private readonly DataContext _Context;
		private readonly IMapper _mapper;

		public PhotoRepository(DataContext Context, IMapper mapper)
		{
			_Context = Context;
			_mapper = mapper;
		}

		public void DeletePhoto(Photo photo)
		{
			_Context.Photos.Remove(photo);
		}

		public async Task<PagedList<PhotoDTO>> getUnvalidatedPhoto(UserParams userParams)
		{
			var query = _Context.Photos
				.IgnoreQueryFilters()
				.Where(p => p.IsValidated == false)
				.ProjectTo<PhotoDTO>(_mapper.ConfigurationProvider)
				.AsQueryable();
			return await PagedList<PhotoDTO>.CreateAsync(query, userParams.PageNumber, userParams.PageSize); 
		}
		
		public async Task<Photo> getPhoto(int id) {
			return await _Context.Photos
				.IgnoreQueryFilters()
				.FirstOrDefaultAsync(p => p.Id == id);
		}
	}
}
