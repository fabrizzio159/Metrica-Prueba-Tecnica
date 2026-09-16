using AuthService.Data;
using AuthService.Interfaces;
using AuthService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    private readonly AuthDbContext _dbContext;

    public UserRepository(AuthDbContext context) : base(context)
    {
        _dbContext = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }
}
