using PublicationPlanning.StoredModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PublicationPlanning.Repositories
{
    public interface IImageInfoRepository : IRepository<ImageInfo>
    {
        Task<IEnumerable<ImageInfo>> GetByOrders(int startOrder, int endOrder);
    }

    public class ImageInfoFileRepository : IImageInfoRepository
    {
        private const string storageFileNamePattern = "ImageInfo-*.txt";
        private const string storageFileNameFormat = "ImageInfo-{0:yyyy-MM-dd-HH-mm-ss}.txt";
        private const string fieldSeparator = "\t";
        private const int fieldsCount = 4;
        private readonly string basePath;
        private const int saveDebounceSec = 1;
        private int saveRequestCount = 0;
        private List<ImageInfo> allData = new List<ImageInfo>();    // TODO: HashTable?
        private object lockObject = new object();

        public ImageInfoFileRepository()
        {
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            LoadAllData();

            // TODO: метод для проверки кэша картинок и удаления ненужных, удаления не новейших версий файла
        }

        private async Task LoadAllData()
        {
            try
            {
                string[] storageFiles = Directory.GetFiles(basePath, storageFileNamePattern);

                if (!storageFiles.Any())
                {
                    //CreateStorageFile();
                    allData = new List<ImageInfo>();
                    return;
                }

                string lastFile = storageFiles.Max();
                string[] lines;
                lock (lockObject)
                {
                    lines = File.ReadAllLines(lastFile);
                }

                allData = new List<ImageInfo>();
                foreach (var line in lines)
                {
                    allData.Add(ConvertToEntity(line));
                }
            }
            catch (Exception ex)
            {
                throw new FileLoadException("Can't to load storage file", ex);
                // TODO: log
            }
        }

        private ImageInfo ConvertToEntity(string line)
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

            return new ImageInfo()
            {
                Id = id,
                Order = order,
                SourceType = sourceType,
                ImageRef = imageRef,
            };
        }

        private string ConvertToText(ImageInfo entity)
        {
            if (entity.Id <= 0)
                throw new ArgumentException("Entity must have a positive id");

            if (string.IsNullOrEmpty(entity.ImageRef))
                throw new ArgumentException("Image ref is not declared");

            string[] fields = new string[4]
            { 
                entity.Id.ToString(),
                entity.Order.ToString(),
                ((int)entity.SourceType).ToString(),
                entity.ImageRef
            };

            return string.Join(fieldSeparator, fields);
        }

        private void SaveFileRequest()
        {
            Interlocked.Increment(ref saveRequestCount);

            Task.Run(() => RealFileSave());
        }

        private async Task RealFileSave()
        {
            await Task.Delay(saveDebounceSec * 1000);

            Interlocked.Decrement(ref saveRequestCount);

            if (saveRequestCount == 0)
                UpgradeStorage();
        }

        private void UpgradeStorage()
        {
            SaveStorageFile();
            DeleteOldStorageFiles();
        }

        private void SaveStorageFile()
        {
            try
            {
                lock (lockObject)
                {
                    string fileName = Path.Combine(basePath, string.Format(storageFileNameFormat, DateTime.Now));
                    List<string> lines = allData.Select(x => ConvertToText(x)).ToList();
                    File.WriteAllLines(fileName, lines);
                }
            }
            catch (Exception ex)
            {
                throw new FileNotFoundException("Can't to save data", ex);
                // TODO: log
            }
        }

        private void DeleteOldStorageFiles()
        {
            try
            {
                string[] storageFiles = Directory.GetFiles(basePath, storageFileNamePattern);

                if (storageFiles.Length <= 1)
                    return;

                foreach (var file in storageFiles.OrderByDescending(x => x).Skip(1))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                // TODO: log
            }
        }

        #region IRepository

        public Task<IEnumerable<ImageInfo>> GetPage(int page, int limit)
        {
            lock (lockObject)
            {
                return Task.FromResult(allData.OrderBy(x => x.DefaultOrder())
                    .Skip(page * limit)
                    .Take(limit));
            }
        }

        public Task<ImageInfo> Get(int id)
        {
            lock (lockObject)
            {
                return Task.FromResult(allData.FirstOrDefault(x => x.Id == id));
            }
        }

        public Task<IEnumerable<ImageInfo>> GetByOrders(int startOrder, int endOrder)
        {
            lock (lockObject)
            {
                return Task.FromResult(allData.Where(x => x.Order >= startOrder && x.Order <= endOrder));
            }
        }

        public async Task<int> Update(int id, ImageInfo entity)
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

            SaveFileRequest();

            return id;
        }

        public async Task<int> Insert(ImageInfo entity)
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

            SaveFileRequest();

            return entity.Id;
        }

        public async Task<bool> Delete(int id)
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

            SaveFileRequest();
            return image != null;
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

        #endregion IRepository

        private int GetNewId()
        {
            if (!allData.Any())
                return 1;

            return allData.Max(x => x.Id) + 1;
        }

        /// <summary>
        /// Проверяем источник фото. Если он не в папке приложения - поместим копию изображения туда и изменим ссылку.
        /// </summary>
        /// <param name="entity"></param>
        private async Task<string> CheckAndCacheImage(ImageInfo entity)
        {
            bool isLocal = entity.ImageRef.StartsWith(basePath);

            if (isLocal)
                return entity.ImageRef;

            if (entity.SourceType == ImageSourceType.FilePath)
            {
                // скопировать файл изображения
                string extension = Path.GetExtension(entity.ImageRef);
                string fileName = Path.ChangeExtension(Path.GetRandomFileName(), extension);    // меняем имя на случай повторного добавления
                string filePath = Path.Combine(basePath, fileName);
                File.Copy(entity.ImageRef, filePath);

                return filePath;
            }
            else if (entity.SourceType == ImageSourceType.Url)
            {
                // скачать и сохранить изображение
                string extension = GetExtensionFromUri(entity.ImageRef);
                string fileName = Path.ChangeExtension(Path.GetRandomFileName(), extension);
                string filePath = Path.Combine(basePath, fileName);

                using (WebClient webClient = new WebClient())
                {
                    try
                    {
                        Uri uri = new Uri(entity.ImageRef);
                        byte[] data = await webClient.DownloadDataTaskAsync(uri);

                        File.WriteAllBytes(filePath, data);
                    }
                    catch (Exception ex)
                    {
                        // TODO: log
                    }
                }

                return filePath;
            }

            // TODO: подрезать/сжать изображение

            return entity.ImageRef;
        }

        private string GetExtensionFromUri(string imageRef)
        {
            if (imageRef.ToLower().EndsWith("jpg") || imageRef.ToLower().EndsWith("jpeg"))
                return "jpg";
            else if (imageRef.ToLower().EndsWith("png"))
                return "png";
            else if (imageRef.ToLower().EndsWith("gif"))
                return "gif";
            else
                throw new ArgumentException("Extension of picture is not supported");
        }
    }
}
