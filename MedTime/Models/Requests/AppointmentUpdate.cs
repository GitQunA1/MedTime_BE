namespace MedTime.Models.Requests
{
    public class AppointmentUpdate
    {
        public string? Doctorname { get; set; }

        public string? Hospitalname { get; set; }

        public DateTime? Appointmentdate { get; set; }

        public string? Notes { get; set; }
    }
}
