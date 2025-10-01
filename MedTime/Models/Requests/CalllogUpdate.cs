using MedTime.Models.Enums;

namespace MedTime.Models.Requests
{
    public class CalllogUpdate
    {
        public int? Scheduleid { get; set; }

        public DateTime? Calltime { get; set; }

        public CallStatusEnum? Status { get; set; }
    }
}
