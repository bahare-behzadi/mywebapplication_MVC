using mywebapplication.DataAccess.Data;
using mywebapplication.DataAccess.Repository.IRepository;
using mywebapplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace mywebapplication.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(Category categoryObj)
        {
            _db.Categories.Update(categoryObj);
        }


    }
}
