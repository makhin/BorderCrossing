using System.ComponentModel.DataAnnotations;

namespace BorderCrossing.Models
{
    public enum IntervalType
    {
        [Display(Name = "24 часа")]
        Day = 1,

        [Display(Name = "12 часов")]
        Every12Hours = 2,

        [Display(Name = "1 час")]
        Hour = 3
    }
}
