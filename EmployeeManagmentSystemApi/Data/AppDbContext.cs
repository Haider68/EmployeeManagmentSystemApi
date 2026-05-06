using EmployeeManagmentSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagmentSystemApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectAttachment> ProjectAttachments { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<SalaryRecord> SalaryRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Employee configurations
            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.Email)
                .IsUnique();

            modelBuilder.Entity<Employee>()
                .Property(e => e.Salary)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Employee>()
                .Property(e => e.HourlyRate)
                .HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<Employee>()
                .Property(e => e.OvertimeRate)
                .HasColumnType("decimal(18,2)");

            // Project configurations
            modelBuilder.Entity<Project>()
                .HasOne(p => p.CreatedBy)
                .WithMany(e => e.CreatedProjects)
                .HasForeignKey(p => p.CreatedById)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.LastUpdatedBy)
                .WithMany(e => e.UpdatedProjects)
                .HasForeignKey(p => p.LastUpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Project>()
                .HasMany(p => p.TeamMembers)
                .WithMany(e => e.Projects)
                .UsingEntity(j => j.ToTable("ProjectTeamMembers"));

            modelBuilder.Entity<Project>()
                .Property(p => p.Budget)
                .HasColumnType("decimal(18,2)");

            // LeaveRequest configurations
            modelBuilder.Entity<LeaveRequest>()
                .HasOne(lr => lr.Employee)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(lr => lr.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(lr => lr.ApprovedBy)
                .WithMany(e => e.ApprovedLeaveRequests)
                .HasForeignKey(lr => lr.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Attendance configurations
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.Attendances)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // SalaryRecord configurations
            modelBuilder.Entity<SalaryRecord>()
                .HasOne(sr => sr.Employee)
                .WithMany(e => e.SalaryRecords)
                .HasForeignKey(sr => sr.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SalaryRecord>()
                .Property(sr => sr.BaseSalary)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SalaryRecord>()
                .Property(sr => sr.TotalWorkedHours)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SalaryRecord>()
                .Property(sr => sr.RequiredHours)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SalaryRecord>()
                .Property(sr => sr.ShortageHours)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SalaryRecord>()
                .Property(sr => sr.DeductionAmount)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SalaryRecord>()
                .Property(sr => sr.OvertimeHours)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SalaryRecord>()
                .Property(sr => sr.OvertimePay)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SalaryRecord>()
                .Property(sr => sr.BonusesAmount)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SalaryRecord>()
                .Property(sr => sr.NetSalary)
                .HasColumnType("decimal(18,2)");
        }
    }
}
