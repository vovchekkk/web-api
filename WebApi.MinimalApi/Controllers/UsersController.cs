using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : Controller
{
    private readonly IUserRepository userRepository;

    public UsersController(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    [HttpGet("{userId}")]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    public IActionResult CreateUser([FromBody] object user)
    {
        throw new NotImplementedException();
    }

    [HttpPut("{id}")]
    public ActionResult<UserDto> UpdateUser(
        string id,
        [FromBody] UpdateUserDto? user
    )
    {
        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var parsedId))
            return BadRequest();

        if (user == null)
            return BadRequest();

        if (string.IsNullOrEmpty(user.Login))
            return UnprocessableEntity(new
            {
                login = new[] { "Login is required" }
            });
        
        if (user.Login.Any(c => !char.IsLetter(c)))
            return UnprocessableEntity(new
            {
                login = new[] { "Login can only contain letters" }
            });
        
        if (string.IsNullOrEmpty(user.FirstName))
            return UnprocessableEntity(new
            {
                firstName = new[] { "First name is required" }
            });
        
        if (string.IsNullOrEmpty(user.LastName))
            return UnprocessableEntity(new
            {
                lastName = new[] { "Last name is required" }
            });
        
        var userEntity = userRepository.FindById(parsedId);

        if (userEntity == null)
        {
            userEntity = new UserEntity(parsedId);
        }

        userEntity.Login = user.Login;
        userEntity.FirstName = user.FirstName;
        userEntity.LastName = user.LastName;

        userRepository.UpdateOrInsert(userEntity, out var isInserted);

        if (isInserted)
        {
            return Created(
                $"/users/{userEntity.Id}",
                new { Id = userEntity.Id.ToString() }
            );
        }

        return NoContent();
    }
}