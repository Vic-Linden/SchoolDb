using MyDatabase.Data;
using MyDatabase.Models;
using Spectre.Console;

namespace MyDatabase

{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var context = new SchoolDbContext();
            var menu = new Menu(context);
            menu.Start();
        }
    }
}
