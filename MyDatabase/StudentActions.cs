using System;
using System.Collections.Generic;
using System.Text;
using MyDatabase.Data;
using MyDatabase.Models;
using Spectre.Console;
using Microsoft.EntityFrameworkCore;

namespace MyDatabase
{
    internal class StudentActions
    {
        private readonly SchoolDbContext _context;

        public StudentActions(SchoolDbContext context)
        {
            _context = context;
        }

        public void ViewAllStudents()
        {
            var sortField = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title("Sort students by:")
                    .AddChoices("First name", "Last name"));

            var sortOrder = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title("Sort order:")
                    .AddChoices("Ascending", "Descending"));

            IQueryable<Student> query = _context.Students;

            switch (sortField)
            {
                case "First name":
                    if (sortOrder == "Ascending")
                        query = query.OrderBy(s => s.FirstName ?? "");
                    else
                        query = query.OrderByDescending(s => s.FirstName ?? "");
                    break;

                case "Last name":
                    if (sortOrder == "Ascending")
                        query = query.OrderBy(s => s.LastName ?? "");
                    else
                        query = query.OrderByDescending(s => s.LastName ?? "");
                    break;

                default:
                    query = query.OrderBy(s => s.LastName ?? "");
                    break;
            }
            ;
            // jag testade även att göra en switch-expression.
            //query = (sortField, sortOrder) switch
            //{
            //    ("First name", "Ascending") => query.OrderBy(s => s.FirstName ?? ""),
            //    ("First name", "Descending") => query.OrderByDescending(s => s.FirstName ?? ""),
            //    ("Last name", "Ascending") => query.OrderBy(s => s.LastName ?? ""),
            //    _ => query.OrderByDescending(s => s.LastName ?? "")
            //};

            var students = query.ToList();

            var table = new Spectre.Console.Table()
                .AddColumn("ID")
                .AddColumn("First name")
                .AddColumn("Last name");

            students.ForEach(s => table.AddRow(s.StudentId.ToString(),
                s.FirstName ?? "",
                s.LastName ?? ""
                ));

            AnsiConsole.Write(table);
        }

        public void ViewStudentsFromClass()
        {
            var classes = _context.Classes.OrderBy(c => c.ClassName).ToList();

            var chosenClass = AnsiConsole.Prompt(new SelectionPrompt<Class>()
                .Title("Choose a class:")
                .UseConverter(c => c.ClassName)
                .AddChoices(classes));

            // EXTRA UTMANING- user can choose FirstName or LastName
            var sortField = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Sort students by:")
                .AddChoices("First name", "Last name"));

            var sortOrder = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Sort order:")
                .AddChoices("Ascending", "Descending"));

            // Start query. students from class
            IQueryable<Student> query = _context.Students.Where(s => s.ClassId == chosenClass.ClassId);

            switch (sortField)
            {
                case "First name":
                    if (sortOrder == "Ascending")
                        query = query.OrderBy(s => s.FirstName ?? "");
                    else
                        query = query.OrderByDescending(s => s.FirstName ?? "");
                    break;
                case "Last name":
                    if (sortOrder == "Descending")
                        query = query.OrderBy(s => s.LastName ?? "");
                    else
                        query = query.OrderByDescending(s => s.LastName ?? "");
                    break;
                default:
                    query = query.OrderBy(s => s.LastName ?? "");
                    break;
            }

            // start SQL.
            var students = query.ToList();

            AnsiConsole.MarkupLine($"\n[bold]Class:[/] {chosenClass.ClassName}");

            var table = new Spectre.Console.Table()
                .AddColumn("ID")
                .AddColumn("First name")
                .AddColumn("Last name");

            students.ForEach(s => table.AddRow(
                s.StudentId.ToString(),
                s.FirstName ?? "",
                s.LastName ?? ""
            ));

            AnsiConsole.Write(table);
        }

        public void AddNewStudent()
        {
            var firstName = AnsiConsole.Ask<string>("First name:");
            var lastName = AnsiConsole.Ask<string>("Last name:");
            var personNumber = AnsiConsole.Ask<string>("Personal security number:");

            var classes = _context.Classes.OrderBy(c => c.ClassName).ToList();

            var chosenClass = AnsiConsole.Prompt(new SelectionPrompt<Class>()
                .Title("Choose class:")
                .UseConverter(c => c.ClassName)
                .AddChoices(classes));

            // creates student-object
            var student = new Student
            {
                FirstName = firstName,
                LastName = lastName,
                Personnumber = personNumber,
                ClassId = chosenClass.ClassId
            };

            _context.Students.Add(student);
            _context.SaveChanges();

            AnsiConsole.MarkupLine("[green]Student saved successfully![/]");
        }

        public void DeleteStudent()
        {
            // View student
            var students = _context.Students
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToList();

            if (!students.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No students found.[/]");
                return;
            }

            // User can choose wich student to delete
            var chosenStudent = AnsiConsole.Prompt(new SelectionPrompt<Student>()
                .Title("Choose a student to delete:")
                .PageSize(10)
                .UseConverter(s => $"{s.StudentId} - {s.FirstName ?? ""} {s.LastName ?? ""} ({s.Personnumber})")
                .AddChoices(students));

            var confirmChoice = AnsiConsole.Confirm(
                $"Are you sure you want to delete: ID:{chosenStudent.StudentId} - {chosenStudent.FirstName} {chosenStudent.LastName}?");

            if (!confirmChoice)
            {
                AnsiConsole.MarkupLine("[grey]Delete cancelled.[/]");
                return;
            }

            // I use try/catch to handle errors when deleting data from the database.
            // If it fails the database relationships, an DbUpdateException is caught and a message is shown instead of program crashing.
            try
            {
                _context.Students.Remove(chosenStudent);
                _context.SaveChanges();
                AnsiConsole.MarkupLine("[green]Student deleted successfully![/]");
            }
            catch (DbUpdateException)
            {
                AnsiConsole.MarkupLine(
                    "[red]Could not delete student.[/]\n" +
                    "[yellow]Reason:[/] The student is likely linked to other data (exemple Grades).\n" +
                    "Delete or update the related records first, or change FK rules in the database.");
            }
        }
    }
}
