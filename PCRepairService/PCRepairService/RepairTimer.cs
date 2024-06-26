﻿using PCRepairService.Interfaces;
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
            var tmpList = new List<RepairStopTime>(_stoppedTime);
            var target = tmpList.FirstOrDefault(obj => obj.ServiceOrderId == stoppedtime.ServiceOrderId);
            if(target != null)
            {
                //target.StopTime += stoppedtime.StopTime;
                _stoppedTime.FirstOrDefault(obj => obj.ServiceOrderId == stoppedtime.ServiceOrderId).StopTime = stoppedtime.StopTime;
            }
            else _stoppedTime.Add(stoppedtime);
        }

        public async Task SaveStoppedTime(long id)
        {
            var tmpList = new List<RepairStopTime>(_stoppedTime);
            var target = tmpList.FirstOrDefault(obj => obj.ServiceOrderId == id);
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
                TimeSpan diff = timestamp.Timestamp1 - target.Timestamp1;
                target.Duration = (diff.TotalMilliseconds < 0) ? (diff.TotalMilliseconds * -1) : diff.TotalMilliseconds;
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
