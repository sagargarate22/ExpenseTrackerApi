using AutoMapper;
using Azure;
using ExpenseTrackerApi.Entities;
using ExpenseTrackerApi.Models.ApiResponse;
using ExpenseTrackerApi.Models.DTO;
using ExpenseTrackerApi.Services.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ExpenseTrackerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]

    public class RoleController : ControllerBase
    {
        private readonly IRoleServices _roleServices;
        private readonly IMapper _mapper;
        private readonly APIResponse _response;
        public RoleController(IRoleServices roleServices, IMapper mapper)
        {
            _roleServices = roleServices;
            _mapper = mapper;
            _response = new();
        }

        [HttpPost]
        [Route("Admin/CreateRole")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<APIResponse>> CreateRole(RoleDTO dto)
        {
            try
            {
                if(!ModelState.IsValid) return BadRequest(ModelState);

                var exitRole = await _roleServices.GetRecordAsync(role => role.RoleName == dto.RoleName);
                if(exitRole != null)
                {
                    _response.Status = false;
                    _response.StatusCode = HttpStatusCode.Conflict;
                    _response.Errors.Add($"The role {dto.RoleName} is alredy present");
                    return Conflict(_response);
                }

                Role role = _mapper.Map<Role>(dto);
                role.IsDeleted = false;
                role.CreatedAt = DateTime.UtcNow;
                role.UpdatedAt = DateTime.UtcNow;

                await _roleServices.CreateAsync(role);
                dto.RoleId = role.RoleId;
                _response.Status = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Data = dto;
                return Ok(_response);

            }catch (Exception e)
            {
                _response.Errors.Add(e.Message);
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return _response;
            }
        }

        [HttpGet]
        [Route("Admin/GetRoles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> GetRoles()
        {
            try
            {
                var roles = await _roleServices.GetRecordsAsync();
                _response.Status = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Data = roles;
                return Ok(_response);

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
        [Route("Admin/GetRole")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> GetRole(int id)
        {
            try
            {
                var role = await _roleServices.GetRecordAsync(role=>role.RoleId == id);
                if(role == null)
                {
                    _response.Status = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.Errors.Add($"Not Found Role with Id: {id}");
                    return NotFound(_response);
                }
                _response.Status = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Data = role;
                return Ok(_response);

            }
            catch (Exception e)
            {
                _response.Errors.Add(e.Message);
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return _response;
            }
        }

        [HttpPut]
        [Route("Super-Admin/UpdateRole")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<APIResponse>> UpdateRole(RoleDTO dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var exitRole = await _roleServices.GetRecordAsync(role => role.RoleName == dto.RoleName && !role.IsDeleted,true);
                if (exitRole == null)
                {
                    _response.Status = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.Errors.Add($"The role {dto.RoleName} is not present");
                    return NotFound(_response);
                }

                _mapper.Map(dto, exitRole);
                exitRole.UpdatedAt = DateTime.UtcNow;
                await _roleServices.CreateAsync(exitRole);
                dto.RoleId = exitRole.RoleId;
                _response.Status = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Data = dto;
                return Ok(_response);

            }
            catch (Exception e)
            {
                _response.Errors.Add(e.Message);
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return _response;
            }
        }

        [HttpDelete]
        [Route("Super-Admin/DeleteRole")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<APIResponse>> DeleteRole(int id)
        {
            try
            {
                if (id<=0) return BadRequest(ModelState);

                var exitRole = await _roleServices.GetRecordAsync(role => role.RoleId == id && !role.IsDeleted,true);
                if (exitRole == null)
                {
                    _response.Status = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.Errors.Add($"Not Found Role with Id: {id}");
                    return NotFound(_response);
                }

                exitRole.IsDeleted = true;
                exitRole.UpdatedAt = DateTime.UtcNow;

                await _roleServices.UpdateAsync(exitRole); 
                _response.Status = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Data = true;
                return Ok(_response);

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
