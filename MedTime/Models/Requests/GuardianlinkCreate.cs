using System.ComponentModel.DataAnnotations;

namespace MedTime.Models.Requests
{
    public class GuardianlinkCreate
    {
        [Required(ErrorMessage = "Guardian ID is required")]
        public int Guardianid { get; set; }

        [Required(ErrorMessage = "Patient ID is required")]
        public int Patientid { get; set; }
    }
}
