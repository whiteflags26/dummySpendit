﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace dummySpendit.Models
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, one can rename the ASP.NET Identity table names and more.
            // Add customizations after calling base.OnModelCreating(builder);
        }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SpenditUser> SpenditUsers { get; set;}

        public void filterTransactions()
        {
            var temp = Transactions;
            foreach (Transaction transaction in temp)
            {
                if (0 == 1)
                {
                    Transactions.Remove(transaction);
                }
            }
        }

        public void filterCategories()
        {
            var temp = Categories;
            foreach (Category category in temp)
            {
                if (0 == 1)
                {
                    Categories.Remove(category);
                }
            }
        }
    }
}