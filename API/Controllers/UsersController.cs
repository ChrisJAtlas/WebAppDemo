using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]     // /api/users
public class UsersController(DataContext context) : ControllerBase
{

    [HttpGet]
    public async Task <ActionResult<IEnumerable<AppUser>>> GetUsers()
    {
        var users = await context.Users.ToListAsync();
        return Ok(users);
    }

    [HttpGet("{id:int}")]   // /api/users/1
    public async Task <ActionResult<AppUser>> GetUsers(int id)
    {
        var users = await context.Users.FindAsync(id);

        if (users == null) return NotFound();

        return Ok(users);
    }
}