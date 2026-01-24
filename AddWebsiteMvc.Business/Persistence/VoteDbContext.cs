using AddWebsiteMvc.Business.Entities;
using AddWebsiteMvc.Business.Entities.Identity;
using AddWebsiteMvc.Business.Entities.SurveyEntity;
using AddWebsiteMvc.Business.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AddWebsiteMvc.Business.Persistence
{

    public class VoteDbContext : IdentityDbContext<
    ApplicationUser,
    ApplicationRole,
    Guid,
    IdentityUserClaim<Guid>,
    ApplicationUserRole,
    IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>,
    IdentityUserToken<Guid>>, IApplicationDbContext
    {
        public VoteDbContext(DbContextOptions<VoteDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
        public DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();
        public DbSet<ApplicationUserRole> ApplicationUserRoles => Set<ApplicationUserRole>();


        public DbSet<Ballot> Ballots { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<CandidateCategory> CandidateCategories { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Election> Elections { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<PaymentLog> PaymentLogs { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<Survey> Surveys { get; set; }
        public DbSet<SurveyAnswer> SurveyAnswers { get; set; }
        public DbSet<SurveyEmailTracking> SurveyEmailTrackings { get; set; }
        public DbSet<SurveyQuestion> SurveyQuestions { get; set; }
        public DbSet<SurveyResponse> SurveyResponses { get; set; }
        public DbSet<VotePrice> VotePrices { get; set; }
        public DbSet<Voter> Voters { get; set; }

        public DbSet<TransactionModel> TransactionModels { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users");
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.HasMany(u => u.UserRoles)
                    .WithOne(ur => ur.User)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            // Configure ApplicationRole
            builder.Entity<ApplicationRole>(entity =>
            {
                entity.ToTable("Roles");
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasMany(r => r.UserRoles)
                    .WithOne(ur => ur.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
            });

            // Configure ApplicationUserRole
            builder.Entity<ApplicationUserRole>(entity =>
            {
                entity.ToTable("UserRoles");
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
            });

            // Rename default Identity tables
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");


            // Survey Configuration
            builder.Entity<Survey>(entity =>
            {
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.IsActive);
            });

            // SurveyQuestion Configuration
            builder.Entity<SurveyQuestion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.QuestionText).IsRequired().HasMaxLength(500);
                entity.Property(e => e.QuestionType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IsRequired).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Survey)
                      .WithMany(s => s.Questions)
                      .HasForeignKey(e => e.SurveyId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.SurveyId);
            });

            // SurveyResponse Configuration
            builder.Entity<SurveyResponse>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserEmail).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ResponseToken).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.StartedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);

                entity.HasOne(e => e.Survey)
                      .WithMany(s => s.Responses)
                      .HasForeignKey(e => e.SurveyId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => e.SurveyId);
                entity.HasIndex(e => e.UserEmail);
                entity.HasIndex(e => e.ResponseToken);
                entity.HasIndex(e => e.CompletedAt);
            });

            // SurveyAnswer Configuration
            builder.Entity<SurveyAnswer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AnsweredAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Response)
                      .WithMany(r => r.Answers)
                      .HasForeignKey(e => e.ResponseId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Question)
                      .WithMany(q => q.Answers)
                      .HasForeignKey(e => e.QuestionId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => e.ResponseId);
                entity.HasIndex(e => e.QuestionId);
            });

            // SurveyEmailTracking Configuration
            builder.Entity<SurveyEmailTracking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserEmail).IsRequired().HasMaxLength(255);
                entity.Property(e => e.SentAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.EmailStatus).IsRequired().HasMaxLength(50).HasDefaultValue(EmailStatus.Sent);
                entity.Property(e => e.FailureReason).HasMaxLength(500);

                entity.HasOne(e => e.Response)
                      .WithOne(r => r.EmailTracking)
                      .HasForeignKey<SurveyEmailTracking>(e => e.ResponseId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => e.UserEmail);
                entity.HasIndex(e => e.EmailStatus);
            });

            // Seed default survey data (optional)
            //SeedDefaultSurvey(builder);

            // Seed default roles
            SeedRoles(builder);
        }

        private static void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole
                {
                    Id = Guid.Parse("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"),
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    Description = "Administrator role with full access",
                    CreatedAt = DateTime.UtcNow,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new ApplicationRole
                {
                    Id = Guid.Parse("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"),
                    Name = "User",
                    NormalizedName = "USER",
                    Description = "Standard user role",
                    CreatedAt = DateTime.UtcNow,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                }
            );
        }

        //private void SeedDefaultSurvey(ModelBuilder modelBuilder)
        //{
        //    // Seed default survey
        //    modelBuilder.Entity<Survey>().HasData(new Survey
        //    {
        //        Id = Guid.Parse("5c48d2cf-d296-4cd4-a533-97b31e55a68b"),
        //        Title = "Voting Experience Survey",
        //        Description = "Help us improve! Share your experience with our voting platform.",
        //        IsActive = true,
        //        ThankYouMessage = "Thank you for your valuable feedback! Your input helps us improve.",
        //        AllowMultipleResponses = false,
        //        CreatedAt = Convert.ToDateTime("2026-01-19"),
        //        UpdatedAt = Convert.ToDateTime("2026-01-19")
        //    });

        //    // Seed default questions
        //    modelBuilder.Entity<SurveyQuestion>().HasData(
        //        new SurveyQuestion
        //        {
        //            Id = Guid.Parse("ada9ce3f-c3dd-4083-aee9-e3de0d9f7cfa"),
        //            SurveyId = Guid.Parse("5c48d2cf-d296-4cd4-a533-97b31e55a68b"),
        //            QuestionText = "How would you rate your overall voting experience?",
        //            QuestionType = QuestionType.Rating,
        //            IsRequired = true,
        //            DisplayOrder = 1,
        //            MinValue = 1,
        //            MaxValue = 5,
        //            CreatedAt = Convert.ToDateTime("2026-01-19")
        //        },
        //        new SurveyQuestion
        //        {
        //            Id = Guid.Parse("a93ecc9a-9860-4f91-8e84-ce98867b651c"),
        //            SurveyId = Guid.Parse("5c48d2cf-d296-4cd4-a533-97b31e55a68b"),
        //            QuestionText = "Was the voting process easy to understand?",
        //            QuestionType = QuestionType.YesNo,
        //            IsRequired = true,
        //            DisplayOrder = 2,
        //            CreatedAt = Convert.ToDateTime("2026-01-19")
        //        },
        //        new SurveyQuestion
        //        {
        //            Id = Guid.Parse("0016bfbb-1bad-43ff-9e19-fcab88238fc3"),
        //            SurveyId = Guid.Parse("5c48d2cf-d296-4cd4-a533-97b31e55a68b"),
        //            QuestionText = "How satisfied are you with the website interface?",
        //            QuestionType = QuestionType.Rating,
        //            IsRequired = true,
        //            DisplayOrder = 3,
        //            MinValue = 1,
        //            MaxValue = 5,
        //            CreatedAt = Convert.ToDateTime("2026-01-19")
        //        },
        //        new SurveyQuestion
        //        {
        //            Id = Guid.Parse("b117642b-8440-4fad-b3c6-fde39000b5bd"),
        //            SurveyId = Guid.Parse("5c48d2cf-d296-4cd4-a533-97b31e55a68b"),
        //            QuestionText = "Did you encounter any technical issues?",
        //            QuestionType = QuestionType.YesNo,
        //            IsRequired = true,
        //            DisplayOrder = 4,
        //            CreatedAt = Convert.ToDateTime("2026-01-19")
        //        },
        //        new SurveyQuestion
        //        {
        //            Id = Guid.Parse("ea3fd4ad-8f23-4b56-be7f-380c7f3229be"),
        //            SurveyId = Guid.Parse("5c48d2cf-d296-4cd4-a533-97b31e55a68b"),
        //            QuestionText = "What did you like most about the voting process?",
        //            QuestionType = QuestionType.Text,
        //            IsRequired = false,
        //            DisplayOrder = 5,
        //            CreatedAt = Convert.ToDateTime("2026-01-19")
        //        },
        //        new SurveyQuestion
        //        {
        //            Id = Guid.Parse("265e66ea-67a5-4416-bbfe-d4420a3b4cd6"),
        //            SurveyId = Guid.Parse("5c48d2cf-d296-4cd4-a533-97b31e55a68b"),
        //            QuestionText = "What could we improve?",
        //            QuestionType = QuestionType.Text,
        //            IsRequired = false,
        //            DisplayOrder = 6,
        //            CreatedAt = Convert.ToDateTime("2026-01-19")
        //        },
        //        new SurveyQuestion
        //        {
        //            Id = Guid.Parse("d5fee1ab-2d88-43ba-bad5-7afc4dada945"),
        //            SurveyId = Guid.Parse("5c48d2cf-d296-4cd4-a533-97b31e55a68b"),
        //            QuestionText = "Would you recommend our voting platform to others?",
        //            QuestionType = QuestionType.YesNo,
        //            IsRequired = true,
        //            DisplayOrder = 7,
        //            CreatedAt = Convert.ToDateTime("2026-01-19")
        //        }
        //    );
        //}
    }
}
