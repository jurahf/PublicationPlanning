using System;
using System.Collections.Generic;
using System.Text;

namespace PublicationPlanning.Settings
{
    public interface ISettings
    {
        bool ResizeImages { get; set; }
        int ImageResizeWidth { get; set; }
        int ImageResizeHeight { get; set; }
    }
}
