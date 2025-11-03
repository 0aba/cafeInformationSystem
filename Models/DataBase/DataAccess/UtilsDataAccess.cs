using System;
using System.Linq;
using cafeInformationSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace cafeInformationSystem.Models.DataBase.DataAccess;

public static class UtilsDataAccess
{
    public static Employee GetEmployee(string username)
    {
        var context = DatabaseService.GetContext();

        var employee = context.Employee.AsNoTracking().FirstOrDefault(e => e.Username == username);

        return employee ?? throw new Exception($"Employee with username '{username}' not found");
    }

    public static bool CheckExistsEmployee(string username)
    {
        var context = DatabaseService.GetContext();

        var employee = context.Employee.AsNoTracking().FirstOrDefault(e => e.Username == username);

        return employee is not null;
    }
}
