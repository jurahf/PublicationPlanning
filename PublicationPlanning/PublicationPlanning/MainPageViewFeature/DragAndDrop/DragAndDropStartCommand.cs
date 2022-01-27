using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace PublicationPlanning.MainPageViewFeature.DragAndDrop
{
    public class DragAndDropStartCommand : ICommand
    {
        public event EventHandler CanExecuteChanged = delegate { };

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {

        }
    }
}
