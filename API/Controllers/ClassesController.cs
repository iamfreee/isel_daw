using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Data.Contracts;
using API.Models;
using API.Services;
using API.TransferModels.InputModels;
using API.TransferModels.ResponseModels;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class ClassesController : Controller
    {
        private IClassRepository _repo;

        private readonly ClassesSirenHto _representation;

        public ClassesController(IClassRepository repo, ClassesSirenHto representation)
        {
            _repo = repo;
            _representation = representation;
        }

        [HttpGet("", Name=Routes.ClassList)]
        public async Task<IActionResult> GetAll([FromQuery] ListQueryStringDto query)
        {
            PagedList<Class> classes = await _repo.GetAllPaginatedAsync(query);

            return Ok(_representation.Collection(classes, query));
        }

        [HttpGet("{id}", Name=Routes.ClassEntry)]
        public async Task<IActionResult> Get(int Id)
        {

            Class c = await _repo.GetByIdAsync(Id);

            if(c == null){
                return NotFound();
            }

            return Ok(_representation.Entity(c));
        }

        [HttpPost("", Name=Routes.ClassCreate)]
        //[Authorize(Roles = Roles.Admin)] //TODO: Authorization only for admins
        public async Task<IActionResult> Post([FromBody]ClassDTO dto)
        {
            if(!ModelState.IsValid){
                return BadRequest(ModelState);
            }

            //TODO: AutoMapper
            Class c = new Class{
                Name = dto.Name,
                MaxGroupSize = dto.MaxGroupSize,
                AutoEnrollment = dto.AutoEnrollment,
                SemesterId = dto.SemesterId,
                CourseId = dto.CourseId,
            };

            if(await _repo.AddAsync(c)){
                return CreatedAtRoute(
                    Routes.ClassEntry,
                    new { id = c.Id },
                    _representation.Entity(c));
            }

            throw new Exception("Unable to add Class");
        }

        [HttpPut("{id}", Name=Routes.ClassEdit)]
        //[Authorize(Roles = Roles.Admin)] //TODO: Authorization only for admins
        public async Task<IActionResult> Put(int Id, [FromBody]ClassDTO dto)
        {
            if(!ModelState.IsValid){
                return BadRequest(ModelState);
            }

            Class c = await _repo.GetByIdAsync(Id);
            if(c == null)
            {
                return NotFound();
            }

            //TODO: Mapper
            c.Name = dto.Name;
            c.MaxGroupSize = dto.MaxGroupSize;
            c.AutoEnrollment = dto.AutoEnrollment;
            c.SemesterId = dto.SemesterId;
            c.CourseId = dto.CourseId;

            if(await _repo.EditAsync(c)){
                return Ok(_representation.Entity(c));
            }

            throw new Exception("Unbale to edit Class " + Id);
        }

        [HttpDelete("{id}", Name=Routes.ClassDelete)]
        //[Authorize(Roles = Roles.Admin)] //TODO: Authorization only for admins
        public async Task<IActionResult> Delete(int Id)
        {
            Class c = await _repo.GetByIdAsync(Id);
            if(c == null)
            {
                return NotFound();
            }

            if(await _repo.DeleteAsync(c)){
                return NoContent();
            }

            throw new Exception("Unable to delete Class " + Id);
        }

        [HttpGet("{id}/groups", Name=Routes.ClassGroupsList)]
        public async Task<IActionResult> ClassGroups(int Id, [FromQuery]ListQueryStringDto query)
        {
            PagedList<Group> groups = await _repo.GetClassGroups(Id, query);

            return Ok(_representation.ClassGroupsCollection(groups, query));
        }

        [HttpPost("{id}/student", Name=Routes.ClassParticipantAdd)]
        public async Task<IActionResult> AddParticipant(int Id, [FromBody]StudentDTO dto)
        {
            if(!ModelState.IsValid){
                return BadRequest(ModelState);
            }

            Class c = await _repo.GetByIdAsync(Id);
            if(c == null){
                return NotFound();
            }

            if(await _repo.AddParticipantTo(c, dto.Number)){
                return Ok();    //TODO: What to return...
            }

            throw new Exception("Unable to add participant " + dto.Number);
        }
    }
}