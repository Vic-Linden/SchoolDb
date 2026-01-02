using System;
using System.Collections.Generic;
using System.Text;
using MyDatabase.Data;
using MyDatabase.Models;
using Spectre.Console;
using Microsoft.EntityFrameworkCore;

namespace MyDatabase
{
    internal class StaffActions
    {
        private readonly SchoolDbContext _context;

        public StaffActions(SchoolDbContext context)
        {
            _context = context;
        }

        public void ViewAllStaff()
        {
            var staff = _context.Staff.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToList();

            var table = new Spectre.Console.Table()
               .AddColumn("ID")
               .AddColumn("First name")
               .AddColumn("Last name")
               .AddColumn("Role");

            staff.ForEach(s => table.AddRow(s.PersonId.ToString(),
                    s.FirstName ?? "",
                    s.LastName ?? "",
                    s.Role));

            AnsiConsole.Write(table);
        }

        public void TeachersPerDepartment() 
        {
            // Shows all teachers, groups them by department, and counts how many teachers work in each department.
            var result = _context.Staff
                .Where(s => s.Role == "Teacher")
                .GroupBy(s => s.Department != null ? s.Department.DepartmentName : "No department")
                .Select(g => new
                {
                    DepartmentName = g.Key,
                    TeacherCount = g.Count()
                })
                .OrderBy(x => x.DepartmentName)
                .ToList();

            if (!result.Any()) 
            {
                AnsiConsole.MarkupLine("[yellow]No teachers found.[/]");
                return;
            }

            var table = new Table()
                .AddColumn("Department")
                .AddColumn("Teachers");

            result.ForEach(r =>
              table.AddRow(r.DepartmentName, r.TeacherCount.ToString()));

            AnsiConsole.Write(table);
        }
        public void AddNewStaff()
        {
            var firstName = AnsiConsole.Ask<string>("First name:");
            var lastName = AnsiConsole.Ask<string>("Last name:");
            var role = AnsiConsole.Ask<string>("Role:");

            var staff = new Staff
            {
                FirstName = firstName,
                LastName = lastName,
                Role = role
            };

            _context.Staff.Add(staff);
            _context.SaveChanges();

            AnsiConsole.MarkupLine("[green]Staff member saved successfully![/]");
        }

        public void DeleteStaff()
        {
            var staffList = _context.Staff
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToList();

            if (!staffList.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No staff found.[/]");
                return;
            }

            var chosenStaff = AnsiConsole.Prompt(new SelectionPrompt<Staff>()
            .Title("Choose a staff member to delete:")
            .PageSize(10)
            .UseConverter(s => $"{s.PersonId} - {s.FirstName ?? ""} {s.LastName ?? ""} ({s.Role})")
            .AddChoices(staffList));

            var confirmChoice = AnsiConsole.Confirm(
                $"Are you sure you want to delete: ID:{chosenStaff.PersonId} - {chosenStaff.FirstName} {chosenStaff.LastName}?");

            if (!confirmChoice)
            {
                AnsiConsole.MarkupLine("[grey]Delete cancelled.[/]");
                return;
            }

            try
            {
                _context.Staff.Remove(chosenStaff);
                _context.SaveChanges();
                AnsiConsole.MarkupLine("[green]Staff deleted successfully![/]");
            }
            catch (DbUpdateException)
            {
                AnsiConsole.MarkupLine(
                    "[red]Could not delete student.[/]\n" +
                    "[yellow]Reason:[/] The student is likely linked to other data.\n" +
                    "Delete or update the related records first, or change FK rules in the database.");
            }
        }

        public void ViewActiveCourses() 
        {
            var courses = _context.Courses
                .Where(c => c.IsActive)
                .OrderBy(c => c.CourseName)
                .ToList();

            if (!courses.Any()) 
            {
                AnsiConsole.Markup("[yellow]No active courses found.[/]");
                return;
            }

            var table = new Table()
                .AddColumn("Course ID")
                .AddColumn("Course name");

            foreach (var c in courses) 
            {
                table.AddRow(
                    c.CourseId.ToString(),
                    c.CourseName
                    );
            }
            AnsiConsole.Write(table);
        }
        public void SetGradeWithTransaction() 
        {
            //choose a student.
            var student = _context.Students
               .OrderBy(s => s.LastName)
               .ThenBy(s => s.FirstName)
               .ToList();

            if (!student.Any()) 
            {
                AnsiConsole.Markup("[yellow]No student found.[/]");
                return;
            }

            var chosenStudent = AnsiConsole.Prompt(new SelectionPrompt<Student>()
                .Title("Choose a student:")
                .UseConverter(s => $"{s.StudentId} - {s.FirstName ?? ""} - {s.LastName ?? ""}".Trim())
                .AddChoices(student)
                );

            //choose active course.
            var course = _context.Courses
                .Where(c => c.IsActive)
                .OrderBy(c => c.CourseName)
                .ToList();

            if (!course.Any()) 
            {
                AnsiConsole.Markup("[yellow]No active course found.[/]");
                return;
            }
            var chosenCourse = AnsiConsole.Prompt(new SelectionPrompt<Course>()
                .Title("Choose an active course:")
                .UseConverter(c => $"{c.CourseId} - {c.CourseName}")
                .AddChoices(course)
                );

            //Choose a teacher.
            var teacher = _context.Staff
                .Where(s => s.Role == "Teacher")
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToList();

            if (!teacher.Any()) 
            {
                AnsiConsole.Markup("[yellow]No teacher found[/]");
                return;
            }

            var chosenTeacher = AnsiConsole.Prompt(new SelectionPrompt<Staff>()
                .Title("Choose a teacher who sets the grade:")
                .UseConverter(s => $"{s.PersonId} - {s.FirstName ?? ""} - {s.LastName ?? ""}".Trim())
                .AddChoices(teacher)
                );

            //Choose grade.
            var chosenGrade = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("choose grade:")
                .AddChoices("A", "B", "C", "D", "E", "F")
                );

            //Transaction
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var grade = new Grade
                {
                    StudentId = chosenStudent.StudentId,
                    CourseId = chosenCourse.CourseId,
                    TeacherId = chosenTeacher.PersonId,
                    Grade1 = chosenGrade,
                    GradeDate = DateOnly.FromDateTime(DateTime.Today)
                };

                _context.Grades.Add(grade);
                _context.SaveChanges();

                transaction.Commit();
                AnsiConsole.MarkupLine("[green]Grade saved successfully![/]");
            }
            catch (DbUpdateException ex)
            {
                transaction.Rollback();
                AnsiConsole.MarkupLine("[red]Could not save. No updates were saved.[/]");
                AnsiConsole.MarkupLine($"[grey]{ex.Message}[/]");
            }
            catch (Exception ex) 
            {
                transaction.Rollback();
                AnsiConsole.MarkupLine("[red]Error. No updates were saved.[/]");
                AnsiConsole.MarkupLine($"[grey]{ex.Message}[/]");
            }
        }
    }
}
