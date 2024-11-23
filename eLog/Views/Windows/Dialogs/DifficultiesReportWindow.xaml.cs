using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace eLog.Views.Windows.Dialogs
{
    public partial class DifficultiesReportWindow : Window
    {
        public bool NonConformingWorkpiece { get; set; }
        public bool NoProgram { get; set; }
        public bool NoDocumentation { get; set; }
        public bool LackOfSkills { get; set; }
        public bool InsufficientTools { get; set; }
        public bool InsufficientEquipment { get; set; }
        public bool NeedMasterHelp { get; set; }
        public bool NeedTechnicalHelp { get; set; }
        public bool NeedSeniorHelp { get; set; }

        public DifficultiesReportWindow()
        {
            InitializeComponent();
        } 
    }
}