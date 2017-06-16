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
    public class ClassRepository : GenericRepository<DatabaseContext, Class>, IClassRepository
    {
        public ClassRepository(DatabaseContext ctx) : base(ctx)
        {
        }

        public Task<Class> GetByIdAsync(int Id)
        {
            return Context.Classes
                .Where(c => c.Id == Id)
                .Include(c => c.Semester)
                .Include(c => c.Course)
                .Include(c => c.Participants)
                .Include(c => c.Groups)
                .FirstOrDefaultAsync();
        }

        public Task<PagedList<Class>> GetAllPaginatedAsync(ListQueryStringDto p)
        {
            IQueryable<Class> classes = Context.Classes
                .Include(c => c.Semester)
                .OrderBy(s => s.Id);

            if (!string.IsNullOrEmpty(p.Search))
            {
                var query = p.Search.Trim();

                int yearToSearch = -1;

                try
                {
                    yearToSearch = Int32.Parse(query);
                }
                catch (FormatException)
                {
                    // we dont want to search by number then..
                }

                classes = classes.Where(
                    s =>
                        s.Semester.Year == yearToSearch ||  //TODO: maybe add string 1617V?
                        s.Name.Contains(query)
                );
            }

            return PagedList<Class>.Create(classes, p.Page, p.Limit);
        }

        public Task<PagedList<Group>> GetClassGroups(int id, ListQueryStringDto p)
        {
            IQueryable<Group> classGroups = Context.Groups
                .Where(g => g.ClassId == id);

            return PagedList<Group>.Create(classGroups, p.Page, p.Limit);
        }

        public Task<bool> AddParticipantTo(Class c, int studentNumberId)
        {
            Context.Add<ClassStudent>(new ClassStudent{
                ClassId = c.Id,
                StudentNumberId = studentNumberId
            });
            
            // c.Participants.Add(new ClassStudent{
            //     Class = c,
            //     StudentNumberId = studentNumberId
            // });

            return SaveAsync();
        }

        public Task<bool> AddGroupTo(Class c)
        {
            c.Groups.Add(new Group{
                ClassId = c.Id
            });

            return SaveAsync();
        }

        public Task<List<Student>> GetClassParticipants(int id)
        {
            throw new NotImplementedException();
        }
    }
}