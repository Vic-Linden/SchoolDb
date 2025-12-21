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
    }
}
