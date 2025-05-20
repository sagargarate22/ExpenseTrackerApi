using AutoMapper;
using ExpenseTrackerApi.Entities;
using ExpenseTrackerApi.Models.ApiResponse;
using ExpenseTrackerApi.Models.DTO;
using ExpenseTrackerApi.Services.Email.Interface;
using ExpenseTrackerApi.Services.JWTAuthentication;
using ExpenseTrackerApi.Services.PasswordEncrypter;
using ExpenseTrackerApi.Services.Repository.IRepository;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Net;

namespace ExpenseTrackerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserServices _userService;
        private readonly IUserRoleMappingServices _userRoleService;
        private readonly IRoleServices _roleService;
        private readonly IEmailServiceProvider _emailServiceProvider;
        private readonly INonReversiblePasswordHasher _passwordHasher;
        private readonly string _pepperKey;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly APIResponse _response;
        private readonly ITokenManager _tokenManager;
        private readonly IEmailServices _emailServices;
        public AuthController(IRoleServices roleService,
            IUserRoleMappingServices userRoleService,
            IUserServices userServices,
            IMapper mapper,
            INonReversiblePasswordHasher passwordHasher,
            IConfiguration configuration,
            ITokenManager tokenManager,
            IEmailServices emailServices,
            IBackgroundJobClient backgroundJobClient,
            IEmailServiceProvider emailServiceProvider)
        {
            _userRoleService = userRoleService;
            _roleService = roleService;
            _userService = userServices;
            _mapper = mapper;
            _response = new();
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _pepperKey = _configuration["ApplicationSecret:PepperKey"]!;
            _tokenManager = tokenManager;
            _emailServices = emailServices;
            _emailServiceProvider = emailServiceProvider;
        }

        [HttpPost]
        public async Task<ActionResult<APIResponse>> Login(LoginDTO dto)
        {
            try
            {
                if(!ModelState.IsValid) return BadRequest(ModelState);

                var user = await _userService.GetRecordAsync(user => user.Email == dto.Email);
                if(user == null)
                {
                    _response.Status = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.Errors.Add($"Not found user with the email {dto.Email}");
                    return NotFound(_response);
                }

                string passwordSalt = user.passwordSalt;
                string passwordHash = user.PasswordHash;

                string computePasswordHash = _passwordHasher.ComputeHash(dto.Password, passwordSalt, _pepperKey, 2);
                if(computePasswordHash == passwordHash)
                {
                    LoginResponseDTO responseDTO = new LoginResponseDTO()
                    {
                        Email = dto.Email,
                    };
                    var userRole = await _userRoleService.GetRecordAsync(u => u.UserId == user.UserId);
                    var role = await _roleService.GetRecordAsync(u => u.RoleId == userRole.RoleId);
                    var userCliam = _tokenManager.GenerateClaims(user, role.RoleName);
                    var token = _tokenManager.GenerateToken(userCliam);

                    responseDTO.Token = token;
                    _response.Status = true;
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.Data = responseDTO;
                    return Ok(_response);
                }
                else
                {
                    _response.Status = false;
                    _response.StatusCode = HttpStatusCode.Unauthorized;
                    _response.Errors.Add("Invalid Password for the user");
                    return Unauthorized(_response);
                }
            }
            catch (Exception e)
            {
                _response.Errors.Add(e.Message);
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return _response;
            }
        }

        [HttpPost]
        [Route("Register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<APIResponse>> Register(RegisterDTO dto)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(dto, $"The argument {nameof(dto)} is required");

                var user = await _userService.GetRecordAsync(user => user.Username == dto.Username && user.Email == dto.Email);
                if (user == null)
                {
                    Users newUser = _mapper.Map<Users>(dto);
                    newUser.IsDeleted = false;
                    newUser.CreatedAt = DateTime.Now;
                    newUser.UpdatedAt = DateTime.Now;

                    string passwordSalt = _passwordHasher.GenerateSalt();
                    string passwordHash = _passwordHasher.ComputeHash(dto.Password, passwordSalt, _pepperKey, 2);

                    newUser.passwordSalt = passwordSalt;
                    newUser.PasswordHash = passwordHash;

                    await _userService.CreateAsync(newUser);

                    var role = await _roleService.GetRecordAsync(role => role.RoleName == dto.Role);

                    UserRoleMapping mapping = new UserRoleMapping()
                    {
                        RoleId = role.RoleId,
                        UserId = newUser.UserId
                    };

                    await _userRoleService.CreateAsync(mapping);

                    var result = await _emailServiceProvider.SendVerifyEmail(newUser.Email);
                    result();
                    _response.Status = true;
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.Data = newUser;
                    return Ok(_response);
                }
                else
                {
                    _response.Status = false;
                    _response.StatusCode = HttpStatusCode.Conflict;
                    _response.Data = "There is an already user present with the same username and email";
                    return Conflict(_response);
                }
            }catch (Exception e)
            {
                _response.Errors.Add(e.Message);
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return _response;
            }
        }

        [HttpPost]
        [Route("VerifyOtp")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> VerifyOtp(string Otp)
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                var user = await _userService.GetRecordAsync(user => user.UserId == userId);
                if (user == null)
                {
                    _response.Status = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.Errors.Add($"Not found user with the id {userId}");
                    return NotFound(_response);
                }
                bool result = await _emailServiceProvider.VerifyOTP(user.Email, Otp);
                if (result)
                {
                    user.IsActive = true;
                    _response.Status = true;
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.Data = true;
                    return Ok(_response);
                }
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Errors.Add($"otp does not match");
                return BadRequest(_response);
            }
            catch (Exception e)
            {
                _response.Errors.Add(e.Message);
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return _response;
            }
        }
        [HttpGet]
        public async Task<ActionResult<APIResponse>> VerifyEmail()
        {
            try
            {
                await _emailServiceProvider.SendVerifyEmail("sagargarate22@gmail.com");
                return Ok();
            }
            catch (Exception e)
            {
                _response.Errors.Add(e.Message);
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return _response;
            }
        }
    }
}
