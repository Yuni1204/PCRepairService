using PCRepairService.Interfaces;
using PCRepairService.Models;

namespace PCRepairService
{
    public class RepairTimer : IRepairTimer
    {
        private IServiceScopeFactory _serviceScopeFactory;
        List<RepairStopTime> _stoppedTime;

        public RepairTimer(IServiceScopeFactory serviceScopeFactory) {
            _serviceScopeFactory = serviceScopeFactory;
            _stoppedTime = new List<RepairStopTime>();
        }

        public void AddStoppedTime(RepairStopTime stoppedtime)
        {
            var target = _stoppedTime.FirstOrDefault(obj => obj.ServiceOrderId == stoppedtime.ServiceOrderId);
            if(target != null)
            {
                target.StopTime += stoppedtime.StopTime;
            }
            else _stoppedTime.Add(stoppedtime);
        }

        public async Task SaveStoppedTime(long id)
        {
            var target = _stoppedTime.FirstOrDefault(obj => obj.ServiceOrderId == id);
            if(target != null)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<IDA_StopTime>();
                    if (dbContext != null)
                    {
                        await dbContext.AddNewStopTimeAsync(target);
                    }
                }
            }
        }
    }
}
