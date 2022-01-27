using System;
using System.Collections.Generic;
using System.Text;

namespace PublicationPlanning.MainPageViewFeature.SelectImage
{
    public interface ISelectImageContext
    {
        void ClearSelection();

        void SetSelection(ImageModelAndControl selected);

        ImageModelAndControl GetSelection();
    }
}
