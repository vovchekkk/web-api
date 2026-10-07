using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : Controller
{
    // Чтобы ASP.NET положил что-то в userRepository требуется конфигурация
    private readonly IMapper mapper;
    private readonly IUserRepository userRepository;
    public UsersController(IMapper mapper, IUserRepository userRepository)
    {
        this.mapper = mapper;
        this.userRepository = userRepository;
    }

    [HttpGet("{userId}", Name = nameof(GetUserById))]
    [Produces("application/json", "application/xml")]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        var userEntity = userRepository.FindById(userId);
        if (userEntity == null)
            return NotFound();

        return Ok(mapper.Map<UserDto>(userEntity));
    }

    [HttpPost]
    public IActionResult CreateUser([FromBody] CreateUserDto? user)
    {
        if (user == null)
            return BadRequest();
        
        if (!string.IsNullOrEmpty(user.Login) &&
            !user.Login.All(char.IsLetterOrDigit))
        {
            ModelState.AddModelError(nameof(CreateUserDto.Login),
                "Login must contain only letters and digits.");
        }

        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);
        
        var userEntity = mapper.Map<UserEntity>(user);
        var createdUserEntity = userRepository.Insert(userEntity);
        var userDto = mapper.Map<UserDto>(createdUserEntity);
        
        return CreatedAtRoute(
            nameof(GetUserById),
            new { userId = createdUserEntity.Id },
            userDto);
    }
}