using Newtonsoft.Json;
using PublicationPlanning.StoredModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PublicationPlanning.Repositories
{
    public abstract class BaseFileRepository<T> : IRepository<T> where T : IStoredEntity
    {
        protected readonly string basePath;
        protected readonly string storageFileNamePattern;
        protected readonly string storageFileNameFormat;

        protected List<T> allData = new List<T>();    // TODO: HashTable?
        protected object lockObject = new object();

        private const int saveDebounceSec = 1;
        private int saveRequestCount = 0;


        public BaseFileRepository()
        {
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            storageFileNamePattern = $"{typeof(T).Name}-*.txt";
            storageFileNameFormat = typeof(T).Name + "-{0:yyyy-MM-dd-HH-mm-ss}.txt";

            LoadAllData();
        }

        protected virtual List<T> ConvertToEntityList(string stringData)
        {
            if (string.IsNullOrEmpty(stringData))
                return new List<T>();

            return JsonConvert.DeserializeObject<List<T>>(stringData);
        }

        protected virtual string ConvertToText(List<T> entityList)
        {
            if (entityList == null || !entityList.Any())
                return "";

            return JsonConvert.SerializeObject(entityList, new EntityJsonConverter());
        }

        private void LoadAllData()
        {
            try
            {
                string[] storageFiles = Directory.GetFiles(basePath, storageFileNamePattern);

                if (!storageFiles.Any())
                {
                    allData = new List<T>();
                    return;
                }

                string lastFile = storageFiles.Max();
                string stringData;
                lock (lockObject)
                {
                    stringData = File.ReadAllText(lastFile);
                }

                allData = ConvertToEntityList(stringData);
            }
            catch (Exception ex)
            {
                throw new FileLoadException("Can't to load storage file", ex);
                // TODO: log
            }
        }

        protected void SaveChangesRequest()
        {
            Interlocked.Increment(ref saveRequestCount);

            Task.Run(() => RealSaveChanges());
        }

        private async Task RealSaveChanges()
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
                    string stringData = ConvertToText(allData);
                    File.WriteAllText(fileName, stringData);
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

        protected int GetNewId()
        {
            if (!allData.Any())
                return 1;

            return allData.Max(x => x.Id) + 1;
        }


        public virtual Task<T> Get(int id)
        {
            lock (lockObject)
            {
                return Task.FromResult(allData.FirstOrDefault(x => x.Id == id));
            }
        }

        public virtual Task<IEnumerable<T>> GetPage(int page, int limit)
        {
            lock (lockObject)
            {
                return Task.FromResult(allData.OrderBy(x => x.DefaultOrder())
                    .Skip(page * limit)
                    .Take(limit));
            }
        }

        public virtual Task<int> Insert(T entity)
        {
            entity.Id = GetNewId();

            lock (lockObject)
            {
                allData.Add(entity);
            }

            SaveChangesRequest();

           return Task.FromResult(entity.Id);
        }

        public virtual async Task<int> Update(int id, T entity)
        {
            if (id <= 0)
                return await Insert(entity);  // спорно

            entity.Id = id;

            lock (lockObject)
            {
                allData = allData.Where(x => x.Id != id).ToList();
                allData.Add(entity);
            }

            SaveChangesRequest();

            return id;
        }

        public virtual Task<bool> Delete(int id)
        {
            T entity = allData.FirstOrDefault(x => x.Id == id);
            lock (lockObject)
            {
                allData = allData.Where(x => x.Id != id).ToList();
            }

            SaveChangesRequest();

            return Task.FromResult(entity != null);
        }

    }
}
