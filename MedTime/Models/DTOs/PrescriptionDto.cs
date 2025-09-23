namespace MedTime.Models.DTOs
{
    public class PrescriptionDto
    {
        public int Prescriptionid { get; set; }

        public int Userid { get; set; }

        public int Medicineid { get; set; }

        public string? Dosage { get; set; }

        public int? Frequencyperday { get; set; }

        public DateOnly? Startdate { get; set; }

        public DateOnly? Enddate { get; set; }

        public int? Remainingquantity { get; set; }

        public string? Doctorname { get; set; }

        public string? Notes { get; set; }
    }
}
