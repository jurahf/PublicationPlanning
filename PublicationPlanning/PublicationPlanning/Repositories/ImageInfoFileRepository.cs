using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PublicationPlanning.ImageResizer;
using PublicationPlanning.StoredModels;

namespace PublicationPlanning.Repositories
{
    public interface IImageInfoRepository : IRepository<ImageInfo>
    {
        Task<IEnumerable<ImageInfo>> GetByOrders(int startOrder, int endOrder);
    }

    public class ImageInfoFileRepository : BaseFileRepository<ImageInfo>, IImageInfoRepository
    {
        private const string fieldSeparator = "\t";
        private const int fieldsCount = 4;        
        private readonly IImageResizer imageResizer;
        private readonly ISettingsRepository settingsRepository;

        public ImageInfoFileRepository(IImageResizer imageResizer, ISettingsRepository settingsRepository)
            : base()
        {
            this.imageResizer = imageResizer;
            this.settingsRepository = settingsRepository;
            

            // TODO: метод для проверки кэша картинок и удаления ненужных, удаления не новейших версий файла
        }

        protected override List<ImageInfo> ConvertToEntityList(string stringData)
        {
            if (string.IsNullOrEmpty(stringData))
                return new List<ImageInfo>();

            if (stringData.StartsWith("["))
            {
                return base.ConvertToEntityList(stringData);
            }
            else
            {
                List<ImageInfo> result = new List<ImageInfo>();
                string[] lines = stringData.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    string[] fields = line.Split(new string[] { fieldSeparator }, StringSplitOptions.None);

                    if (fields.Length != fieldsCount)
                        throw new ArgumentException("Unexpected data length");

                    if (!int.TryParse(fields[0], out int id))
                        throw new ArgumentException($"File position 0 (id): expected a number");

                    if (id <= 0)
                        throw new ArgumentException("Id must be a positive number");

                    if (!int.TryParse(fields[1], out int order))
                        throw new ArgumentException("File position 1 (order): expected a number");

                    if (!int.TryParse(fields[2], out int sourceTypeInt))
                        throw new ArgumentException("File position 2 (source type): expected a number");

                    ImageSourceType sourceType = (ImageSourceType)sourceTypeInt;

                    string imageRef = fields[3];

                    if (string.IsNullOrEmpty(imageRef))
                        throw new ArgumentException("Image ref is not declared");

                    result.Add(new ImageInfo()
                    {
                        Id = id,
                        Order = order,
                        SourceType = sourceType,
                        ImageRef = imageRef,
                    });
                }

                return result;
            }
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
