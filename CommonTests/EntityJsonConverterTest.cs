using System;
using System.Linq;
using Newtonsoft.Json;
using PublicationPlanning.Repositories;
using PublicationPlanning.StoredModels;
using Xunit;

namespace CommonTests
{
    public class EntityJsonConverterTest
    {
        [Fact]
        public void ConvertEntity_Scalar()
        {
            IStoredEntity entity = new Feed()
            {
                Id = 1,
                Name = "Name"
            };
            string actual = JsonConvert.SerializeObject(entity, new EntityJsonConverter());
            string expected = "{\"Id\":1,\"Name\":\"Name\",\"Owner\":null,\"Settings\":null}";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ConvertEntity_References()
        {
            User user = new User()
            {
                Id = 1,
                ActiveFeedId = 2
            };

            Feed entity = new Feed()
            {
                Id = 2,
                Name = "Name",
                Owner = user,
            };

            string actual = JsonConvert.SerializeObject(entity, new EntityJsonConverter());
            string expected = "{\"Id\":2,\"Name\":\"Name\",\"Owner\":{\"Id\":1},\"Settings\":null}";

            Assert.Equal(expected, actual);
        }
    }
}
