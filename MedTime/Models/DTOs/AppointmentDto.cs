namespace MedTime.Models.DTOs
{
    public class AppointmentDto
    {
        public int Appointmentid { get; set; }

        public int Userid { get; set; }

        public string? Doctorname { get; set; }

        public string? Hospitalname { get; set; }

        public DateTime? Appointmentdate { get; set; }

        public string? Notes { get; set; }
    }
}
