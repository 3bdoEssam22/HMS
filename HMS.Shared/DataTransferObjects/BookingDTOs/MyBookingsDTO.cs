using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Shared.DataTransferObjects.BookingDTOs
{
    public class MyBookingsDTO
    {
        public int RoomId { get; set; }
        public string CheckInDate { get; set; } = null!;
        public string CheckOutDate { get; set; } = null!;
        public string Status { get; set; } = null!;
        public decimal TotalAmount { get; set; }
    }
}
