using PublicationPlanning.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace PublicationPlanning
{
    public class ImageModelAndControl
    {
        public int ImageInfoId { get; set; }

        public View Control { get; set; }

        public ImageModelAndControl(int id, View control)
        {
            this.ImageInfoId = id;
            this.Control = control;
        }
    }
}
