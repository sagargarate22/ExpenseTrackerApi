using AutoMapper;
using ExpenseTrackerApi.Entities;
using ExpenseTrackerApi.Models.ApiResponse;
using ExpenseTrackerApi.Models.DTO;
using ExpenseTrackerApi.Services.Email.Interface;
using ExpenseTrackerApi.Services.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Security.Claims;

namespace ExpenseTrackerApi.Controllers
{

    public enum Filter
    {
        Weekly = 1,
        Monthly = 2,
        Custom = 3
    }

    public enum Categories
    {
        Groceries = 1,
        Leisure = 2,
        Electronics = 3,
        Utilities = 4,
        Clothing = 5,
        Health = 6,
        Others = 7
    }


    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExpenseController : ControllerBase
    {
        private readonly APIResponse _response;
        private readonly IMapper _mapper;
        private readonly IExpenseService _expenseService;
        private readonly IUserServices _userService;
        private readonly IEmailServiceProvider _emailServiceProvider;
        public ExpenseController(IMapper mapper, IUserServices user, IExpenseService expense,IEmailServiceProvider emailServiceProvider)
        {
            _mapper = mapper;
            _expenseService = expense;
            _userService = user;
            _response = new APIResponse();
            _emailServiceProvider = emailServiceProvider;
        }

        [HttpPost]
        [Route("AddExpense")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> AddExpense(ExpenseDTO dto)
        {
            try
            {
                if(!ModelState.IsValid) return BadRequest(ModelState);
                var userId = _userService.GetCurrentUserId();
                if(userId <= 0)
                {
                    _response.Status = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Errors.Add("User not found");
                    return BadRequest(ModelState);
                }

                Expenses expenses = _mapper.Map<Expenses>(dto);
                expenses.CreatedAt = DateTime.UtcNow;
                expenses.UpdateAt = DateTime.UtcNow;
                expenses.ExpenseCategoryId = dto.ExpenseType;
                expenses.UserId = userId;
                await _expenseService.CreateAsync(expenses);

                _response.StatusCode = HttpStatusCode.OK;
                _response.Status = true;
                _response.Data = expenses;
                return Ok(_response);
            }
            catch (Exception e)
            {
                _response.Errors.Add(e.Message);
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.OK;
                return _response;
            }
        }

        [HttpDelete]
        [Route("DeleteExpense")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> DeleteExpense([FromQuery] int id)
        {
            try
            {
                if (id <= 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Status = false;
                    _response.Errors.Add("Invalid expense id");
                    return BadRequest(_response);   
                }
                var userId = _userService.GetCurrentUserId();
                var expense = await _expenseService.GetRecordAsync(e => e.ExpensesId == id && e.UserId == userId);
                if(expense == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.Status = false;
                    _response.Errors.Add("Expense not found");
                    return NotFound(_response);
                }

                await _expenseService.DeleteAsync(expense);

                _response.StatusCode = HttpStatusCode.OK;
                _response.Status = true;
                _response.Data = true;
                return Ok(_response);
            }
            catch (Exception e)
            {
                _response.Errors.Add(e.Message);
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.OK;
                return _response;
            }
        }

        [HttpPut]
        [Route("UpdateExpense")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> UpdateExpense([FromBody] ExpensesUpdateDTO dto) 
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var userId = _userService.GetCurrentUserId();
                var expense = await _expenseService.GetRecordAsync(e => e.ExpensesId == dto.ExpensesId && e.UserId == userId,true);
                if(expense == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.Status = false;
                    _response.Errors.Add("Expense not found");
                    return NotFound(_response);
                }

                _mapper.Map(dto, expense);
                expense.UpdateAt = DateTime.UtcNow;    
                await _expenseService.UpdateAsync(expense);
                _response.Status = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Data = expense;
                return Ok(_response);
            }
            catch (Exception e)
            {
                _response.Errors.Add(e.Message);
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.OK;
                return _response;
            }
        }

        [HttpGet]
        [Route("GetExpenseByDate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<ActionResult<APIResponse>> GetExpenseByDate([FromQuery] Filter filter, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            try
            {
                var currentDate = DateTime.UtcNow.Date;
                var startDate = DateTime.UtcNow.Date;
                if(filter == Filter.Weekly)
                {
                    startDate = _expenseService.GetStartOfWeek(startDate);
                }else if(filter == Filter.Monthly)
                {
                    startDate = _expenseService.GetStartOfMonth(startDate);
                }else if(filter == Filter.Custom)
                {
                    startDate = start;
                    currentDate = end;
                }
                var userId = _userService.GetCurrentUserId();
                var expense = await _expenseService.GetRecordsAsync(e => (e.ExpenseDate.Date >= startDate && e.ExpenseDate.Date <= currentDate) && e.UserId == userId);
                _response.Status = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Data = expense;
                return Ok(_response);
            }
            catch (Exception e)
            {
                _response.Errors.Add(e.Message);
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.OK;
                return _response;
            }
        }


        [HttpGet]
        [Route("GetExpenseByCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> GetExpenseByCategory([FromQuery] Categories categories)
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                var categoryId = (int)categories;
                var expense = await _expenseService.GetRecordsAsync(e=>e.ExpenseCategoryId == categoryId && e.UserId == userId);
                _response.Status = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Data = expense;
                return Ok(_response);
            }
            catch (Exception e)
            {
                _response.Errors.Add(e.Message);
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.OK;
                return _response;
            }
        }

        [HttpGet]
        [Route("GetTodayExpense")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> GetTodayExpense()
        {
            try
            {
                var userId = _userService.GetCurrentUserId();
                //var userId = 2;
                var currentDate = DateTime.UtcNow;
                var expense = await _expenseService.GetRecordsAsync(e => e.ExpenseDate.Date == currentDate.Date && e.UserId == userId);
                _response.Status = true;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Data = expense;
                return Ok(_response);
            }
            catch (Exception e)
            {
                _response.Errors.Add(e.Message);
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.OK;
                return _response;
            }
        }

        [HttpGet]
        [Route("test")]
        [AllowAnonymous]
        public async Task<ActionResult<APIResponse>> Test()
        {
            try
            {
                //var currDate = DateTime.Now.Date;
                //var statDate = Da
                await _emailServiceProvider.SendMonthlyReport();
                await _emailServiceProvider.SendDailyReport();
                //var monthlyReport = await _expenseService.GetRecordsAsync(e => (e.ExpenseDate >= startDate && e.ExpenseDate <= currDate) && e.UserId == user.UserId, true);
                _response.Status = true;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception e)
            {
                _response.Errors.Add(e.Message);
                _response.Status = false;
                _response.StatusCode = HttpStatusCode.OK;
                return _response;
            }
        }
    }
}
