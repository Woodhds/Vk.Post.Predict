﻿using Microsoft.EntityFrameworkCore;

namespace Vk.Post.Predict.Entities
{
    public class DataContext : DbContext
    {
        public DbSet<Message> Messages { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>()
                .Property(f => f.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<Message>()
                .HasIndex(f => new {f.Id, f.OwnerId});

            base.OnModelCreating(modelBuilder);
        }
    }
}
