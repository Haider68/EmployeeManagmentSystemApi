using EmployeeManagmentSystemApi.Data;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace EmployeeManagmentSystemApi.Jobs;

public class SubmitPersonTerminationJob
{
    private readonly AppDbContext _dbContext;
    //private readonly IEmployeeCredentialService _credentialService;
    private readonly ILogger<SubmitPersonTerminationJob> _logger;

    public SubmitPersonTerminationJob(
        AppDbContext dbContext,
        /*IEmployeeCredentialService credentialService*/
        ILogger<SubmitPersonTerminationJob> logger)
    {
        _dbContext = dbContext;
        //_credentialService = credentialService;
        _logger = logger;
    }

    [DisableConcurrentExecution(timeoutInSeconds: 600)]
    public async Task ExecuteAsync()
    {
        var now = DateTime.UtcNow;

        var employees = await _dbContext.Employees
            .Where(e => e.ValidUntil != null && e.ValidUntil < now && 
                        e.Status != "inactive")
            .ToListAsync();

        foreach (var employee in employees)
        {
            try
            {
                // Disable the employee account.
                employee.Status = "inactive";
                employee.UpdatedAt = now;

                // Revoke credentials/sessions for this employee.
                //await _credentialService.RevokeAllAsync(employee.Id);

                _logger.LogInformation(
                    "Employee {EmployeeId} was terminated.",
                    employee.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to terminate employee {EmployeeId}.",
                    employee.Id);

                throw;
            }
        }

        await _dbContext.SaveChangesAsync();

        //_logger.LogInformation(
        //    "SubmitPersonTermination completed at {Time}. " +
        //    "Employees processed: {Count}",
        //    now,
        //    employees.Count);
    }
}

