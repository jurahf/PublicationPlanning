using System;
using System.Collections.Generic;
using System.Text;

namespace PublicationPlanning.MainPageViewFeature.DragAndDrop
{
    public interface IDragDropContext
    {
        void StartOperation(string operationName);
    }
}
