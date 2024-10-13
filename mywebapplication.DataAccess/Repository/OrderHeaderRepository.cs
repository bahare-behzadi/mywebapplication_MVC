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
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(OrderHeader orderHeaderObj)
        {
            _db.OrderHeaders.Update(orderHeaderObj);
        }

        public void UpdateStatus(int orderId, string status, string? paymentStatus = null)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u=>u.OrderHeaderId==orderId);
            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus = status; 
                if(!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentId(int orderId, string sessionId, string stripePaymentId)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.OrderHeaderId == orderId);

                if (!string.IsNullOrEmpty(sessionId))
                {
                    orderFromDb.SessionId = sessionId;
                }
            if (!string.IsNullOrEmpty(stripePaymentId))
            {
                orderFromDb.PaymentIntentId = stripePaymentId;
                orderFromDb.PaymentDate = DateTime.Now;
            }
        }
    }
}
