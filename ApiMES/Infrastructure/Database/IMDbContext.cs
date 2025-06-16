using ApiMES.Application.DTOs.Auth;
using ApiMES.Domain.Entities.Auth;
using ApiMES.Domain.Entities.Menu;
using ApiMES.Domain.Entities.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security;

namespace ApiMES.Infrastructure.Database
{
    public class IMDbContext : IdentityDbContext<ApplicationUser>
    {
        public IMDbContext(DbContextOptions<IMDbContext> options) : base(options) { }

        public DbSet<MenuItem> MenuItems { get; set; }

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CheckTCodeDTO>().HasNoKey();

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(r => r.Token).IsUnique();

                entity.HasOne(r => r.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
