using System.ComponentModel.DataAnnotations;

namespace BorderCrossing.Models
{
    public enum IntervalType
    {
        [Display(Name = "По дням")]
        Day = 1,

        [Display(Name = "По часам")]
        Hour = 2
    }
}
