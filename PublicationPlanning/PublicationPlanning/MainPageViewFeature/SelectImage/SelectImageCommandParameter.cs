using System;
using System.Collections.Generic;
using System.Text;

namespace PublicationPlanning.MainPageViewFeature.SelectImage
{
    public class SelectImageCommandParameter
    {
        public ISelectImageContext Interactor { get; }

        public ImageModelAndControl ClickedObject { get; }

        public SelectImageCommandParameter(ISelectImageContext context, ImageModelAndControl clickedObject)
        {
            this.Interactor = context;
            this.ClickedObject = clickedObject;
        }
    }
}
