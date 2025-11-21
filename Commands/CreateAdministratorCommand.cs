using System;
using cafeInformationSystem.Models.Entities;
using cafeInformationSystem.Models.Cryptography;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using cafeInformationSystem.Models.DataBase;

namespace cafeInformationSystem.Commands;

public class CreateAdministratorCommand : ICommand
{
    public void Execute()
    {
        Console.WriteLine("Create new administrator:");

        var employee = new Employee
        {
            FirstName = UtilsCommands.GetInput("First Name", true),
            LastName = UtilsCommands.GetInput("Last Name", true),
            MiddleName = UtilsCommands.GetInput("Middle Name", true),
            Photo = null,  // INFO! не требуются для администратора
            ScanEmploymentContract = null,  // INFO! не требуются для администратора
            Username = UtilsCommands.GetInput("Username", true),
            Password = UtilsCommands.GetPasswordWithConfirmation(),
            Role = EmployeeRole.Administrator,
            WorkStatus = true
        };
        ValidateEmployee(employee);
        CreateEmployeeInDatabase(employee);
    }

    private void ValidateEmployee(Employee employee)
    {
        var validationContext = new ValidationContext(employee);
        var validationResults = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(employee, validationContext, validationResults, true);

        if (!isValid)
        {
            var errors = string.Join("\n", validationResults.Select(r => $" - {r.ErrorMessage}"));
            throw new ValidationException($"Employee validation failed: {errors}");
        }

        var context = DatabaseService.GetContext();
        var existing = context.Employee.FirstOrDefault(e => e.Username == employee.Username);
        if (existing != null)
        {
            throw new ValidationException($"Username '{employee.Username}' already exists");
        }
    }

    private void CreateEmployeeInDatabase(Employee employee)
    {
        var context = DatabaseService.GetContext();

        employee.Password = PasswordHashing.HashPassword(employee.Password);

        context.Employee.Add(employee);
        context.SaveChanges();

        Console.WriteLine("Admin info:");
        Console.WriteLine($"Full Name: {employee.FullName}");
        Console.WriteLine($"Username: {employee.Username}");
    }
}
