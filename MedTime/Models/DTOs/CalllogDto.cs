using MedTime.Models.Enums;

namespace MedTime.Models.DTOs
{
    public class CalllogDto
    {
        public int Callid { get; set; }

        public int Userid { get; set; }

        public int? Scheduleid { get; set; }

        public DateTime? Calltime { get; set; }

        public CallStatusEnum? Status { get; set; }
    }
}
