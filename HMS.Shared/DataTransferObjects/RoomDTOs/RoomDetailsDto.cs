namespace HMS.Shared.DataTransferObjects.RoomDTOs
{
    public class RoomDetailsDto
    {
        public string RoomType { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal PricePerNight { get; set; }
        public string Amenties { get; set; } = null!;
        public List<string> ImagesUrl { get; set; } = [];
        public string roomStatus { get; set; } = null!;
    }
}
