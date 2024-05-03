using PCRepairService.Interfaces;
using PCRepairService.Models;

namespace PCRepairService
{
    public class RepairTimer : IRepairTimer
    {
        private IServiceScopeFactory _serviceScopeFactory;
        List<RepairStopTime> _stoppedTime;
        List<Timestamps> _irlDuration;

        public RepairTimer(IServiceScopeFactory serviceScopeFactory) {
            _serviceScopeFactory = serviceScopeFactory;
            _stoppedTime = new List<RepairStopTime>();
            _irlDuration = new List<Timestamps>();
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

        public void AddIrlDuration(Timestamps timestamp)
        {
            var target = _irlDuration.FirstOrDefault(obj => obj.ServiceOrderId == timestamp.ServiceOrderId);
            if (target != null)
            {
                target.Timestamp2 = timestamp.Timestamp1;
                TimeSpan diff = target.Timestamp1 - target.Timestamp1;
                target.Duration = (diff.TotalMilliseconds < 0) ? (diff * -1) : diff;
            }
            else _irlDuration.Add(timestamp);
        }

        public async Task SaveDuration(long id)
        {
            var target = _irlDuration.FirstOrDefault(obj => obj.ServiceOrderId == id);
            if (target != null)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<IDA_Timestamps>();
                    if (dbContext != null)
                    {
                        await dbContext.AddTimeSpanAsync(target);
                    }
                }
            }
        }

    }
}
