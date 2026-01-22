namespace DoAnCoSo_Nhom2.Models
{
    public class HomestayAmenity
    {
        public int HomestayId { get; set; }
        public Homestay Homestay { get; set; }
        public int AmenityId { get; set; }
        public Amenity Amenity { get; set; }
    }
}