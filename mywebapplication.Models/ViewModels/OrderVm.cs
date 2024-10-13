using mywebapplication.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mywebapplication.Models.ViewModels
{
    public class OrderVm
    {
        public OrderHeader Header { get; set; }
        public IEnumerable<OrderDetail> Details { get; set; }
    }
}
