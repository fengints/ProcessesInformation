using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF.Tools
{
    class ProcessComparer : IEqualityComparer<ProcessInfo>
    {
        #region IEqualityComparer<Contact> Members

        public bool Equals(ProcessInfo x, ProcessInfo y)
        {
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(ProcessInfo obj)
        {
            return obj.Id.GetHashCode();
        }

        #endregion
    }
}
