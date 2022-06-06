using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Interfaces;
public interface IPhotoRepository
{
	void DeletePhoto(Photo photo);
	Task<PagedList<PhotoDTO>> getUnvalidatedPhoto(UserParams userParams);
	Task<Photo> getPhoto(int id);
}
