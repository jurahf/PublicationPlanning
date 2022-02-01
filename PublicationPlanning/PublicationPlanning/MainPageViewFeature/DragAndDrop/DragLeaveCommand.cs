using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace PublicationPlanning.MainPageViewFeature.DragAndDrop
{
    public class DragLeaveCommand : ICommand
    {
        public event EventHandler CanExecuteChanged = delegate { };

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var context = parameter as DropOverCommandParameter;

            if (context == null)
                return;

            context.Interactor.OnDragLeave(context.DragOverObject);
        }
    }
}
