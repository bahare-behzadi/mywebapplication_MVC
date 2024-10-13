using Microsoft.EntityFrameworkCore;
using WebApplicationRzor.Model;

namespace WebApplicationRzor.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Category[] categoryList = [new Category { CategoryOrder = 1 , Name = "Action", Id=1},
                                       new Category {CategoryOrder = 2, Name = "SciFi", Id = 2},
                                       new Category {CategoryOrder = 3, Name = "History", Id=3},
                                       new Category {CategoryOrder =4, Name = "Horror", Id = 4}];
            modelBuilder.Entity<Category>().HasData(categoryList);
        }
    }
}
