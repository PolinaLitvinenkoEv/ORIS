using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniHttpServer.ORM;

namespace Tests
{
    [TestClass]
    public class OrmTests
    {
        [TestMethod]
        public void EscapeIdentifier_ValidName_ReturnsQuotedName()
        {
            string tableName = "Users";
            var orm = new OrmContext("Host=localhost;Database=Oris;Username=postgres;Password=123");
            string dangerousName = "Users; DROP TABLE Users;--";

            var ex = Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                await orm.GetAllAsync<object>(dangerousName)
            ).GetAwaiter().GetResult();

            Assert.IsTrue(ex.Message.Contains("Недопустимое имя SQL-идентификатора"));
        }

    }
}
