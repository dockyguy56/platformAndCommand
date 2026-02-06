using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers
{
    [Route("api/c/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly IMapper _mapper;

        public PlatformsController(IMapper mapper)
        {
            _mapper = mapper;
        }

        [HttpPost]
        public ActionResult GetPlatforms()
        {
            Console.WriteLine("--> Platforms Posted to CommandsService");
            return Ok("Inbound POST # Command Service");
        }
    }
}