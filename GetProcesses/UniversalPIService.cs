using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Data;
using System.Management;
using GetProcesses.PI;
using GetProcesses.WinAPI;

namespace GetProcesses
{
    public class UniversalPIService : IProcessInfoService
    {
        public IEnumerable<ProcessInfo> GetProcessInfos()
        {
            //Get information about all processes
            var processes = Win32ProcessInformation.GetRunningProcessIDs();

            //Get information about every process
            foreach (var pId in processes)
            {
                Win32ProcessInformation proc = null;
                WMIProcessInformation wmiPI;
                ProcessInfo model = null;
                try
                {
                    proc = new Win32ProcessInformation(pId);
                    wmiPI = new WMIProcessInformation(pId);

                    //get winapi methods
                    model = new ProcessInfo()
                    {
                        UserName = proc.RunningUserName(),
                        BitDepth = proc.GetProcessBitness(),
                        FullName = proc.FullName(),
                        //icon = proc.ProcessIcon(),
                        Id = pId,
                        IsElevated = proc.IsElevated(),
                    };

                    //Set name
                    model.Name = System.IO.Path.GetFileName(model.FullName);

                    //digital sign
                    model.Sign = proc.SignatureExist(model.FullName);

                    //Wmi methods
                    model.CMDString = wmiPI.GetProcessCommandLine();

                }
                catch (ArgumentException)
                {
                    // cant open handle to processId
                }
                catch (Exception)
                {

                }
                finally
                {
                    proc?.Dispose();
                    //Return Model
                }
                if (model != null)
                    yield return model;
            }
        }
    }

    public interface IProcessInfoService
    {
        IEnumerable<ProcessInfo> GetProcessInfos();
    }


    


}
