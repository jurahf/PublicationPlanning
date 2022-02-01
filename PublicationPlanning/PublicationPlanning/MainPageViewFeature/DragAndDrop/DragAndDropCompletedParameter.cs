using System;
using System.Collections.Generic;
using System.Text;

namespace PublicationPlanning.MainPageViewFeature.DragAndDrop
{
    public class DragAndDropCompletedParameter
    {
        public IDragDropContext Interactor { get; }

        public ImageModelAndControl DraggedObject { get; }

        public DragAndDropCompletedParameter(IDragDropContext context, ImageModelAndControl draggedObject)
        {
            this.Interactor = context;
            this.DraggedObject = draggedObject;
        }
    }
}
