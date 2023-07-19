using Implementing_Caching_In_WebAPI_With_Redis.Data_Access_Layer;
using Implementing_Caching_In_WebAPI_With_Redis.Models;
using Implementing_Caching_In_WebAPI_With_Redis.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Implementing_Caching_In_WebAPI_With_Redis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DriverController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<DriverController> _logger;
        private readonly ICacheService _cacheService;
        private readonly DriverDbContext _driverContext;

        public DriverController(ILogger<DriverController> logger,
                                ICacheService cacheService,
                                DriverDbContext driverContext)
        {
            _logger = logger;
            _cacheService = cacheService;
            _driverContext = driverContext;
        }

        [HttpGet("drivers")]
        public async Task<IActionResult> GetDrivers()
        {
            //Check information from the cache
            var cacheData = _cacheService.GetData<IEnumerable<Driver>>("drivers");
            if(cacheData != null && cacheData.Count() > 0)
            {
                return Ok(cacheData);
            }

            cacheData = await _driverContext.Drivers.ToListAsync();

            //Set expiry date
            var expiryTime = DateTimeOffset.Now.AddSeconds(30);
            _cacheService.SetData<IEnumerable<Driver>>("drivers", cacheData, expiryTime);

            return Ok(cacheData);
        }

        [HttpPost("AddDriver")]
        public async Task<IActionResult> PostDriver(Driver value)
        {
            var addObj = await _driverContext.Drivers.AddAsync(value);

            var expiryTime = DateTimeOffset.Now.AddSeconds(30);
            _cacheService.SetData<Driver>($"driver{value.DriverId}", addObj.Entity, expiryTime);

            await _driverContext.SaveChangesAsync();

            return Ok(addObj.Entity);
        }

        [HttpPost("DeleteDriver")]
        public async Task<IActionResult> DeleteDriver(int id)
        {
            var driverToDelete = await _driverContext.Drivers.FirstOrDefaultAsync(x => x.DriverId == id);
            if(driverToDelete != null)
            {
                _driverContext.Remove(driverToDelete);
                _cacheService.RemoveData($"driver{id}");

                await _driverContext.SaveChangesAsync();
                return NoContent();
            }

            return NotFound();
        }

    }
}
