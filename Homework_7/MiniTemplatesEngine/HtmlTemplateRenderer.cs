using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiniTemplateEngine
{
    public class HtmlTemplateRenderer: IHtmlTemplateRenderer
    {
        public string RenderFromString(string htmlTemplate, object dataModel)
        {
            if (htmlTemplate == null) return string.Empty;                   // Если шаблон null — возвращаем пустую строку
            var locals = new Dictionary<string, object>();                   // Создаём словарь локальных переменных (для циклов)
            return Render(htmlTemplate, dataModel, locals);                   // Запускаем рекурсивный рендер
        }

        /// Рендерит шаблон из файла.
        public string RenderFromFile(string filePath, object dataModel)
        {
            string template = File.ReadAllText(filePath, Encoding.UTF8);     // Читаем файл целиком
            return RenderFromString(template, dataModel);                    // Передаём в RenderFromString
        }

        /// Рендерит шаблон и сохраняет результат в файл.
        public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
        {
            string result = RenderFromFile(inputFilePath, dataModel);        // Получаем результат рендера
            File.WriteAllText(outputFilePath, result, Encoding.UTF8);        // Сохраняем в выходной файл
            return result;                                                   // Возвращаем результат
        }

        //  Основной рекурсивный метод, который обрабатывает все конструкции
        private string Render(string template, object model, Dictionary<string, object> locals)
        {
            if (string.IsNullOrEmpty(template)) return template;             // Если шаблон пуст — возвращаем его

            template = ProcessForeach(template, model, locals);              // 1. Сначала обрабатываем циклы $foreach
            template = ProcessIf(template, model, locals);                   // 2. Потом условия $if / $else / $endif
            template = ProcessVariables(template, model, locals);            // 3. В конце — заменяем переменные ${...}

            return template;                                                  // Возвращаем обработанный шаблон
        }

        //  Обработка циклов $foreach и $endforeach
        private string ProcessForeach(string template, object model, Dictionary<string, object> locals)
        {
            // Регулярка ищет: $foreach(var x in Collection) ... $endforeach
            Regex regex = new Regex(
                @"\$foreach\s*\(\s*var\s+(?<var>\w+)\s+in\s+(?<collection>[^\s\)]+)\s*\)(?<body>.*?)\$endforeach",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            while (regex.IsMatch(template))                                   // Пока в шаблоне есть $foreach
            {
                template = regex.Replace(template, match =>                   // Заменяем первое вхождение
                {
                    string varName = match.Groups["var"].Value;               // Имя переменной цикла 
                    string collPath = match.Groups["collection"].Value;       // Путь к коллекции
                    string body = match.Groups["body"].Value;                 // Тело цикла

                    object collection = ResolvePath(collPath, model, locals); // Получаем коллекцию по пути
                    if (collection is not IEnumerable enumerable || collection is string)
                        return "";                                            // Если это не коллекция — пустота

                    StringBuilder sb = new StringBuilder();                   // Буфер для результата
                    foreach (var item in enumerable)                          // Перебираем элементы
                    {
                        var childLocals = new Dictionary<string, object>(locals); // Копируем локальные переменные
                        childLocals[varName] = item;                           // Добавляем переменную цикла (i = item)
                        sb.Append(Render(body, model, childLocals));           // Рендерим тело с этой переменной
                    }
                    return sb.ToString();                                     // Возвращаем весь цикл
                }, 1);                                                        // Обрабатываем 1 вхождение за раз
            }
            return template;                                                  // Возвращаем результат
        }

        //  Обработка условий 
        private string ProcessIf(string template, object model, Dictionary<string, object> locals)
        {
            // Регулярка ищет: $if(cond), [$else],$endif
            Regex regex = new Regex(
                @"\$if\s*\(\s*(?<cond>[^\)]+)\s*\)(?<then>.*?)(?:\$else(?<else>.*?))?\$endif",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            while (regex.IsMatch(template))                                   // Пока есть $if
            {
                template = regex.Replace(template, match =>                   // Заменяем первое вхождение
                {
                    string condPath = match.Groups["cond"].Value.Trim();      // Условие 
                    string thenBlock = match.Groups["then"].Value;            // Блок «истина»
                    string elseBlock = match.Groups["else"].Success           // Блок «ложь» (если есть)
                        ? match.Groups["else"].Value : "";

                    object value = ResolvePath(condPath, model, locals);      // Получаем значение по пути
                    return IsTruthy(value)                                    // Если истина —
                        ? Render(thenBlock, model, locals)                    // рендерим then-блок
                        : Render(elseBlock, model, locals);                   // иначе рендерим else-блок
                }, 1);
            }
            return template;                                                  // Возвращаем результат
        }

        //  Замена ${path.to.value}
        private string ProcessVariables(string template, object model, Dictionary<string, object> locals)
        {
            Regex regex = new Regex(@"\$\{(?<path>[^}]+)\}");                 // Регулярка ищет ${...}
            return regex.Replace(template, match =>                           // Заменяем все вхождения
            {
                string path = match.Groups["path"].Value.Trim();              // Извлекаем путь
                object value = ResolvePath(path, model, locals);              // Ищем значение
                return value?.ToString() ?? "";                               // Возвращаем строку или пусто
            });
        }

        //  Получение значения через рефлексию
        private object ResolvePath(string path, object model, Dictionary<string, object> locals)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;                 // Пустой путь — возвращаем null

            // Разбиваем строку по точкам: "bag.Color.Name" -> ["bag", "Color", "Name"]
            string[] parts = path.Split('.');

            object current = null;                                            // Текущий объект, по которому идём

            // 1. Обрабатываем первую часть пути (корень)
            if (locals.TryGetValue(parts[0], out var local))                  // Если первая часть — переменная цикла
            {
                current = local;                                              // Берём её значение
            }
            else
            {
                // Пытаемся найти свойство внутри модели 
                current = GetMember(model, parts[0]);

                // Если не нашли, проверяем: может, пользователь написал имя самого объекта в шаблоне?
                // Например, передали `User`, а в шаблоне пишут `${user.Name}`.
                if (current == null && model != null)
                {
                    string modelTypeName = model.GetType().Name;
                    string lowerModelName = char.ToLower(modelTypeName[0]) + modelTypeName.Substring(1);

                    if (parts[0].Equals(lowerModelName, StringComparison.OrdinalIgnoreCase))
                    {
                        current = model; // Если имя совпало — текущим объектом становится сама модель!
                    }
                }
            }

            // 2. Идём по остальным частям пути
            for (int i = 1; i < parts.Length; i++)
            {
                if (current == null) return null;                             // Если наткнулись на null — дальше идти нельзя
                current = GetMember(current, parts[i]);                       // Достаём следующее вложенное свойство
            }

            return current;                                                   // Возвращаем конечное значение
        }


        //  Получение свойства или поля через рефлексию
        private object GetMember(object obj, string name)
        {
            if (obj == null) return null;                                     // null — null
            Type t = obj.GetType();                                           // Получаем тип

            PropertyInfo prop = t.GetProperty(name,                           // Ищем свойство
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (prop != null) return prop.GetValue(obj);                      // Если нашли — возвращаем значение

            FieldInfo field = t.GetField(name,                                // Иначе ищем поле
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.IgnoreCase);

            return field?.GetValue(obj);                                      // Возвращаем значение поля или null
        }
        //  if
        private bool IsTruthy(object value)
        {
            if (value == null) return false;                                  // null — ложь
            if (value is bool b) return b;                                    // bool — как есть
            if (value is string s) return !string.IsNullOrEmpty(s);           // строка — не пустая
            if (value is int i) return i != 0;                                // int — не ноль
            if (value is IEnumerable e)                                       // коллекция —
            {
                var en = e.GetEnumerator();                                   // берём итератор
                return en.MoveNext();                                         // есть ли элементы
            }
            return true;                                                      // Иначе — истина
        }
    }
}
    

