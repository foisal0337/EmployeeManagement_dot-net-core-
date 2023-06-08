using System;
using System.ComponentModel.DataAnnotations;


namespace webApplication3.Models
{
    public class EmployeeAttendance
    {
        [Key]
        public int AttendanceId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public int IsPresent { get; set; }
        public int IsAbsent { get; set; }
        public int IsOffday { get; set; }
    }
}