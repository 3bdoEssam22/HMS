using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Core.Entities.BookingModule
{
    public enum BookingStatus
    {
        PendingPayment = 0,
        Paid = 1,
        Canceled = 2,
        Failed = 3

    }
}
