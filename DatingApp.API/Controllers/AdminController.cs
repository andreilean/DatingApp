using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DatingApp.API.Dtos;
using Microsoft.AspNetCore.Identity;
using DatingApp.API.Models;
using AutoMapper;
using System.Collections.Generic;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public AdminController(DataContext context, UserManager<User> userManager, IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
            _userManager = userManager;
            _context = context;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("usersWithRoles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            var userList = await (
                from user in _context.Users
                orderby user.UserName
                select new
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Roles = (
                        from userRole in user.UserRoles
                        join role in _context.Roles on userRole.RoleId
                        equals role.Id
                        select role.Name).ToList()
                }).ToListAsync();
            return Ok(userList);
        }


        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("editRoles/{userName}")]
        public async Task<IActionResult> EditRoles(string userName, RoleEditDto roleEditDto)
        {
            var user = await _userManager.FindByNameAsync(userName);

            var userRoles = await _userManager.GetRolesAsync(user);

            var selectedRoles = roleEditDto.RoleNames;

            selectedRoles = selectedRoles ?? new string[] { };

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if (!result.Succeeded)
            {
                return BadRequest("Failed to add roles");
            }

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!result.Succeeded)
            {
                return BadRequest("Failed to remove roles");
            }

            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photosForModeration")]
        public async Task<IActionResult> GetPhotosForModeration()
        {
            var photos = await _repo.GetPhotosForModeration();
            var photosToReturn = _mapper.Map<IEnumerable<PhotoForReturnDto>>(photos);
            return Ok(photosToReturn);
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approvePhoto/{photoId}")]
        public async Task<IActionResult> ApprovePhoto(int photoId)
        {
            var photo = await _repo.GetPhoto(photoId);
            if (photo == null)
                return NotFound("Photo with id=" + photoId + " does not exist");

            if (photo.IsApproved)
                return BadRequest("This photo is already approved.");

            _repo.ApprovePhoto(photoId);

            if (await _repo.SaveAll())
                return Ok();

            return BadRequest("Failed to approve photo");
        }
        
        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("rejectPhoto/{photoId}")]
        public async Task<IActionResult> RejectPhoto(int photoId)
        {
            var photo = await _repo.GetPhoto(photoId);
            if (photo == null)
                return NotFound("Photo with id=" + photoId + " does not exist");

            _repo.Delete(photo);

            if (await _repo.SaveAll())
                return Ok();

            return BadRequest("Failed to reject photo");
        }
    }


}