using PublicationPlanning.MainPageViewFeature.SelectImage;
using System;
using System.Collections.Generic;
using System.Text;

namespace PublicationPlanning.MainPageViewFeature.DragAndDrop
{
    public class DropOverCommandParameter
    {
        public IDragDropContext Interactor { get; }

        public DragDropInfo DragOverObject { get; }

        public DropOverCommandParameter(
            IDragDropContext context,
            DragDropInfo info)
        {
            this.Interactor = context;
            this.DragOverObject = info;
        }
    }
}
