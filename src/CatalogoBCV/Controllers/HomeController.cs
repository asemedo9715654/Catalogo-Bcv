using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CatalogoBCV.Models;
using CatalogoBCV.Data;
using CatalogoBCV.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CatalogoBCV.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly CatalogContext _context;

    public HomeController(CatalogContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalDatabases = await _context.CatalogDatabases.CountAsync();
        var totalTables = await _context.Tables.CountAsync();
        var totalColumns = await _context.Columns.CountAsync();
        var totalUsers = await _context.Users.CountAsync();

        var documentedColumns = await _context.Columns.CountAsync(c => !string.IsNullOrEmpty(c.Description));
        var documentationPercentage = totalColumns > 0 ? (double)documentedColumns / totalColumns * 100 : 0;

        var factTablesCount = await _context.Tables.CountAsync(t => t.IsFactTable);
        var dimensionTablesCount = totalTables - factTablesCount;

        var topDomains = await _context.Domains
            .Select(d => new DomainStat
            {
                Name = d.Name,
                TableCount = d.Tables.Count
            })
            .OrderByDescending(d => d.TableCount)
            .Take(5)
            .ToListAsync();

        var largestTables = await _context.Tables
            .Include(t => t.CatalogDatabase)
            .OrderByDescending(t => t.RowCount)
            .Take(5)
            .Select(t => new TableStat
            {
                DatabaseName = t.CatalogDatabase.DatabaseName,
                TableName = t.Name,
                RowCount = t.RowCount ?? 0
            })
            .ToListAsync();

        var databasesStatus = await _context.CatalogDatabases
            .OrderByDescending(d => d.LastUpdated)
            .ToListAsync();

        var recentActivities = await _context.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(5)
            .ToListAsync();

        var viewModel = new DashboardViewModel
        {
            TotalDatabases = totalDatabases,
            TotalTables = totalTables,
            TotalColumns = totalColumns,
            TotalUsers = totalUsers,
            DocumentationPercentage = Math.Round(documentationPercentage, 1),
            FactTablesCount = factTablesCount,
            DimensionTablesCount = dimensionTablesCount,
            TopDomains = topDomains,
            LargestTables = largestTables,
            DatabasesStatus = databasesStatus,
            RecentActivities = recentActivities
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
