using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : Controller
{
    private readonly IMapper mapper;
    private readonly IUserRepository userRepository;
    private readonly LinkGenerator linkGenerator;

    public UsersController(
        IUserRepository userRepository,
        IMapper mapper,
        LinkGenerator linkGenerator
    )
    {
        this.mapper = mapper;
        this.userRepository = userRepository;
        this.linkGenerator = linkGenerator;
    }

    [HttpGet("{userId}", Name = nameof(GetUserById))]
    [HttpHead("{userId}")]
    [Produces("application/json", "application/xml")]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        var user = userRepository.FindById(userId);
        if (user == null)
            return NotFound();

        if (HttpMethods.IsHead(Request.Method))
        {
            Response.ContentType = "application/json; charset=utf-8";
            return Ok();
        }
        
        var userDto = mapper.Map<UserDto>(user);
        userDto.Id = userId;

        return Ok(userDto);
    }

    [HttpOptions]
    public IActionResult GetUsersOptions()
    {
        Response.Headers.Add("Allow", "POST, GET, OPTIONS");
        return Ok();
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
            new { userId = userDto.Id },
            userDto);
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

    [HttpDelete("{userId}")]
    public IActionResult DeleteUser(string userId)
    {
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var parsedId))
            return NotFound();

        var userEntity = userRepository.FindById(parsedId);
        if (userEntity == null)
            return NotFound();

        userRepository.Delete(parsedId);

        return NoContent();
    }

    [HttpPatch("{id}")]
    [Produces("application/json", "application/xml")]
    public IActionResult PartiallyUpdateUser(
        string id,
        [FromBody] JsonPatchDocument<UpdateUserDto>? patchDoc)
    {
        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var parsedId))
            return NotFound();
        
        if (patchDoc == null)
            return BadRequest();

        var userEntity = userRepository.FindById(parsedId);
        if (userEntity == null)
            return NotFound();
        
        var userToUpdate = mapper.Map<UpdateUserDto>(userEntity);
        patchDoc.ApplyTo(userToUpdate, ModelState);
        
        TryValidateModel(userToUpdate);
        
        if (!string.IsNullOrEmpty(userToUpdate.Login) &&
            !userToUpdate.Login.All(char.IsLetterOrDigit))
        {
            ModelState.AddModelError("login",
                "Login must contain only letters and digits.");
        }
        
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);
        
        mapper.Map(userToUpdate, userEntity);
        userRepository.Update(userEntity);
        
        return NoContent();
    }
    
    [HttpGet(Name = nameof(GetUsers))]
    public ActionResult<IEnumerable<UserDto>> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10
    )
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 20);

        var pageList = userRepository.GetPage(pageNumber, pageSize);

        var previousPageLink = pageList.HasPrevious
            ? linkGenerator.GetUriByRouteValues(HttpContext, nameof(GetUsers), new { pageNumber = pageList.CurrentPage - 1, pageSize = pageList.PageSize })
            : null;

        var nextPageLink = pageList.HasNext
            ? linkGenerator.GetUriByRouteValues(HttpContext, nameof(GetUsers), new { pageNumber = pageList.CurrentPage + 1, pageSize = pageList.PageSize })
            : null;

        var paginationHeader = new
        {
            previousPageLink,
            nextPageLink,
            totalCount = pageList.TotalCount,
            pageSize = pageList.PageSize,
            currentPage = pageList.CurrentPage,
            totalPages = pageList.TotalPages
        };

        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(paginationHeader));

        var users = mapper.Map<IEnumerable<UserDto>>(pageList);

        return Ok(users);
    }
}