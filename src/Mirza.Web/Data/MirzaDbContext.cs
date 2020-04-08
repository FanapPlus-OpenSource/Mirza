﻿using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Mirza.Web.Models;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Mirza.Web.Data
{
    public class MirzaDbContext : IdentityDbContext<MirzaUser, MirzaRole, int>
    {
        public MirzaDbContext(DbContextOptions<MirzaDbContext> options)
            : base(options)
        {
        }

        public DbSet<MirzaUser> UserSet { get; set; }
        public DbSet<Team> TeamSet { get; set; }
        public DbSet<WorkLog> WorkLogSet { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            _ = modelBuilder
                .Entity<MirzaUser>(b =>
                {
                    b.ToTable("Users");

                    b.HasKey(a => a.Id);

                    b.Property(a => a.FirstName)
                     .IsRequired()
                     .HasMaxLength(40);

                    b.Property(a => a.LastName)
                     .IsRequired()
                     .HasMaxLength(50);

                    b.Property(a => a.IsActive)
                     .IsRequired();

                    b.Property(a => a.Email)
                     .IsRequired();

                    b.HasOne(a => a.Team)
                     .WithMany(a => a.Members)
                     .HasForeignKey(a => a.TeamId);

                    b.HasMany(a => a.AccessKeys)
                     .WithOne(a => a.Owner)
                     .HasForeignKey(a => a.OwnerId);

                    b.HasMany(a => a.WorkLog)
                     .WithOne(a => a.User)
                     .HasForeignKey(a => a.UserId);
                })
                .Entity<Team>(b =>
                {
                    b.ToTable("Teams");

                    b.HasKey(a => a.Id);

                    b.Property(a => a.Name)
                     .IsRequired()
                     .HasMaxLength(50);

                    b.HasMany(a => a.Members)
                     .WithOne(a => a.Team)
                     .HasForeignKey(a => a.TeamId)
                     .OnDelete(DeleteBehavior.NoAction);
                })
                .Entity<WorkLog>(b =>
                {
                    b.ToTable("WorkLogs");

                    b.HasKey(a => a.Id);

                    b.Property(a => a.LogDate)
                    .IsRequired()
                    .HasColumnType("datetime2");

                    b.Property(a => a.EntryDate)
                     .HasColumnType("date")
                     .IsRequired();

                    b.Property(a => a.StartTime)
                     .HasColumnType("time")
                     .IsRequired();

                    b.Property(a => a.EndTime)
                     .HasColumnType("time")
                     .IsRequired();
                })
                .Entity<AccessKey>(b =>
                {
                    b.ToTable("AccessKeys");

                    b.HasKey(a => a.Id);

                    b.HasIndex(a => new { a.OwnerId, a.State })
                     .HasName("IDX_Owner_State");

                    b.Property(a => a.Key)
                     .IsRequired()
                     .HasColumnType("char(32)")
                     .IsFixedLength();

                    b.Property(a => a.State)
                     .IsRequired()
                     .HasConversion(new EnumToStringConverter<AccessKeyState>());

                    b.Property(a => a.Expiration)
                     .IsRequired()
                     .HasColumnType("datetime2");
                });
        }
    }
}