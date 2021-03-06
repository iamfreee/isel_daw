using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data.Contracts;
using API.Models;
using API.Services;
using API.TransferModels.InputModels;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class TeacherRepository : GenericRepository<DatabaseContext, Teacher>, ITeacherRepository
    {
        public TeacherRepository(DatabaseContext ctx) : base(ctx)
        {
        }

        public override Task<List<Teacher>> GetAllAsync()
        {
            return Context.Teachers
                .Include(s => s.Classes)
                .ToListAsync();
        }

        public Task<PagedList<Teacher>> GetAllPaginatedAsync(ListQueryStringDto p)
        {
            IQueryable<Teacher> teachers = Context.Teachers.OrderBy(s => s.Number);

            if (!string.IsNullOrEmpty(p.Search))
            {
                var query = p.Search.Trim();

                int numberToSearch = -1;

                try
                {
                    numberToSearch = Int32.Parse(query);
                }
                catch (FormatException)
                {
                    // we dont want to search by number then..
                }

                teachers = teachers.Where(
                    s =>
                        s.Email.Contains(query) ||
                        s.Name.Contains(query)  ||
                        s.Number == numberToSearch
                );
            }

            return PagedList<Teacher>.Create(teachers, p.Page, p.Limit);
        }

        public Task<Teacher> GetByEmailAndPasswordAsync(string email, string password)
        {
            return Context.Teachers
                .Where(s => s.Email == email && s.Password == password)
                .SingleOrDefaultAsync();
        }

        public Task<Teacher> GetByNumberAsync(int number)
        {
            return Context.Teachers
                .Where(c => c.Number == number)
                .Include(c => c.Classes)
                .SingleOrDefaultAsync();
        }

        public Task<PagedList<Class>> GetPaginatedTeacherClassesAsync(
            int number,
            ListQueryStringDto query)
        {
            IQueryable<Class> classes = Context.Classes
                .Where(cl => cl.Teachers
                    .Where(ct => ct.TeacherId == number)
                    .FirstOrDefault() != default(ClassTeacher)
                ).Include(cl => cl.Semester);

            return PagedList<Class>.Create(classes, query.Page, query.Limit);
        }

        public Task<PagedList<Course>> GetPaginatedTeacherCourses(int number, ListQueryStringDto query)
        {
            IQueryable<Course> courses = Context.Courses
                .Where(cl => cl.CoordinatorId == number);

            return PagedList<Course>.Create(courses, query.Page, query.Limit);
        }
    }
}
