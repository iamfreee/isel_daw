using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class Class
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int MaxGroupSize { get; set; }

        public int SemesterId { get; set; }
        public Semester Semester { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public bool AutoEnrollment { get; set; }

        public ICollection<ClassTeacher> Teachers { get; set; }

        public ICollection<ClassStudent> Participants { get; set; }

        public List<Group> Groups { get; set; }
    }

    /*
    |--------------------------------------------------------------------------
    | Join Tables - Many to Many (EF Core needs this at the moment)
    |--------------------------------------------------------------------------
    */
    public class ClassTeacher {

        public int ClassId { get; set; }
        [ForeignKey("ClassId")]
        public Class Class { get; set; }

        public int TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public Teacher Teacher { get; set; }
    }

    public class ClassStudent
    {

        public int ClassId { get; set; }
        [ForeignKey("ClassId")]
        public Class Class { get; set; }

        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student Student { get; set; }
    }
}