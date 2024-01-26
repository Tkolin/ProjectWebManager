using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
 
namespace Test.Models;

public partial class ProjectDbContext : IdentityDbContext<Employee, IdentityRole<int>, int,
    IdentityUserClaim<int>, IdentityUserRole<int>, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
{
    public ProjectDbContext()
    {
    }

    public ProjectDbContext(DbContextOptions<ProjectDbContext> options)
        : base(options)
    {
    }
 
    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeToProject> EmployeeToProjects { get; set; }

    public virtual DbSet<Project> Projects { get; set; }
    public virtual DbSet<StatusTask> StatusTask { get; set; }
    public virtual DbSet<Task> Task { get; set; }

    public virtual DbSet<DocumentsToProject> DocumentsToProject { get; set; }
    public virtual DbSet<Document> Document { get; set; }
    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Server=GRIGORIY\\SQLEXPRESS;Database=ProjectDB;Trusted_Connection=True;Trust Server Certificate=True;Integrated security=true;");
    // Добавьте этот метод для инициализации ролей

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Company");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employee");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Patronymic)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<EmployeeToProject>(entity =>
        {
            entity.ToTable("EmployeeToProject");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.ProjectId).HasColumnName("ProjectID");

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeToProjects)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeToProject_Employee");

            entity.HasOne(d => d.Project).WithMany(p => p.EmployeeToProjects)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeToProject_Project");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Project");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CompaniesCutomerId).HasColumnName("CompaniesCutomerID");
            entity.Property(e => e.DateEnd).HasColumnType("date");
            entity.Property(e => e.DateStart).HasColumnType("date");
            entity.Property(e => e.ImplementingCutomerId).HasColumnName("ImplementingCutomerID");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ProjectDirectorId).HasColumnName("ProjectDirectorID");

            entity.HasOne(d => d.CompaniesCutomer).WithMany(p => p.ProjectCompaniesCutomers)
                .HasForeignKey(d => d.CompaniesCutomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Project_Company");

            entity.HasOne(d => d.ImplementingCutomer).WithMany(p => p.ProjectImplementingCutomers)
                .HasForeignKey(d => d.ImplementingCutomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Project_Company1");

            entity.HasOne(d => d.ProjectDirector).WithMany(p => p.Projects)
                .HasForeignKey(d => d.ProjectDirectorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Project_Employee");

        });
        modelBuilder.Entity<Task>(entity =>
        {
            entity.ToTable("Task");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ProjectId).HasColumnName("ProjectId");
            entity.Property(e => e.AuthorId).HasColumnName("AuthorId");
            entity.Property(e => e.ExecutorId).HasColumnName("ExecutorId");
            entity.Property(e => e.StatusId).HasColumnName("ImplementingCutomerID");
            entity.Property(e => e.Priority);
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectTask)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Task_Project");
            entity.HasOne(d => d.Author).WithMany(p => p.TasksAuthor)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Task_Employee");
            entity.HasOne(d => d.Executor).WithMany(p => p.TasksExecutor)
                .HasForeignKey(d => d.ExecutorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Task_Employee1");
            entity.HasOne(d => d.StatusTask).WithMany(p => p.Status)
               .HasForeignKey(d => d.StatusId)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("FK_Task_StatusTask");
        });
        // Стутус

        modelBuilder.Entity<StatusTask>(entity =>
        {
            entity.ToTable("StatusTask");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
        });
        // Документы

        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Document");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(255).IsUnicode(false);
            entity.Property(e => e.Data).HasColumnName("Data");



        });

        modelBuilder.Entity<DocumentsToProject>(entity =>
        {
            entity.HasKey(r => new { r.ProjectId, r.DocumentId });

            entity.HasOne(d => d.Document).WithMany(p => p.DocumentsToProjects)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentToProject_Document");

            entity.HasOne(d => d.Project).WithMany(p => p.DocumentsToProjects)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentToProject_Project");

        }
        );
        base.OnModelCreating(modelBuilder);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
