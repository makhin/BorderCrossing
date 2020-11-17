using BorderCrossing.Models;
using BorderCrossing.Res;
using BorderCrossing.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Linq;
using System.Threading.Tasks;

namespace BorderCrossing.Pages
{
    public class ResultBase : ComponentBase
    {
        [Inject]
        public IBorderCrossingServiceWebWrapper BorderCrossingServiceWebWrapper { get; set; }

        [Parameter]
        public string RequestId { get; set; }

        [Inject]
        private IStringLocalizer<SharedResource> L { get; set; }

        public BorderCrossingResponse Response { get; set; }

        public string Message { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Message = L["UploadP2"];
            this.StateHasChanged();

            var checkPoints = await BorderCrossingServiceWebWrapper.GetResultAsync(RequestId);

            if (!checkPoints.Any())
            {
                Message = L["ResultNodFound"];
            }
            else
            {
                Response = new BorderCrossingResponse();

                var arrivalPoint = checkPoints.First();
                Response.Periods.Add(new Period
                {
                    ArrivalPoint = arrivalPoint,
                    Country = arrivalPoint.CountryName
                });
                var last = Response.Periods.Last();

                foreach (var checkPoint in checkPoints.Skip(1).Take(checkPoints.Count - 2))
                {
                    last.DeparturePoint = checkPoint;
                    Response.Periods.Add(new Period
                    {
                        ArrivalPoint = checkPoint,
                        Country = checkPoint.CountryName,
                    });
                    last = Response.Periods.Last();
                }

                last.DeparturePoint = checkPoints.Last();
            }

            this.StateHasChanged();
        }
    }
}
