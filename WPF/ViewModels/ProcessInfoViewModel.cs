using Data;
using GetProcesses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WPF.Tools;

namespace WPF.ViewModels
{

    public class ProcessInfoViewModel : IProcessInfoViewModel
    {
        public ProcessInfoViewModel()
        {
            this.service = new UniversalPIService();
        }
        IProcessInfoService service;

        public ObservableCollection<ProcessInfo> ProcessInfos { get; } = new ObservableCollection<ProcessInfo>();
        public StatusInfo Initialize()
        {
            return new StatusInfo(0, 100);
        }
        public async Task UpdateAsync(StatusInfo info, CancellationToken token) // return remain time information
        {
            int len = 100;
            info.StatusMax = len;

            var tempData = await Task.Run(async () => await GetData(info, token));

            info.CurrentStatus = len;

            var comparer = new ProcessComparer();
            //Delete old elements
            var old = ProcessInfos.Except(tempData, comparer).ToList();
            foreach (var d in old)
            {
                ProcessInfos.Remove(d);
            }

            //Add new elements
            var newElements = tempData.Except(ProcessInfos, comparer);
            foreach (var element in newElements)
            {
                ProcessInfos.Add(element);
            }
        }
        private async Task<List<ProcessInfo>> GetData(StatusInfo si, CancellationToken token)
        {
            var data = new List<ProcessInfo>();
            foreach (var p in service.GetProcessInfos().Where(x => !String.IsNullOrEmpty(x.FullName)))
            {
                if (token.IsCancellationRequested)
                {
                    return await Task.FromResult(data);
                    //There is no return Task.CompletedTask 
                }

                data.Add(p);
                si.CurrentStatus++;
                await Task.Delay(10);
            }
            return data;
        }

    }

    public interface IProcessInfoViewModel
    {
        ObservableCollection<ProcessInfo> ProcessInfos { get; }
        StatusInfo Initialize();
        Task UpdateAsync(StatusInfo info, CancellationToken token);
    }
}
