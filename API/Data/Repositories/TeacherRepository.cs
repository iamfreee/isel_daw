﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models;

namespace API.Data.Repositories
{
    public class TeacherRepository : IRepository<Teacher>
    {
        private readonly DatabaseContext _context;

        public TeacherRepository(DatabaseContext context)
        {
            _context = context;
        }

        public Task<IEnumerable<Teacher>> GetAll(Func<Teacher, bool> pred = default(Func<Teacher, bool>))
        {
            return Task.Factory.StartNew<IEnumerable<Teacher>>(() => {
                if (pred == null)
                    pred = u => { return true; };
                    
                return _context.Teachers.Where(pred).ToList();
            });
        }

        public async Task<Teacher> Find(int Id) {
            return await _context.Teachers.FindAsync(Id);
        }

        public async Task<bool> Add(Teacher item) {
            _context.Teachers.Add(item);       //No access to Database
            if(await _context.SaveChangesAsync() > 0){
                return true;
            }
            return false;
        }

        public async Task<bool> Remove(int Id) {
            var teacher = await Find(Id);
            if(teacher == null){
                return false;
            }
            _context.Teachers.Remove(teacher);   //No access to Database
            if(await _context.SaveChangesAsync() > 0){
                return true;
            }
            return false;
        }

        public async Task<bool> Update(Teacher item) {
            _context.Teachers.Update(item);     //No access to Database
            if(await _context.SaveChangesAsync() > 0){
                return true;
            }
            return false;
        }
    }
}
