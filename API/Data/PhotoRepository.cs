using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class PhotoRepository(DataContext context, IMapper mapper) : IPhotoRepository
{
    public async Task<IEnumerable<PhotoForApprovalDto>> GetPhotosForModeration()
    {
        return await context.Photos
            .Where(x => !x.IsApproved)
            .IgnoreQueryFilters()
            .ProjectTo<PhotoForApprovalDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<Photo?> GetPhotoById(int photoId)
    {
        return await context.Photos
            .Where(x => x.Id == photoId)
            .Include(x => x.AppUser)
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync();
    }

    public void RemovePhoto(Photo photo)
    {
        context.Photos.Remove(photo);
    }
}
