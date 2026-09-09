using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuntimeInvestigation.Application.Common.Interfaces;
using RuntimeInvestigation.Application.Features.Users;
using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ITenantContext _tenantContext;

    public UsersController(UserService userService, ITenantContext tenantContext)
    {
        _userService = userService;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Investigator")]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _userService.GetUsersAsync(_tenantContext.TenantId, cancellationToken);
        var result = users.Select(u => new UserResponse(u.Id, u.TenantId, u.Email, u.Name, u.Role.ToString(), u.CreatedAt)).ToList();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { error = "Email is required." });
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
        {
            role = UserRole.Viewer;
        }

        try
        {
            var user = await _userService.CreateUserAsync(_tenantContext.TenantId, request.Email, request.Name ?? "", role, cancellationToken);
            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, new UserResponse(user.Id, user.TenantId, user.Email, user.Name, user.Role.ToString(), user.CreatedAt));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserResponse>> UpdateRole(string id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
        {
            return BadRequest(new { error = $"Invalid role '{request.Role}'." });
        }

        var user = await _userService.UpdateRoleAsync(id, role, cancellationToken);
        if (user == null)
        {
            return NotFound(new { error = $"User '{id}' not found." });
        }

        return Ok(new UserResponse(user.Id, user.TenantId, user.Email, user.Name, user.Role.ToString(), user.CreatedAt));
    }
}

public sealed record CreateUserRequest(string Email, string? Name, string? Role);
public sealed record UpdateRoleRequest(string Role);
public sealed record UserResponse(string Id, string TenantId, string Email, string Name, string Role, DateTime CreatedAt);
