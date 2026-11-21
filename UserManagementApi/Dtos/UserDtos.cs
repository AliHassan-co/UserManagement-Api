using System.ComponentModel.DataAnnotations;

namespace UserManagementApi.Dtos;

public record CreateUserRequest(
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters.")]
    string FullName,

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [StringLength(150)]
    string Email,

    [StringLength(30)]
    string Role = "User"
);

public record UpdateUserRequest(
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters.")]
    string FullName,

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [StringLength(150)]
    string Email,

    [StringLength(30)]
    string Role = "User",

    bool IsActive = true
);

public record UserResponse(
    int Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive
);