using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MiniMax.Attributes;
using MyLibrary.Collections;
using MyLibrary.Trace;
using MyMath;

namespace MiniMax
{
    /// <summary>
    ///     Класс для обработки массивов статистических данных
    ///     тип данных - decimal
    /// </summary>
    public class Builder : ITrace
    {
        public Builder(Type type)
        {
            Type = type;
        }

        private Type Type { get; set; }
        public ProgressCallback ProgressCallback { get; set; }
        public AppendLineCallback AppendLineCallback { get; set; }
        public CompliteCallback CompliteCallback { get; set; }

        public void SaveAs(string fileName, IEnumerable<object> dataSource)
        {
            PropertyInfo[] props = Type.GetProperties();
            using (StreamWriter writer = File.CreateText(fileName))
            {
                writer.WriteLine(string.Join(";", props.Select(p => p.Name)));
                foreach (object data in dataSource)
                {
                    writer.WriteLine(string.Join(";", props.Select(p => p.GetValue(data, null))));
                }
            }
        }

        public void LoadFrom(string fileName, ref IEnumerable<object> dataSource)
        {
            PropertyInfo[] props = Type.GetProperties();
            var list = new StackListQueue<object>(dataSource);
            using (StreamReader reader = File.OpenText(fileName))
            {
                string line = reader.ReadLine();
                string[] split = line.Split(';');
                var dictionary = new Dictionary<int, PropertyInfo>();
                int index = 0;
                foreach (string name in split)
                {
                    PropertyInfo prop = props.FirstOrDefault(p => string.CompareOrdinal(p.Name, name) == 0);
                    if (prop != null) dictionary.Add(index, prop);
                    index++;
                }
                for (line = reader.ReadLine(); !string.IsNullOrEmpty(line); line = reader.ReadLine())
                {
                    split = line.Split(';');
                    object x = Activator.CreateInstance(Type);
                    foreach (var pair in dictionary)
                        pair.Value.SetValue(x, Convert.ChangeType(split[pair.Key], pair.Value.PropertyType));
                    list.Add(x);
                }
                dataSource = list;
            }
        }

        /// <summary>
        ///     Нахождение линейной зависимости величин от параметров
        ///     y = a * x + b
        ///     ey = a * ex + b
        ///     s2y = ey^2 - e2y = a^2 * s2x
        /// </summary>
        /// <param name="dataSource"></param>
        public static void DetectLinearDependencies(IEnumerable<object> dataSource, PropertyInfo variable,
            PropertyInfo value, ref decimal a, ref decimal b)
        {
            IEnumerable<decimal> y = dataSource.Select(obj => Convert.ToDecimal(value.GetValue(obj, null)));
            IEnumerable<decimal> x = dataSource.Select(obj => Convert.ToDecimal(value.GetValue(obj, null)));
            decimal ex = x.Average(v => v);
            decimal ey = y.Average(v => v);
            decimal s2x = x.Average(v => v*v) - ex*ex;
            decimal s2y = y.Average(v => v*v) - ey*ey;
            // ey = a * ex + b
            // s2y = a^2 * s2x
            // a = sqrt( s2y / s2x )
            // b = ey - a * ex
            a = (decimal) Math.Sqrt((double) s2y/(double) s2x);
            b = ey - a*ex;
        }

        public static void DetectLinearDependencies(IEnumerable<object> dataSource,
            IEnumerable<PropertyInfo> variables,
            IEnumerable<PropertyInfo> values, ref Matrix<decimal> matrix, ref Vector<decimal> vector)
        {
            int rows = values.Count();
            int columns = variables.Count();
            Debug.Assert(vector.Count == rows);
            Debug.Assert(matrix.Rows == rows);
            Debug.Assert(matrix.Columns == columns);
            IEnumerable<decimal> ey =
                values.Select(
                    value => dataSource.Select(obj => Convert.ToDecimal(value.GetValue(obj, null))).Average(v => v));
            IEnumerable<decimal> ex =
                variables.Select(
                    value => dataSource.Select(obj => Convert.ToDecimal(value.GetValue(obj, null))).Average(v => v));
            IEnumerable<decimal> s2y =
                values.Select(
                    (value, index) =>
                        dataSource.Select(obj => Convert.ToDecimal(value.GetValue(obj, null))).Average(v => v*v) -
                        ey.ElementAt(index)*ey.ElementAt(index));
            IEnumerable<decimal> s2x =
                variables.Select(
                    (value, index) =>
                        dataSource.Select(obj => Convert.ToDecimal(value.GetValue(obj, null))).Average(v => v*v) -
                        ex.ElementAt(index)*ex.ElementAt(index));
            foreach (
                var pair in
                    from r in Enumerable.Range(0, rows)
                    from c in Enumerable.Range(0, columns)
                    select new {row = r, column = c})
            {
                int row = pair.row;
                int column = pair.column;
                matrix[row][column] = (decimal) Math.Sqrt((double) s2y.ElementAt(row)/(double) s2x.ElementAt(column));
            }
            // y = ai * xi + b
            // b = ey - ai * exi
            foreach (int row in from r in Enumerable.Range(0, rows) select r)
            {
                vector[row] = ey.ElementAt(row);
            }
            foreach (
                var pair in
                    from r in Enumerable.Range(0, rows)
                    from c in Enumerable.Range(0, columns)
                    select new {row = r, column = c})
            {
                int row = pair.row;
                int column = pair.column;
                vector[row] -= matrix[row][column]*ex.ElementAt(column);
            }
        }

        /// <summary>
        ///     Построение таблицы наилучших значений
        ///     Перебираем все значения переменныж и опций
        ///     Удаляем недопустимые величины
        ///     Группируем по опциям
        ///     Оставляем опции только с наилучшим значением
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="type"></param>
        public void BuildOptionTable(ref IEnumerable<object> dataSource, object constant)
        {
            var formula = new Formula(Type);
            var list = new StackListQueue<object>(constant);
            IEnumerable<PropertyInfo> variables = formula.GetProperties(typeof (MiniMaxVariableAttribute));
            IEnumerable<PropertyInfo> options = formula.GetProperties(typeof (MiniMaxOptionAttribute));
            IEnumerable<PropertyInfo> constants = formula.GetProperties(typeof (MiniMaxConstantAttribute));
            IEnumerable<PropertyInfo> targets = formula.GetProperties(typeof (MiniMaxTargetAttribute));
            IEnumerable<PropertyInfo> union = variables.Union(options);
            IEnumerable<PropertyInfo> union1 = options.Union(constants);
            long current = 0;
            long total = 0;
            int index = 0;
            foreach (PropertyInfo prop in union)
            {
                var values = new StackListQueue<object>();
                IEnumerable<Attribute> customs = prop.GetCustomAttributes(typeof (MiniMaxVariableAttribute))
                    .Union(prop.GetCustomAttributes(typeof (MiniMaxOptionAttribute)));
                foreach (Attribute custom in customs)
                {
                    Type type = custom.GetType();
                    PropertyInfo valueProp = type.GetProperty("Values");
                    values.AddRangeExcept((object[]) valueProp.GetValue(custom, null));
                }
                if (ProgressCallback != null) ProgressCallback(current, total += list.Count()*values.Count());
                var next = new StackListQueue<object>();
                foreach (object item in list)
                {
                    foreach (object value in values)
                    {
                        object x = Activator.CreateInstance(Type);
                        // Копируем значения констант
                        foreach (PropertyInfo constProp in constants)
                            constProp.SetValue(x, constProp.GetValue(constant, null));
                        // Копируем предыдущие значения
                        for (int i = 0; i < index; i++)
                            union.ElementAt(i).SetValue(x, union.ElementAt(i).GetValue(item, null));
                        // Добавляет перебираемое значение
                        prop.SetValue(x, Convert.ChangeType(value, prop.PropertyType));
                        next.Add(x);
                        if (ProgressCallback != null) ProgressCallback(++current, total);
                    }
                }
                list = next;
                index++;
            }
            list.RemoveAll(formula.IsInvalid);
            // Подсчитываем сумму целевых функций
            Dictionary<object, decimal> f = list.ToDictionary(i => i, i => 0m);
            foreach (PropertyInfo prop in targets)
            {
                IEnumerable<Attribute> customs = prop.GetCustomAttributes(typeof (MiniMaxTargetAttribute));
                Type type = customs.FirstOrDefault().GetType();
                PropertyInfo targetProp = type.GetProperty("Target");
                decimal valueTarget = Convert.ToDecimal(targetProp.GetValue(customs.FirstOrDefault(), null));
                foreach (object item in list)
                    f[item] += valueTarget*Convert.ToDecimal(prop.GetValue(item, null));
            }
            var result = new Dictionary<object, decimal>();
            if (ProgressCallback != null)
                ProgressCallback(current, total += list.Count()*(list.Count() - 1)/2);
            for (int i = 0; i < f.Keys.Count - 1; i++)
            {
                object key1 = f.Keys.ElementAt(i);
                result.Add(key1, f[key1]);
                for (int j = i + 1; j < f.Keys.Count; j++)
                {
                    object key2 = f.Keys.ElementAt(j);
                    if (ProgressCallback != null) ProgressCallback(++current, total);
                    if (!union1.All(prop => prop.GetValue(key1, null).Equals(prop.GetValue(key2, null))) ||
                        f[key1] >= f[key2] || result.ContainsKey(key2)) continue;
                    result.Remove(key1);
                    result.Add(key2, f[key2]);
                    break;
                }
            }
            dataSource = new StackListQueue<object>(result.Keys);
        }

        public string GetText(IEnumerable<object> dataSource)
        {
            PropertyInfo[] props = Type.GetProperties();
            var builder = new StringBuilder();
            builder.AppendLine(string.Join(";", props.Select(p => p.Name)));
            foreach (object data in dataSource)
            {
                builder.AppendLine(string.Join(";", props.Select(p => p.GetValue(data, null))));
            }
            return builder.ToString();
        }
    }
}