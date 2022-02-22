using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PublicationPlanning.ImageTranslations;
using PublicationPlanning.StoredModels;

namespace PublicationPlanning.Repositories
{
    public interface IImageInfoRepository : IRepository<ImageInfo>
    {
        Task<IEnumerable<ImageInfo>> GetByOrders(int startOrder, int endOrder);

        Task RotateImage(ImageInfo imageInfo, float degrees);
    }

    public class ImageInfoFileRepository : BaseFileRepository<ImageInfo>, IImageInfoRepository
    {
        private readonly IImageResizer imageResizer;
        private readonly IImageRotator imageRotator;
        private readonly ISettingsRepository settingsRepository;

        public ImageInfoFileRepository(
            IImageResizer imageResizer,
            IImageRotator imageRotator, 
            ISettingsRepository settingsRepository)
            : base()
        {
            this.imageResizer = imageResizer;
            this.imageRotator = imageRotator;
            this.settingsRepository = settingsRepository;
            

            // TODO: метод для проверки кэша картинок и удаления ненужных, удаления не новейших версий файла
        }

        public Task<IEnumerable<ImageInfo>> GetByOrders(int startOrder, int endOrder)
        {
            lock (lockObject)
            {
                return Task.FromResult(allData.Where(x => x.Order >= startOrder && x.Order <= endOrder));
            }
        }

        public override async Task<int> Update(int id, ImageInfo entity)
        {
            if (id <= 0)
                return await Insert(entity);  // спорно

            if (string.IsNullOrEmpty(entity.ImageRef))
                throw new ArgumentException("Image ref is not declared");

            entity.Id = id;
            entity.ImageRef = await CheckAndCacheImage(entity);
            entity.SourceType = ImageSourceType.FilePath;

            lock (lockObject)
            {
                allData = allData.Where(x => x.Id != id).ToList();
                allData.Add(entity);
            }

            SaveChangesRequest();

            return id;
        }

        public override async Task<int> Insert(ImageInfo entity)
        {
            if (string.IsNullOrEmpty(entity.ImageRef))
                throw new ArgumentException("Image ref is not declared");

            entity.ImageRef = await CheckAndCacheImage(entity);
            entity.SourceType = ImageSourceType.FilePath;
            entity.Id = GetNewId();

            lock (lockObject)
            {
                allData.Add(entity);
            }

            SaveChangesRequest();

            return entity.Id;
        }

        public override Task<bool> Delete(int id)
        {
            ImageInfo image = allData.FirstOrDefault(x => x.Id == id);
            lock (lockObject)
            {
                allData = allData.Where(x => x.Id != id).ToList();
            }

            if (image != null && image.SourceType == ImageSourceType.FilePath)
            {
                DeleteImageFile(image.ImageRef);
            }

            SaveChangesRequest();
            return Task.FromResult(image != null);
        }

        private void DeleteImageFile(string file)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch (Exception ex)
            {
                // TODO: log
            }
        }

        /// <summary>
        /// Проверяем источник фото. Если он не в папке приложения - поместим копию изображения туда и изменим ссылку.
        /// </summary>
        /// <param name="entity"></param>
        private async Task<string> CheckAndCacheImage(ImageInfo entity)
        {
            Settings settings = settingsRepository.GetByUserId(0);  // TODO: пока только 1 пользователь
            bool isLocal = entity.ImageRef.StartsWith(basePath);

            if (isLocal)
                return entity.ImageRef;

            if (entity.SourceType == ImageSourceType.FilePath)
            {
                // скопировать файл изображения
                string extension = Path.GetExtension(entity.ImageRef);
                string fileName = Path.ChangeExtension(Path.GetRandomFileName(), extension);    // меняем имя на случай повторного добавления
                ImageFormat format = ParseImageFormat(extension);
                string filePath = Path.Combine(basePath, fileName);

                if (settings.ResizeImages)
                {
                    byte[] bytes = File.ReadAllBytes(entity.ImageRef);
                    byte[] resizedBytes = await imageResizer.ResizeImage(bytes, settings.ImageResizeWidth, settings.ImageResizeHeight, format, settings.ImageCompressQuality);
                    File.WriteAllBytes(filePath, resizedBytes);
                }
                else
                {
                    File.Copy(entity.ImageRef, filePath);
                }

                return filePath;
            }
            else if (entity.SourceType == ImageSourceType.Url)
            {
                // скачать и сохранить изображение
                string extension = GetExtensionFromUri(entity.ImageRef);
                string fileName = Path.ChangeExtension(Path.GetRandomFileName(), extension);
                ImageFormat format = ParseImageFormat(extension);
                string filePath = Path.Combine(basePath, fileName);

                using (WebClient webClient = new WebClient())
                {
                    try
                    {
                        Uri uri = new Uri(entity.ImageRef);
                        byte[] data = await webClient.DownloadDataTaskAsync(uri);

                        if (settings.ResizeImages)
                        {
                            byte[] resizedBytes = await imageResizer.ResizeImage(data, settings.ImageResizeWidth, settings.ImageResizeHeight, format, settings.ImageCompressQuality);
                            File.WriteAllBytes(filePath, resizedBytes);
                        }
                        else
                        {
                            File.WriteAllBytes(filePath, data);
                        }
                    }
                    catch (Exception ex)
                    {
                        // TODO: log
                    }
                }

                return filePath;
            }

            return entity.ImageRef;
        }

        public async Task RotateImage(ImageInfo imageInfo, float degrees)
        {
            bool isLocal = imageInfo.ImageRef.StartsWith(basePath);

            if (!isLocal)
                throw new NotSupportedException();

            string extension = Path.GetExtension(imageInfo.ImageRef);
            ImageFormat format = ParseImageFormat(extension);

            // lock ?
            byte[] bytes = File.ReadAllBytes(imageInfo.ImageRef);
            byte[] rotated = await imageRotator.Rotate(bytes, format, degrees);
            File.WriteAllBytes(imageInfo.ImageRef, rotated);
        }

        private ImageFormat ParseImageFormat(string extension)
        {
            switch (extension.ToLower().Trim('.', ' '))
            {
                case "jpg":
                case "jpeg":
                    return ImageFormat.JPEG;
                case "png":
                    return ImageFormat.PNG;

                default:
                    return ImageFormat.JPEG;
            }
        }

        private string GetExtensionFromUri(string imageRef)
        {
            if (imageRef.ToLower().EndsWith("jpg") || imageRef.ToLower().EndsWith("jpeg"))
                return "jpg";
            else if (imageRef.ToLower().EndsWith("png"))
                return "png";
            else
                throw new ArgumentException("Extension of picture is not supported");
        }
    }
}
