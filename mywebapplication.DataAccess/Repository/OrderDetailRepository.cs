using mywebapplication.DataAccess.Data;
using mywebapplication.DataAccess.Repository.IRepository;
using mywebapplication.Models;
using mywebapplication.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace mywebapplication.DataAccess.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private ApplicationDbContext _db;
        public OrderDetailRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(OrderDetail orderDetailObj)
        {
            _db.OrderDetails.Update(orderDetailObj);
        }


    }
}
