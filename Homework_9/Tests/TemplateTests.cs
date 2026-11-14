using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniTemplateEngine;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class TemplateTests
    {
        private HtmlTemplateRenderer renderer = null!;

        [TestInitialize]
        public void Setup()
        {
            renderer = new HtmlTemplateRenderer();
        }
        [TestMethod]
        public void Test_Book_Replacement()
        {
            var book = new Book("Солярис", "1961", "Станислав Лем");
            string template = "<p>Книга: ${book.Name}</p><p>Год: ${book.Year}</p><p>Автор: ${book.Author}</p>";
            string result = renderer.RenderFromString(template, book);
            Assert.AreEqual("<p>Книга: Солярис</p><p>Год: 1961</p><p>Автор: Станислав Лем</p>", result);
        }

        [TestMethod]
        public void Test_Bag_Nested_Objects()
        {
            var bag = new Bag(
                "Рюкзак",
                new Size(30, 40),
                new Color("Красный"),
                new Material("Кожа")
            );
            string template = "${bag.Name} размером ${bag.Size.Width}x${bag.Size.Height}, цвет ${bag.Color.Name}, материал ${bag.Material.Name}";
            string result = renderer.RenderFromString(template, bag);
            Assert.AreEqual("Рюкзак размером 30x40, цвет Красный, материал Кожа", result);
        }

        [TestMethod]
        public void Test_If_Condition()
        {
            var book1 = new Book("Солярис", "1961", "Лем");
            var book2 = new Book("Гарри Поттер", "", "Роулинг");

            string template = "$if(book.Year)Год известен: ${book.Year}$elseГод неизвестен$endif";
            string result1 = renderer.RenderFromString(template, book1);
            string result2 = renderer.RenderFromString(template, book2);
            Assert.AreEqual("Год известен: 1961", result1);
            Assert.AreEqual("Год неизвестен", result2);
        }

        [TestMethod]
        public void Test_Foreach_Loop()
        { 
            var books = new List<Book>
            {
                new Book("Солярис", "1961", "Лем"),
                new Book("Гарри Поттер", "1997", "Роулинг")
            };

            var model = new { books };
            string template = "<ul>$foreach(var b in books)<li>${b.Name}</li>$endforeach</ul>";
            string result = renderer.RenderFromString(template, model);
            Assert.AreEqual("<ul><li>Солярис</li><li>Гарри Поттер</li></ul>", result);
        }

        [TestMethod]
        public void Test_Nested_If_Inside_Foreach()
        {
            var books = new List<Book>
            {
                new Book("Солярис", "1961", "Лем"),
                new Book("Без года", "", "Неизвестен")
            };
            var model = new { books };
            string template = "$foreach(var b in books)$if(b.Year)[${b.Name}: ${b.Year}]$else[${b.Name}]$endif$endforeach";
            string result = renderer.RenderFromString(template, model);
            Assert.AreEqual("[Солярис: 1961][Без года]", result);
        }
    }
}

