using mywebapplication.DataAccess.Data;
using mywebapplication.Models;
using mywebapplication.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mywebapplication.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader orderHeaderObj);
   
        void UpdateStatus(int orderId, string status, string? paymentStatus=null);
        void UpdateStripePaymentId(int orderId, string sessionId, string stripePaymentId);
    }   
}
