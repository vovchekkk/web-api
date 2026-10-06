using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : Controller
{
    // Чтобы ASP.NET положил что-то в userRepository требуется конфигурация
    private IUserRepository userRepository;
    
    public UsersController(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    [HttpGet("{userId}")]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        var user = userRepository.FindById(userId); // $"{src.LastName} {src.FirstName}"
        if (user == null)
            return NotFound();
        var fullName = $"{user.LastName} {user.FirstName}";

        var userDto = new UserDto
        {
            Id = userId,
            Login = user.Login,
            FullName = fullName,
            GamesPlayed = user.GamesPlayed,
            CurrentGameId = user.CurrentGameId
        };

        // userDto.Id = userId;
        // userDto.Login = user.Login;
        // userDto.FullName = fullName;
        // userDto.GamesPlayed = user.GamesPlayed;
        // userDto.CurrentGameId = user.CurrentGameId;

        return Ok(userDto);
    }

    [HttpPost]
    public IActionResult CreateUser([FromBody] object user)
    {
        throw new NotImplementedException();
    }
}