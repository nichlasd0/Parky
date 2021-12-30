using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkyAPI.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ParkyAPI.Models.DTO;
using ParkyAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace ParkyAPI.Controllers
{
    [Route("api/v{version:apiVersion}/trails")]
    [ApiController]
    //[ApiExplorerSettings(GroupName = "ParkyOpenAPISpecTrails")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class TrailsController : ControllerBase
    {
        private readonly ITrailRepository _trailRepo;
        private readonly IMapper _mapper;

        public TrailsController(ITrailRepository trailRepo, IMapper mapper)
        {
            _trailRepo = trailRepo;
            _mapper = mapper;
        }

        /// <summary>
        /// Get list of trails.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<TrailDto>))]
        public IActionResult GetTrails()
        {
            var objList = _trailRepo.GetTrails();

            var objDto = new List<TrailDto>();
            foreach (var obj in objList)
            {
                objDto.Add(_mapper.Map<TrailDto>(obj));
            }
            return Ok(objDto);
        }
        /// <summary>
        /// Get individual trail
        /// </summary>
        /// <param name="trailId"> The Id of the trail</param>
        /// <returns></returns>
        [HttpGet("{trailId:int}", Name = "GetTrail")]
        [ProducesResponseType(200, Type = typeof(TrailDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesDefaultResponseType]
        [Authorize(Roles = "Admin")]
        public IActionResult GetTrail(int trailId)
        {
            var obj = _trailRepo.GetTrail(trailId);
            if (obj == null)
            {
                return NotFound();
            }

            var objDto = _mapper.Map<TrailDto>(obj);

            return Ok(objDto);
        }
        [HttpGet("[action]/{trailId:int}")]
        [ProducesResponseType(200, Type = typeof(TrailDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesDefaultResponseType]
        public IActionResult GetTrailInNationalPark(int nationalParkId)
        {
            var objList = _trailRepo.GetTrailsInNationalPark(nationalParkId);
            if (objList == null)
            {
                return NotFound();
            }
            var objDto = new List<TrailDto>();
            foreach(var obj in objList)
            {
                objDto.Add(_mapper.Map<TrailDto>(obj));
            }


            return Ok(objDto);
        }


        [HttpPost]
        [ProducesResponseType(201, Type = typeof(TrailDto))]
        [ProducesResponseType(404)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesDefaultResponseType]
        public IActionResult CreateTrail([FromBody] TrailCreateDto TrailDto)
        {
            if (TrailDto == null)
            {
                return BadRequest(ModelState);
            }
            if (_trailRepo.TrailExists(TrailDto.Name))
            {
                ModelState.AddModelError("", "Trail exists!");
                return StatusCode(404, ModelState);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var TrailObj = _mapper.Map<Trail>(TrailDto);

            if(!_trailRepo.CreateTrail(TrailObj))
            {
                ModelState.AddModelError("", $"Something whent wrong when saving the record {TrailObj.Name}");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetTrail", new { TrailId = TrailObj.Id}, TrailObj);
        }

        [HttpPatch("{TrailId:int}", Name = "UpdateTrail")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateTrail(int TrailId, [FromBody] TrailUpdateDto TrailDto)
        {
            if (TrailDto == null || TrailId != TrailDto.Id)
            {
                return BadRequest(ModelState);
            }
            var TrailObj = _mapper.Map<Trail>(TrailDto);

            if (!_trailRepo.UpdateTrail(TrailObj))
            {
                ModelState.AddModelError("", $"Something whent wrong when updating the record {TrailObj.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [HttpDelete("{TrailId:int}", Name = "DeleteTrail")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteTrail(int TrailId)
        {
            if (!_trailRepo.TrailExists(TrailId))
            {
                return NotFound();
            }
            var TrailObj = _trailRepo.GetTrail(TrailId);

            if (!_trailRepo.DeleteTrail(TrailObj))
            {
                ModelState.AddModelError("", $"Something whent wrong when deleting the record {TrailObj.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}
