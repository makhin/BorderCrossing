using BorderCrossing.Models;
using BorderCrossing.Services;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

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
        public IBorderCrossingServiceWebWrapper BorderCrossingServiceWebWrapper { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Parameter]
        public string RequestId { get; set; }

        public QueryRequest QueryRequest { get; set; }

        public int PercentageProc { get; set; }

        public bool IsDemo = false;

        protected PageStatus Status { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Status = PageStatus.AskParameter;
            QueryRequest = await BorderCrossingServiceWebWrapper.GetQueryRequestAsync(RequestId);
        }

        public async Task Start()
        {
            Status = PageStatus.Processing;
            this.StateHasChanged();
            var checkpoints = await BorderCrossingServiceWebWrapper.ParseLocationHistoryAsync(RequestId, QueryRequest, (sender, e) =>
            {
                PercentageProc = e.ProgressPercentage;
                InvokeAsync(StateHasChanged);
            });

            await BorderCrossingServiceWebWrapper.UpdateResultAsync(RequestId, checkpoints);
            NavigationManager.NavigateTo($"result/{RequestId}");
        }
    }
}
