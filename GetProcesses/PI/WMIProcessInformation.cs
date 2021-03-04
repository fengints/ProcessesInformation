using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace GetProcesses.PI
{
    public class WMIProcessInformation
    {
        private readonly int pId;

        public WMIProcessInformation(int pId)
        {
            this.pId = pId;
        }
        public string GetProcessCommandLine()
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + pId))
            {

                using (ManagementObjectCollection objects = searcher.Get())
                {
                    return objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
                }
            }
        }

    }
}
