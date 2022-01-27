using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace PublicationPlanning.MainPageViewFeature.SelectImage
{
    public class SelectImageCommand : ICommand
    {
        public event EventHandler CanExecuteChanged = delegate { };

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            SelectImageCommandParameter context = parameter as SelectImageCommandParameter;
            var oldSelection = context.Interactor.GetSelection();
            context.Interactor.ClearSelection();
            
            if (oldSelection == null || oldSelection.ImageInfoId != context.ClickedObject.ImageInfoId)
            {
                context.Interactor.SetSelection(context.ClickedObject);
            }
        }
    }
}
