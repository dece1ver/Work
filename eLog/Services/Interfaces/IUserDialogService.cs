using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eLog.Services.Interfaces
{
    internal interface IUserDialogService
    {
        bool Edit(object item);

        void ShowInfo(string message, string caption);

        void ShowWarning(string message, string caption);

        void ShowError(string message, string caption);

        bool Confirm(string message, string caption, bool Exclamation = false);
        bool GetBarCode(ref string barCode);
    }
}
