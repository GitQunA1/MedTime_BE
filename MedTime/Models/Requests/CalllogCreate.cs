using MedTime.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MedTime.Models.Requests
{
    public class CalllogCreate
    {
        public int? Scheduleid { get; set; }

        public DateTime? Calltime { get; set; }

        public CallStatusEnum? Status { get; set; }
    }
}
