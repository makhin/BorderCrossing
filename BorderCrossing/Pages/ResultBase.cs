using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BorderCrossing.Models;
using BorderCrossing.Services;
using Microsoft.AspNetCore.Components;

namespace BorderCrossing.Pages
{
    public class ResultBase : ComponentBase
    {
        [Inject]
        public IBorderCrossingService BorderCrossingService { get; set; }

        [Parameter]
        public string RequestId { get; set; }

        public BorderCrossingResponse Result { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var checkPoints = await BorderCrossingService.GetResultAsync(RequestId);

            if (!checkPoints.Any())
            {
                return;
            }

            var response = new BorderCrossingResponse();

            var arrivalPoint = checkPoints.First();
            response.Periods.Add(new Period()
            {
                ArrivalPoint = arrivalPoint,
                Country = arrivalPoint.CountryName
            });
            var last = response.Periods.Last();

            foreach (var checkPoint in checkPoints.Skip(1))
            {
                last.DeparturePoint = checkPoint;
                response.Periods.Add(new Period
                {
                    ArrivalPoint = checkPoint,
                    Country = checkPoint.CountryName,
                });
                last = response.Periods.Last();
            }

            last.DeparturePoint = checkPoints.Last();
        }
    }
}
