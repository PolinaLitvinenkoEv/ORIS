using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework.Results;

namespace Tests
{
    [TestClass]
    public class ResultsTests
    {
        [TestMethod]
        public void JsonResult_SerializesToCamelCase()
        {

            var data = new { FirstName = "Alex", Age = 20 };
            var jsonResult = new JsonResult(data);

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            string json = JsonSerializer.Serialize(data, options);

            Assert.AreEqual("{\"firstName\":\"Alex\",\"age\":20}", json);
        }
    }
}
