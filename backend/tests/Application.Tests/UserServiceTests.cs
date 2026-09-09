using System;
using System.Threading.Tasks;
using RuntimeInvestigation.Application.Features.Users;
using RuntimeInvestigation.Domain.Entities;
using RuntimeInvestigation.Infrastructure.Persistence.Repositories;
using Xunit;

namespace Application.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task CreateUser_Success_ReturnsTenantUser()
    {
        var repo = new InMemoryUserRepository();
        var service = new UserService(repo);

        var user = await service.CreateUserAsync("tenant-alpha", "alice@example.com", "Alice", UserRole.Investigator);

        Assert.NotNull(user);
        Assert.Equal("tenant-alpha", user.TenantId);
        Assert.Equal("alice@example.com", user.Email);
        Assert.Equal("Alice", user.Name);
        Assert.Equal(UserRole.Investigator, user.Role);
    }

    [Fact]
    public async Task CreateUser_DuplicateEmailInSameTenant_ThrowsInvalidOperationException()
    {
        var repo = new InMemoryUserRepository();
        var service = new UserService(repo);

        await service.CreateUserAsync("tenant-alpha", "duplicate@example.com", "User 1", UserRole.Viewer);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateUserAsync("tenant-alpha", "duplicate@example.com", "User 2", UserRole.Viewer));
    }

    [Fact]
    public async Task CreateUser_SameEmailDifferentTenant_Allowed()
    {
        var repo = new InMemoryUserRepository();
        var service = new UserService(repo);

        var u1 = await service.CreateUserAsync("tenant-1", "user@corp.com", "Corp User 1", UserRole.Viewer);
        var u2 = await service.CreateUserAsync("tenant-2", "user@corp.com", "Corp User 2", UserRole.Admin);

        Assert.NotNull(u1);
        Assert.NotNull(u2);
        Assert.Equal("tenant-1", u1.TenantId);
        Assert.Equal("tenant-2", u2.TenantId);
    }

    [Fact]
    public async Task GetUsersAsync_FiltersByTenant()
    {
        var repo = new InMemoryUserRepository();
        var service = new UserService(repo);

        await service.CreateUserAsync("tenant-a", "a1@example.com", "A1", UserRole.Viewer);
        await service.CreateUserAsync("tenant-a", "a2@example.com", "A2", UserRole.Admin);
        await service.CreateUserAsync("tenant-b", "b1@example.com", "B1", UserRole.Investigator);

        var usersA = await service.GetUsersAsync("tenant-a");
        Assert.Equal(2, usersA.Count);

        var usersB = await service.GetUsersAsync("tenant-b");
        Assert.Single(usersB);
    }

    [Fact]
    public async Task UpdateRoleAsync_UpdatesRoleCorrectly()
    {
        var repo = new InMemoryUserRepository();
        var service = new UserService(repo);

        var user = await service.CreateUserAsync("tenant-x", "promoteme@example.com", "Promotee", UserRole.Viewer);
        Assert.Equal(UserRole.Viewer, user.Role);

        var updated = await service.UpdateRoleAsync(user.Id, UserRole.Admin);
        Assert.NotNull(updated);
        Assert.Equal(UserRole.Admin, updated!.Role);

        var fetched = await service.GetByIdAsync(user.Id);
        Assert.Equal(UserRole.Admin, fetched!.Role);
    }
}
