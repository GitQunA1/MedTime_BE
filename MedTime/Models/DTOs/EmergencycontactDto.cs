namespace MedTime.Models.DTOs
{
    public class EmergencycontactDto
    {
        public int Contactid { get; set; }

        public int Userid { get; set; }

        public string Name { get; set; } = null!;

        public string? Phonenumber { get; set; }

        public string? Relation { get; set; }
    }
}
