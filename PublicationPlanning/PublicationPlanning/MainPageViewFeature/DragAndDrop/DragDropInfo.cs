using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace PublicationPlanning.MainPageViewFeature.DragAndDrop
{
    public class DragDropInfo
    {
        public int ImageInfoId { get; set; }

        public View Control { get; set; }

        public DropOnObjectDirection Direction { get; }

        public DragDropInfo(int imageInfoId, View control, DropOnObjectDirection direction)
        {
            this.ImageInfoId = imageInfoId;
            this.Control = control;
            this.Direction = direction;
        }
    }
}
