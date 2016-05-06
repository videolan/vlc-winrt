using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VLC_WinRT.Utils
{
    public abstract class AttemptCommand
    {
        public abstract Task<bool> Execute(object parameter);
    }
}
