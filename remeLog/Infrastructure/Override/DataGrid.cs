﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Infrastructure.Override
{
    public partial class DataGrid : System.Windows.Controls.DataGrid
    {

        /// <summary>
        /// This method overrides the 
        /// if (canExecute && HasRowValidationError) condition of the base method to allow
        /// ----entering edit mode when there is a pending validation error
        /// ---editing of other rows
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCanExecuteBeginEdit(System.Windows.Input.CanExecuteRoutedEventArgs e)
        {

            bool hasCellValidationError = false;
            bool hasRowValidationError = false;
            BindingFlags bindingFlags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance;
            //Current cell
            PropertyInfo cellErrorInfo = this.GetType().BaseType.GetProperty("HasCellValidationError", bindingFlags);
            //Grid level
            PropertyInfo rowErrorInfo = this.GetType().BaseType.GetProperty("HasRowValidationError", bindingFlags);

            if (cellErrorInfo != null) hasCellValidationError = (bool)cellErrorInfo.GetValue(this, null);
            if (rowErrorInfo != null) hasRowValidationError = (bool)rowErrorInfo.GetValue(this, null);

            base.OnCanExecuteBeginEdit(e);
            if (!e.CanExecute && !hasCellValidationError && hasRowValidationError)
            {
                e.CanExecute = true;
                e.Handled = true;
            }
        }

        #region baseOnCanExecuteBeginEdit
        //protected virtual void OnCanExecuteBeginEdit(CanExecuteRoutedEventArgs e)
        //{
        //    bool canExecute = !IsReadOnly && (CurrentCellContainer != null) && !IsEditingCurrentCell && !IsCurrentCellReadOnly && !HasCellValidationError;

        //    if (canExecute && HasRowValidationError)
        //    {
        //        DataGridCell cellContainer = GetEventCellOrCurrentCell(e);
        //        if (cellContainer != null)
        //        {
        //            object rowItem = cellContainer.RowDataItem;

        //            // When there is a validation error, only allow editing on that row
        //            canExecute = IsAddingOrEditingRowItem(rowItem);
        //        }
        //        else
        //        {
        //            // Don't allow entering edit mode when there is a pending validation error
        //            canExecute = false;
        //        }
        //    }

        //    e.CanExecute = canExecute;
        //    e.Handled = true;
        //}
        #endregion baseOnCanExecuteBeginEdit
    }
}
