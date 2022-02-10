using System;
using System.Collections.Generic;
using System.Text;

namespace PublicationPlanning.ViewModels
{
    public class SettingsViewModel : IViewEntity
    {
        public int Id { get; set; }

        /// <summary>
        /// Использовать изменение размера для хранения изображений
        /// </summary>
        public bool ResizeImages { get; set; }

        /// <summary>
        /// Задает ширину для преобразования картинок, используется, если включена настройка изменения размера картинок для хранения
        /// </summary>
        public int ImageResizeWidth { get; set; }

        /// <summary>
        /// Задает высоту для преобразования картинок, используется, если включена настройка изменения размера картинок для хранения
        /// </summary>
        public int ImageResizeHeight { get; set; }

        /// <summary>
        /// Сколько картинок загружать при постраничной загрузке
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Сколько столбцов из картинок формировать на странице
        /// </summary>
        public int ColumnsCount { get; set; }

        /// <summary>
        /// Величина обрамления вокруг картинок
        /// </summary>
        public int ImageSpacingPixels { get; set; }
    }
}
