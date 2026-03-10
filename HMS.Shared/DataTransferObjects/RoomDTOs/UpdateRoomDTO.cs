using HMS.Shared.SharedEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Shared.DataTransferObjects.RoomDTOs
{
    public class UpdateRoomDTO
    {
        [Required(ErrorMessage = "Room Type is required.")]
        public RoomType RoomType { get; set; }

        [Required(ErrorMessage = "Room Description is Required.")]
        [MaxLength(200)]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Price is Reqiured.")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
        public decimal PricePerNight { get; set; }

        [Required(ErrorMessage = "Room must have at least one amenity.")]
        public string Amenities { get; set; } = null!;

        public RoomStatus RoomStatus { get; set; }
    }
}
