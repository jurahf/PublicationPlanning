using System;
using System.Collections.Generic;
using System.Text;

namespace PublicationPlanning.Settings
{
    public class DefaultSettings : ISettings
    {
        public bool ResizeImages { get; set; }
        public int ImageResizeWidth { get; set; }
        public int ImageResizeHeight { get; set; }

        public DefaultSettings()
        {
            ResizeImages = true;
            ImageResizeWidth = 800;
            ImageResizeHeight = 800;
        }
    }
}
