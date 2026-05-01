using Backend_APIs.DTOs;
using Microsoft.AspNetCore.Http;

namespace Backend_APIs.Services
{
    public interface IUserService
    {
        Task<(bool Success, string Message, UserDto? User)> GetProfileAsync(int userId);
        Task<(bool Success, string Message, UserDto? User)> UpdateProfileAsync(int userId, UpdateProfileDto updateProfileDto);
        Task<(bool Success, string Message)> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<(bool Success, string Message, string? ImageUrl)> UploadProfilePhotoAsync(int userId, IFormFile photo);
    }
}
