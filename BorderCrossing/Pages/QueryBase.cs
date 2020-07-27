using System.Threading.Tasks;
using BorderCrossing.Models;
using BorderCrossing.Services;
using Microsoft.AspNetCore.Components;

namespace BorderCrossing.Pages
{
    public class QueryBase : ComponentBase
    {
        protected enum PageStatus
        {
            AskParameter,
            Processing,
        }

        [Inject]
        public IBorderCrossingService BorderCrossingService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Parameter]
        public string RequestId { get; set; }

        public QueryRequest QueryRequest { get; set; }

        public int PercentageProc { get; set; }

        public bool IsDemo = true;

        protected PageStatus Status { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Status = PageStatus.AskParameter;
            QueryRequest = await BorderCrossingService.GetQueryRequestAsync(RequestId);
        }

        public async Task Start()
        {
            Status = PageStatus.Processing;
            this.StateHasChanged();
            await BorderCrossingService.ParseLocationHistoryAsync(RequestId, QueryRequest, (sender, e) =>
            {
                PercentageProc = e.ProgressPercentage;
                InvokeAsync(StateHasChanged);
            });

            NavigationManager.NavigateTo($"result/{RequestId}");
        }
    }
}
