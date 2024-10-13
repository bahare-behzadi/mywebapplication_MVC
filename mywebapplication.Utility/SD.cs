using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mywebapplication.Utility
{
    public class SD
    {
        public const string Role_User_Customer = "Customer";
        public const string Role_User_Company = "Company";
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";

        public const string statusPending = "Pending";
		public const string statusApproved = "Approved";
		public const string statusInProcess = "Processing";
		public const string statusShiped = "Shipped";
		public const string statusCanceled = "Canceled";
		public const string statusRefund = "Refund";

		public const string PaymentstatusPending = "Pending";
		public const string PaymentstatusApproved = "Approved";
		public const string PaymentstatusDelayedPayment = "ApprovedForDelayedPayment";
		public const string PaymentstatusRejected = "Rejected";

		public const string SessionCart = "SessionShoppingCart";

	}
}
