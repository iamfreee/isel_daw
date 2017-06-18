using System;
using System.Threading.Tasks;
using API.Data.Contracts;
using API.Models;
using API.Services;
using API.TransferModels.InputModels;
using API.TransferModels.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class TeachersController : Controller
    {
        private ITeacherRepository _repo;

        private readonly TeachersSirenHto _teachersRep;
        private readonly ClassesSirenHto _classesRep;

        public TeachersController(
            ITeacherRepository repo,
            TeachersSirenHto teachersRepresentation,
            ClassesSirenHto classesRepresentation)
        {
            _repo = repo;
            _teachersRep = teachersRepresentation;
            _classesRep = classesRepresentation;
        }

        [HttpGet("", Name=Routes.TeacherList)]
        public async Task<IActionResult> GetAll([FromQuery]ListQueryStringDto query)
        {
            var teachers = await _repo.GetAllPaginatedAsync(query);

            var result = _teachersRep.Collection(teachers, query);

            return Ok(result);
        }

        [HttpGet("{number}", Name=Routes.TeacherEntry)]
        public async Task<IActionResult> Get(int Number)
        {
            var teacher = await _repo.GetByNumberAsync(Number);

            if(teacher == null){
                return NotFound();
            }

            return Ok(_teachersRep.Entity(teacher));
        }

        [HttpPost("", Name=Routes.TeacherCreate)]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Post([FromBody]TeacherDTO dto)
        {
            if(!ModelState.IsValid){
                return BadRequest(ModelState);
            }

            //TODO: AutoMapper
            Teacher teacher = new Teacher{
                Number = dto.Number,
                Name = dto.Name,
                Email = dto.Email,
                Password = dto.Password,
                IsAdmin = dto.IsAdmin
            };

            if(!await _repo.AddAsync(teacher)){
                throw new Exception("Unable to add teacher");
            }

            return CreatedAtRoute(
                Routes.TeacherEntry,
                new {number = teacher.Number},
                _teachersRep.Entity(teacher)
            );
        }

        [HttpPut("{number}", Name=Routes.TeacherEdit)]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Put(int Number, [FromBody]TeacherDTO dto)
        {
            if(!ModelState.IsValid){
                return BadRequest(ModelState);
            }

            Teacher teacher = await _repo.GetByNumberAsync(Number);
            if(teacher == null){
                return NotFound();
            }

            //TODO: AutoMapper
            teacher.Number = dto.Number;
            teacher.Name = dto.Name;
            teacher.Email = dto.Email;
            teacher.Password = dto.Password;
            teacher.IsAdmin = dto.IsAdmin;

            if(!await _repo.EditAsync(teacher)){
                throw new Exception("Unable to edit teacher " + Number);
            }
            
            return Ok(_teachersRep.Entity(teacher));
            //return NoContent();
        }

        [HttpDelete("{number}", Name=Routes.TeacherDelete)]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(int Number)
        {
            Teacher teacher = await _repo.GetByNumberAsync(Number);

            if(teacher == null){
                return NotFound();
            }

            if(await _repo.DeleteAsync(teacher))
            {
                return NoContent();
            }
            
            throw new Exception("Unable to delete teacher " + Number);
        }

        [HttpGet("{number}/classes", Name = Routes.TeacherClassList)]
        public async Task<IActionResult> TeacherClasses(
            int number,
            [FromQuery] ListQueryStringDto query)
        {
            Teacher teacher = await _repo.GetByNumberAsync(number);

            if(teacher == null){
                 return NotFound();
            }

            PagedList<Class> classes =
                await _repo.GetPaginatedTeacherClassesAsync(number, query);

            return Ok(classes);
            //return Ok(_classesRep.Collection(classes, query));
        }
    }
}