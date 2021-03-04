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
        public async Task Initialize()
        {
            var info = new StatusInfo(0, 100);
            var r = this.UpdateAsync(info, new CancellationToken());
        }
        public async Task<bool> UpdateAsync(StatusInfo info, CancellationToken token) // return remain time information
        {
            int len = 100;
            info.StatusMax = len;

            var tempData = await Task.Run(() => GetData(info, token));

            if (token.IsCancellationRequested) {
                return false;
            }

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

            return true;
        }
        private List<ProcessInfo> GetData(StatusInfo si, CancellationToken token)
        {
            var data = new List<ProcessInfo>();
            foreach (var p in service.GetProcessInfos().Where(x => !String.IsNullOrEmpty(x.FullName)))
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                data.Add(p);
                si.CurrentStatus++;
            }
            return data;
        }

    }

    public interface IProcessInfoViewModel
    {
        ObservableCollection<ProcessInfo> ProcessInfos { get; }
        Task Initialize();
        Task<bool> UpdateAsync(StatusInfo info, CancellationToken token);
    }
}
