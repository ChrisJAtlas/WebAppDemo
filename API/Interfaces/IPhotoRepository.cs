using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IPhotoRepository
{
    Task<IEnumerable<PhotoForApprovalDto>> GetPhotosForModeration();

    Task<Photo?> GetPhotoById(int photoId);

    void RemovePhoto(Photo photo);
}
