using AutoMapper;
using ExpenseTrackerApi.Models.ApiResponse;
using ExpenseTrackerApi.Services.BlobStorage;
using ExpenseTrackerApi.Services.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection.Metadata.Ecma335;
using System.Diagnostics;

namespace ExpenseTrackerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly APIResponse _response;
        private readonly IUserServices _userService;
        private readonly IBlobStorageService _blobStorageService;
        Stopwatch stopwatch = new Stopwatch();
        public UserController(IUserServices userServices, IMapper mapper, IBlobStorageService blobStorageService)
        
        {
            _userService = userServices;
            _mapper = mapper;
            _response = new();
            _blobStorageService = blobStorageService;
        }

        [HttpGet]
        [Route("GetAllUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetRecordsAsync();
                _response.Status = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Data = users;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.Errors.Add(ex.Message);
                return _response;
            }
        }

        [HttpGet]
        [Route("GetUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<APIResponse>> GetUser([FromQuery] string query)
        {
            try
            {

                var users = await _userService.GetRecordsAsync(
                    u => EF.Functions.Like(u.Username, $"%{query}%") ||
                    EF.Functions.Like(u.Email, $"%{query}%"));
                _response.Status = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Data = users;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.Errors.Add(ex.Message);
                return _response;
            }
        }

        [HttpPost]
        [Route("UploadImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> UploadImage(IFormFile file)
        {
            try
            {

                var userId = _userService.GetCurrentUserId();
                await using var stream = file.OpenReadStream();
                var blobClinet = await _blobStorageService.UploadRecord(stream,file.FileName);
                var user = await _userService.GetRecordAsync(u => u.UserId == userId,true);
                if (user == null)
                {
                    _response.Errors.Add($"Not found user with the userId: {userId}");
                    _response.Status=false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                user.ImagePath = blobClinet.Uri.ToString();
                await _userService.UpdateAsync(user);
                _response.Status = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Data = blobClinet;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.Errors.Add(ex.Message);
                return _response;
            }
        }

        [HttpPut]
        [Route("UpdateImage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> UpdateImage(IFormFile file)
        {
            try
            {
                stopwatch.Start();
                var userId = _userService.GetCurrentUserId();
                var user = await _userService.GetRecordAsync(u => u.UserId == userId,true);

                if (user == null)
                {
                    _response.Errors.Add($"Not found user with the userId: {userId}");
                    _response.Status = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                if (!string.IsNullOrEmpty(user.ImagePath))
                {
                    var image = Uri.UnescapeDataString(user.ImagePath!);
                    var splitPath = image.Split('/');
                    string fileName = splitPath[splitPath.Length - 1];
                    await _blobStorageService.DeleteRecord(fileName);
                }

                await using var stream = file.OpenReadStream();

                var blobClinet = await _blobStorageService.UploadRecord(stream, file.FileName);
                user.ImagePath = blobClinet.Uri.AbsoluteUri;

                await _userService.UpdateAsync(user);

                _response.Status = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Data = blobClinet;
                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                _response.Errors.Add($"Time Taken: {elapsedTime}");
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.Errors.Add(ex.Message);
                return _response;
            }
        }

        [HttpGet]
        [Route("Download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Download()
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                var user = await _userService.GetRecordAsync(u => u.UserId == userId, true);
                if (user == null)
                {
                    _response.Errors.Add($"Not found user with the userId: {2}");
                    _response.Status = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                if (string.IsNullOrEmpty(user.ImagePath))
                {
                    return BadRequest();
                }

                var image = Uri.UnescapeDataString(user.ImagePath);
                var splitPath = image.Split('/');
                string fileName = splitPath[splitPath.Length - 1];
                var result = await _blobStorageService.GetRecord(fileName);
                //_response.Status = true;
                //_response.StatusCode = HttpStatusCode.OK;
                //_response.Data = true;
                //return Ok(_response);
                return File(result.Content,result.ContentType,result.Name);
            }
            catch (Exception ex)
            {
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.Errors.Add(ex.Message);
                return NotFound(_response);

            }
        }

        [HttpDelete]
        [Route("Delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> Delete()
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                var user = await _userService.GetRecordAsync(u => u.UserId == userId, true);
                if (user == null)
                {
                    _response.Errors.Add($"Not found user with the userId: {2}");
                    _response.Status = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                if (string.IsNullOrEmpty(user.ImagePath))
                {
                    return BadRequest();
                }
                var image = Uri.UnescapeDataString(user.ImagePath);
                var splitPath = image.Split('/');
                string fileName = splitPath[splitPath.Length - 1];
                await _blobStorageService.DeleteRecord(fileName);
                user.ImagePath = null!;
                await _userService.UpdateAsync(user);
                _response.Status = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Data = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.Errors.Add(ex.Message);
                return _response;

            }
        }

        [HttpGet]
        [Route("Test")]
        public ActionResult GetUser()
        {
            var users = _userService.GetUsers();
            return Ok(users);
        }
    }
}
