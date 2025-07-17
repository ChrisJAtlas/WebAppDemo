using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController(IUnitOfWork unitOfWork, UserManager<AppUser> userManager, IPhotoService photoService) : BaseAPIController
{
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        var users = await userManager.Users
            .OrderBy(x => x.UserName)
            .Select(x => new
            {
                x.Id,
                Username = x.UserName,
                Roles = x.UserRoles.Select(r => r.Role.Name).ToList()
            }).ToListAsync();

        return Ok(users);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string username, string roles)
    {
        if (string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");

        var selectedRoles = roles.Split(",").ToArray();

        var user = await userManager.FindByNameAsync(username);

        if (user == null) return BadRequest("User not found");

        var userRoles = await userManager.GetRolesAsync(user);

        var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

        if (!result.Succeeded) return BadRequest("Failed to add to roles");

        result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

        if (!result.Succeeded) return BadRequest("Failed to remove from roles");

        return Ok(await userManager.GetRolesAsync(user));
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public async Task<ActionResult<IEnumerable<PhotoForApprovalDto>>> GetPhotosForModeration()
    {
        var photosToModerate = await unitOfWork.PhotoRepository.GetPhotosForModeration();

        return Ok(photosToModerate);
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("approve-photo/{photoId:int}")]
    public async Task<ActionResult> ApprovePhoto(int photoId)
    {
        //we need the username from this photo data
        var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoId);

        if (photo == null || photo.IsApproved) return BadRequest("Cannot approve this photo");

        photo.IsApproved = true;

        // Req 6: When an admin or moderator approves a photo for a user that does
        // not have a main photo, then this should set the photo to main
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(photo.AppUser.UserName!);

        if (user == null) return BadRequest("User not found");

        var mainExists = user.Photos.Exists(x => x.IsMain);
        if (!mainExists) photo.IsMain = true;

        if (await unitOfWork.Complete()) return NoContent();

        return BadRequest("Problem approving photo");
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("reject-photo/{photoId:int}")]
    public async Task<ActionResult> RejectPhoto(int photoId)
    {
        //we need the publicId from this photo data
        var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoId);

        if (photo == null || photo.IsApproved) return BadRequest("This photo cannot be rejected");

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }

        unitOfWork.PhotoRepository.RemovePhoto(photo);

        if (await unitOfWork.Complete()) return Ok();

        return BadRequest("Problem rejecting photo");
    }
}
