﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

using Microsoft.Extensions.Logging;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Mapping;
using Domain.DTO;
using Domain.Entities;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly VagtplanDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(VagtplanDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetById(Guid userId)
        {
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    _logger.LogWarning("Employment period {userId} not found", userId);
                    return NotFound(new { message = "Employment period not found" });
                }

                return Ok(user.ToDTO());
            }
            catch (DbException dbEx)
            {
                _logger.LogError(dbEx, "Database error occurred while retrieving employment period {EmploymentPeriodId}", userId);
                return StatusCode(500, new { message = "A database error occurred" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while retrieving employment period {EmploymentPeriodId}", userId);
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var users = await _context.Users
                    .AsNoTracking()
                    .ToListAsync();

                if (!users.Any())
                {
                    _logger.LogWarning("No users found");
                    return NotFound(new { message = "No users found" });
                }

                return Ok(users);
            }
            catch (DbException dbEx)
            {
                _logger.LogError(dbEx, "Database error occurred while retrieving users");
                return StatusCode(500, new { message = "A database error occurred" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while retrieving users");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [HttpGet("GetAdminInfo")]
        public async Task<IActionResult> GetAdminInfo()
        {
            try
            {
                var admin = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Permission.Delete &&
                                             u.Permission.Read &&
                                             u.Permission.Write &&
                                             u.Permission.Edit);

                if (admin == null)
                {
                    _logger.LogWarning("Admin user not found");
                    return NotFound(new { message = "Admin not found" });
                }

                return Ok(admin.ToDTO());
            }
            catch (DbException dbEx)
            {
                _logger.LogError(dbEx, "Database error occurred while retrieving admin info");
                return StatusCode(500, new { message = "A database error occurred" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while retrieving admin info");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [HttpGet("GetUsersWithEmployment")]
        public async Task<IActionResult> GetUsersWithEmployment()
        {
            try
            {
                var users = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.DoctorType != null && u.EmploymentPeriods.Any())
                    .ToListAsync();

                if (!users.Any())
                {
                    _logger.LogWarning("No users with employment found");
                    return NotFound(new { message = "No users in employment" });
                }

                return Ok(users.Select(u => u.ToDTO()));
            }
            catch (DbException dbEx)
            {
                _logger.LogError(dbEx, "Database error occurred while retrieving users with employment");
                return StatusCode(500, new { message = "A database error occurred" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while retrieving users with employment");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Create([FromBody] UserDTO dto)
        {
            try
            {
                if (dto == null)
                {
                    _logger.LogWarning("DTO was null");
                    return BadRequest(new { message = "User data wasn't provided" });
                }

                var user = dto.ToEntity();
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                return Ok(user.ToDTO());
            }
            catch (DbException dbEx)
            {
                _logger.LogError(dbEx, "Database error occurred while creating user");
                return StatusCode(500, new { message = "A database error occurred" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while creating user");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] User entity)
        {
            try
            {
                if (entity == null)
                {
                    _logger.LogWarning("DTO was null");
                    return BadRequest(new { message = "User data wasn't provided" });
                }

                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == entity.Id);

                if (existingUser == null)
                {
                    _logger.LogWarning("User {UserId} not found", entity.Id);
                    return NotFound(new { message = "User not found" });
                }

                existingUser.FullName = entity.FullName;
                existingUser.Email = entity.Email;
                existingUser.PhoneNumber = entity.PhoneNumber;
                existingUser.DepartmentId = entity.DepartmentId;
                existingUser.DoctorTypeId = entity.DoctorTypeId;

                await _context.SaveChangesAsync();

                return Ok(existingUser.ToDTO());
            }
            catch (DbException dbEx)
            {
                _logger.LogError(dbEx, "Database error occurred while updating user {UserId}", entity.Id);
                return StatusCode(500, new { message = "A database error occurred" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating user {UserId}", entity.Id);
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (id == default)
                {
                    _logger.LogWarning("Id was null");
                    return BadRequest(new { message = "Id wasn't provided" });
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found", id);
                    return NotFound(new { message = "User not found" });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(user.ToDTO());
            }
            catch (DbException dbEx)
            {
                _logger.LogError(dbEx, "Database error occurred while deleting user {UserId}", id);
                return StatusCode(500, new { message = "A database error occurred" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting user {UserId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }
    }
}