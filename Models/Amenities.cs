namespace DoAnCoSo_Nhom2.Models
{
    public class Amenity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<HomestayAmenity> HomestayAmenities { get; set; }
    }
}