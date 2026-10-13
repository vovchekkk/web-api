using System.Buffers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.WebHost.UseUrls("http://localhost:5000");
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options => {
        options.SuppressModelStateInvalidFilter = true;
        options.SuppressMapClientErrors = true;
    });
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.CreateMap<UserEntity, WebApi.MinimalApi.Models.UserDto>()
        .ForMember(d => d.FullName, opt => opt.MapFrom(s => $"{s.LastName} {s.FirstName}"));
}, Array.Empty<System.Reflection.Assembly>());

builder.Services.AddAutoMapper(cfg =>
{
    cfg.CreateMap<UserEntity, UserDto>()
        .ForMember(dest => dest.FullName, 
            opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
    cfg.CreateMap<CreateUserDto, UserEntity>();
}, new System.Reflection.Assembly[0]);

builder.Services.AddControllers(options =>
    {
        options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
        options.OutputFormatters.Insert(0, new
            NewtonsoftJsonOutputFormatter(new JsonSerializerSettings
            {
                ContractResolver = new
                    CamelCasePropertyNamesContractResolver()
            }, ArrayPool<char>.Shared, options));
        options.ReturnHttpNotAcceptable = true;
        options.RespectBrowserAcceptHeader = true;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
        options.SuppressMapClientErrors = true;
    })
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Populate;
    });


var app = builder.Build();

app.MapControllers();

app.Run();