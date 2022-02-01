using System;
using System.Collections.Generic;
using System.Text;

namespace PublicationPlanning.MainPageViewFeature.DragAndDrop
{
    public interface IDragDropContext
    {
        void OnDragOver(DragDropInfo landing);

        void OnDragLeave(DragDropInfo landing);

        void CompleteDrop(ImageModelAndControl src);
    }
}
