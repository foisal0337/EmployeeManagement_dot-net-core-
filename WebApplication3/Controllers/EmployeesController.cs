using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using webApplication3.Models;


namespace webApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        // API01: Update an employee’s Employee Code 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployeeCode(int id, Employee employee)
        {
            // Check if the employee with the given ID exists
            var existingEmployee = await _context.Employees.FindAsync(id);
            if (existingEmployee == null)
            {
                return NotFound();
            }

            // Check if the new employee code already exists
            var duplicateEmployeeCode = await _context.Employees.AnyAsync(e => e.EmployeeCode == employee.EmployeeCode && e.EmployeeId != id);
            if (duplicateEmployeeCode)
            {
                return Conflict("Employee code already exists.");
            }

            // Update the employee code
            existingEmployee.EmployeeCode = employee.EmployeeCode;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "An error occurred while updating the employee code.");
            }

            return NoContent();
        }

        // API02: Get all employees based on maximum to minimum salary
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployeesBySalary()
        {
            var employees = await _context.Employees.OrderByDescending(e => e.EmployeeSalary).ToListAsync();
            return employees;
        }

        // API03: Find all employees who are absent at least one day
        [HttpGet("absent")]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployeesWithAbsence()
        {
            var employees = await _context.Employees
                .Join(_context.EmployeeAttendance, e => e.EmployeeId, a => a.EmployeeId, (e, a) => new { Employee = e, Attendance = a })
                .Where(x => x.Attendance.IsAbsent > 0)
                .Select(x => x.Employee)
                .ToListAsync();

            return employees;
        }

        // API04: Get monthly attendance report of all employees
        [HttpGet("monthly-attendance")]
        public async Task<ActionResult<IEnumerable<MonthlyAttendanceReport>>> GetMonthlyAttendanceReport()
        {
            var reports = await _context.Employees
                .Join(
                    _context.EmployeeAttendance,
                    e => e.EmployeeId,
                    ea => ea.EmployeeId,
                    (e, ea) => new { Employee = e, Attendance = ea }
                )
                .GroupBy(
                    x => new { x.Employee.EmployeeName, Month = x.Attendance.AttendanceDate.Month },
                    x => new { x.Attendance.IsPresent, x.Attendance.IsAbsent, x.Attendance.IsOffday }
                )
                .Select(g => new MonthlyAttendanceReport
                {
                    EmployeeName = g.Key.EmployeeName,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                    TotalPresent = g.Sum(x => x.IsPresent),
                    TotalAbsent = g.Sum(x => x.IsAbsent),
                    TotalOffday = g.Sum(x => x.IsOffday)
                })
                .ToListAsync();

            return Ok(reports);
        }

    }
}
