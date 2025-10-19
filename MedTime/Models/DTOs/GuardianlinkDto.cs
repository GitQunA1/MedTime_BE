namespace MedTime.Models.DTOs
{
    public class GuardianlinkDto
    {
        public int Guardianid { get; set; }

        public string? GuardianFullname { get; set; }

        public int Patientid { get; set; }

        public string? PatientFullname { get; set; }

        public DateTime? Createdat { get; set; }
    }
}
