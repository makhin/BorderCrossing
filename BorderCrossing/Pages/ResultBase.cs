using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BorderCrossing.DbContext;
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

        public BorderCrossingResponse Response { get; set; }

        public string Message { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Message = "Загрузка...";
            this.StateHasChanged();

            var checkPoints = await BorderCrossingService.GetResultAsync(RequestId);

            if (!checkPoints.Any())
            {
                Message = "Результатов не найдено";
                this.StateHasChanged();
                return;
            }

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
    }
}
