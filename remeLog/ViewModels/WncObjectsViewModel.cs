using libeLog;
using libeLog.Base;
using remeLog.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace remeLog.ViewModels
{
    public class WncObjectsViewModel : ViewModel
    {
        public WncObjectsViewModel(ObservableCollection<WncObject> wncObjects)
        {
            WncObjects = wncObjects;
            OpenLinkCommand = new LambdaCommand(OnOpenLinkCommandExecuted, CanOpenLinkCommandExecute);
        }

        public ObservableCollection<WncObject> WncObjects { get; set; }

        #region ClearContent
        public ICommand OpenLinkCommand { get; }
        private void OnOpenLinkCommandExecuted(object p)
        {
            if (p is WncObject wncObject)
            {
                Process.Start(new ProcessStartInfo(wncObject.Link) { UseShellExecute = true });
            }
        }
        private static bool CanOpenLinkCommandExecute(object p) => true;
        #endregion
    }
}
