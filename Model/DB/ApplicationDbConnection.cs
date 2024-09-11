using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApiIdentity_security.Model.DB
{
    public class ApplicationDbConnection :IdentityDbContext<IdentityUser>
    {

        public ApplicationDbConnection(DbContextOptions<ApplicationDbConnection> options):base(options) 

        {

            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            seeRole(builder);
        }

        // role set 

        private static void seeRole(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole() { Name="Admin",ConcurrencyStamp="1",NormalizedName="Admin"},
                new IdentityRole() { Name="User",ConcurrencyStamp="2",NormalizedName="User"},
                new IdentityRole() { Name="HR",ConcurrencyStamp="3",NormalizedName="HR"}
                );
        }
    }

     
}
