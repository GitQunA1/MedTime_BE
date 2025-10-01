using System.ComponentModel.DataAnnotations;

namespace MedTime.Models.Requests
{
    public class GuardianlinkUpdate
    {
        [Required(ErrorMessage = "Guardian ID is required")]
        public int Guardianid { get; set; }

        [Required(ErrorMessage = "Patient ID is required")]
        public int Patientid { get; set; }
    }
}
