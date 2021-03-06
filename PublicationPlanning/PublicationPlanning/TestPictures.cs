using PublicationPlanning.Services;
using PublicationPlanning.StoredModels;
using PublicationPlanning.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace PublicationPlanning
{
    public class TestPictures
    {
        [DataContract]
        class ImageList
        {
            [DataMember(Name = "photos")]
            public List<string> Photos = null;
        }

        private readonly IImageInfoService service;

        public TestPictures(IImageInfoService service)
        {
            this.service = service;
        }

        public async void LoadTestDataToStorage()
        {
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    Uri uri = new Uri("https://raw.githubusercontent.com/xamarin/docs-archive/master/Images/stock/small/stock.json");
                    byte[] data = await webClient.DownloadDataTaskAsync(uri);

                    using (Stream stream = new MemoryStream(data))
                    {
                        var jsonSerializer = new DataContractJsonSerializer(typeof(ImageList));
                        ImageList imageList = (ImageList)jsonSerializer.ReadObject(stream);

                        foreach (string filepath in imageList.Photos.Take(5))
                        {
                            await service.InsertFirst(new ImageInfoViewModel()
                            {
                                ImageRef = filepath,
                                SourceType = ImageSourceType.Url
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Cannot access list of bitmap files", ex);
                }
            }
        }

    }
}
