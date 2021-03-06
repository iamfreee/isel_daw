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
    public class CoursesController : Controller
    {
        private ICourseRepository _repo;

        private readonly CoursesSirenHto _coursesRep;
        private readonly CourseClassesSirenHto _classesRep;

        public CoursesController(
            ICourseRepository repo,
            CoursesSirenHto coursesRepresentation,
            CourseClassesSirenHto classesRepresentation)
        {
            _repo = repo;
            _coursesRep = coursesRepresentation;
            _classesRep = classesRepresentation;
        }

        [HttpGet("", Name = Routes.CourseList)]
        public async Task<IActionResult> GetAll([FromQuery] ListQueryStringDto query)
        {
            PagedList<Course> courses = await _repo.GetAllPaginatedAsync(query);

            return Ok(_coursesRep.Collection(courses, query));
        }

        [HttpGet("{id}", Name = Routes.CourseEntry)]
        public async Task<IActionResult> Get(int Id)
        {
            Course course = await _repo.GetByIdAsync(Id);

            if (course == null)
            {
                return NotFound(new ProblemJson{
                    Type = "/clourse-not-found",
                    Status = 404,
                    Title = "Course Not Found",
                    Detail = "The course with the id "+Id+" does not exist or it wasn't found."
                });
            }

            return Ok(_coursesRep.Entity(course));
        }

        [HttpGet("{id}/classes", Name = Routes.CourseClassList)]
        public async Task<IActionResult> GetCourseClasses(int Id, [FromQuery] ListQueryStringDto query)
        {
            Course course = await _repo.GetByIdAsync(Id);

            if (course == null)
            {
                return NotFound(new ProblemJson{
                    Type = "/course-not-found",
                    Status = 404,
                    Title = "Course Not Found",
                    Detail = "The course with the id "+Id+" does not exist or it wasn't found."
                });
            }

            PagedList<Class> classes = await _repo.GetCourseClassesAsync(Id, query);

            return Ok(_classesRep.WeakCollection(Id, classes, query));
        }

        [HttpPost("", Name = Routes.CourseCreate)]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Post([FromBody]CourseDTO dto){
            if(!ModelState.IsValid){
                return BadRequest(ModelState);
            }

            Course c = new Course {
                Name = dto.Name,
                Acronym = dto.Acronym,
                CoordinatorId = dto.CoordinatorId
            };

            if(!await _repo.AddAsync(c)){
                throw new Exception("Unable to Add Course");
            }

            c = await _repo.GetByIdAsync(c.Id);

            return Created(Routes.CourseEntry, new { Id = c.Id });
        }

        [HttpPut("{id}", Name = Routes.CourseEdit)]
        [Authorize(Roles = Roles.Admin)]
        public IActionResult Put(int id){
            return StatusCode(501, "Not Implemented");
        }
        
        [HttpDelete("{id}", Name = Routes.CourseDelete)]
        [Authorize(Roles = Roles.Admin)]
        public IActionResult Delete(int id){
            return StatusCode(501, "Not Implemented");
        }

    }
}