using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Dtos;
using UserManagementApi.Models;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private static readonly List<User> Users =
    [
        new User { Id = 1, FullName = "Ali Raza", Email = "ali.raza@example.com", Role = "Admin", IsActive = true },
        new User { Id = 2, FullName = "Sara Khan", Email = "sara.khan@example.com", Role = "User", IsActive = true }
    ];

    private static readonly object LockObj = new();
    private static int _nextId = 3;

    [HttpGet]
    public ActionResult<IEnumerable<UserResponse>> GetAllUsers()
    {
        lock (LockObj)
        {
            var users = Users
                .Select(u => new UserResponse(u.Id, u.FullName, u.Email, u.Role, u.IsActive))
                .ToList();

            return Ok(users);
        }
    }

    [HttpGet("{id:int}")]
    public ActionResult<UserResponse> GetUserById(int id)
    {
        if (id <= 0)
            return BadRequest(new { message = "User ID must be a positive integer." });

        lock (LockObj)
        {
            var user = Users.FirstOrDefault(u => u.Id == id);
            if (user is null)
                return NotFound(new { message = $"User with ID {id} was not found." });

            return Ok(new UserResponse(user.Id, user.FullName, user.Email, user.Role, user.IsActive));
        }
    }

    [HttpPost]
    public ActionResult<UserResponse> CreateUser([FromBody] CreateUserRequest request)
    {
        lock (LockObj)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            if (Users.Any(u => u.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase)))
                return Conflict(new { message = "A user with this email already exists." });

            var user = new User
            {
                Id = _nextId++,
                FullName = request.FullName.Trim(),
                Email = normalizedEmail,
                Role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role.Trim(),
                IsActive = true
            };

            Users.Add(user);

            var response = new UserResponse(user.Id, user.FullName, user.Email, user.Role, user.IsActive);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, response);
        }
    }

    [HttpPut("{id:int}")]
    public ActionResult<UserResponse> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        if (id <= 0)
            return BadRequest(new { message = "User ID must be a positive integer." });

        lock (LockObj)
        {
            var user = Users.FirstOrDefault(u => u.Id == id);
            if (user is null)
                return NotFound(new { message = $"User with ID {id} was not found." });

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            if (Users.Any(u => u.Id != id && u.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase)))
                return Conflict(new { message = "Another user already uses this email." });

            user.FullName = request.FullName.Trim();
            user.Email = normalizedEmail;
            user.Role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role.Trim();
            user.IsActive = request.IsActive;

            return Ok(new UserResponse(user.Id, user.FullName, user.Email, user.Role, user.IsActive));
        }
    }

    [HttpDelete("{id:int}")]
    public IActionResult DeleteUser(int id)
    {
        if (id <= 0)
            return BadRequest(new { message = "User ID must be a positive integer." });

        lock (LockObj)
        {
            var user = Users.FirstOrDefault(u => u.Id == id);
            if (user is null)
                return NotFound(new { message = $"User with ID {id} was not found." });

            Users.Remove(user);
            return NoContent();
        }
    }
}