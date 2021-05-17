using Business.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_Api.Controllers
{
    [Authorize]
    [Route("api/[Controller]")]
    public class AmenityController : Controller
    {
        private readonly IAmenitRepository amenitRepository;
        public AmenityController(IAmenitRepository amenitRepository)
        {
            this.amenitRepository = amenitRepository;
        }
        
        [AllowAnonymous]
        [HttpGet]
        public async Task<IEnumerable<AmenityDTO>> GetAllAmenity()
        {
            return await amenitRepository.GetAllAmenity();
        }
    }
}
