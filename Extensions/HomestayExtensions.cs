using DoAnCoSo_Nhom2.Models;

namespace DoAnCoSo_Nhom2.Extensions
{
    public static class HomestayExtensions
    {
        public static void UpdateDetails(this Homestay homestay, Homestay newDetails, List<string> newImages, List<int> amenityIds)
        {
            homestay.Name = newDetails.Name;
            homestay.Description = newDetails.Description;
            homestay.Address = newDetails.Address;
            homestay.City = newDetails.City;
            homestay.PricePerNight = newDetails.PricePerNight;
            homestay.IsApproved = newDetails.IsApproved;

            if (newImages.Count > 0)
            {
                homestay.Images = newImages;
            }

            homestay.HomestayAmenities.Clear();
            homestay.HomestayAmenities.AddRange(amenityIds.Select(id => new HomestayAmenity { AmenityId = id }));
        }
    }
}