using System;
using System.Collections.Generic;
using System.Text;
using MyDatabase.Data;
using MyDatabase.Models;
using Spectre.Console;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;

namespace MyDatabase
{
    internal class Menu
    {
        private readonly StudentActions _students;
        private readonly StaffActions _staff;

        public Menu(SchoolDbContext context)
        {
            _students = new StudentActions(context);
            _staff = new StaffActions(context);
        }

        public void Start()
        {
            AnsiConsole.MarkupLine("[bold yellow]Welcome to the [underline]School Database[/]![/]\n");
            
            AnsiConsole.WriteLine();

            while (true)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold gold1]What would you like to do?[/]")
                        .PageSize(12)
                        .HighlightStyle(new Style(Color.Gold1))
                        .AddChoices(
                            "View all students",
                            "View students from class",
                            "View student detail",
                            "Add new student",
                            "[red]Delete student[/]",
                            "View all staff",
                            "Teachers per department",
                            "Add new staff",
                            "[red]Delete staff[/]",
                            "View active courses",
                            "[green]Set grade[/]",
                            "Exit"
                        ));

                switch (choice)
                {
                    case "View all students":
                        _students.ViewAllStudents();
                        break;                      
                    case "View students from class":
                        _students.ViewStudentsFromClass();
                        break;
                    case "View student detail":
                        _students.ViewStudentDetail();
                        break;
                    case "Add new student":
                        _students.AddNewStudent();
                        break;
                    case "[red]Delete student[/]":
                        _students.DeleteStudent();
                        break;
                    case "View all staff":
                        _staff.ViewAllStaff();
                        break;
                    case "Teachers per department":
                        _staff.TeachersPerDepartment();
                        break;
                    case "Add new staff":
                        _staff.AddNewStaff();
                        break;
                    case "[red]Delete staff[/]":
                        _staff.DeleteStaff();
                        break;
                    case "View active courses":
                        _staff.ViewActiveCourses();
                        break;
                    case "[green]Set grade[/]":
                        _staff.SetGradeWithTransaction();
                        break;
                    case "Exit":
                        AnsiConsole.MarkupLine("[bold gold1]Thank you for visiting![/]");
                        return;
                }
                AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
                Console.ReadKey();
                Console.Clear();
            }
        }
    }  
}
