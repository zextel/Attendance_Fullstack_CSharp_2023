using Microsoft.Data.Sqlite;

using SGMK_REST.Models;

using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace SGMK_REST.Repositories
{
    public class TreeRepository : ITreeRepository
    {
        private SqliteConnection connection;
        public TreeRepository()
        {
            connection = new SqliteConnection("Data Source=sample.db");
        }

        /// <summary>
        /// Метод записи в БД нового продукта.
        /// </summary>
        /// <param name="nomenclature">Экземпляр класса PostNomenklature (JSON-сериализуемый объект)</param>
        /// <remarks>
        /// Если nomenclature.ParentID > 0 - добавляется дополнительная запись в таблицу Links с ссылкой связанный объект
        /// </remarks>
        public void AddNomenclature(PostNomenklature nomenclature)
        // Метод
        {
            using (var conn = connection)
            {
                conn.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = "INSERT INTO Nomenklature (name, price) VALUES (@value1, @value2)";
                command.Parameters.AddWithValue("@value1", nomenclature.Name);
                command.Parameters.AddWithValue("@value2", nomenclature.Price);
                command.ExecuteNonQuery();

                if (nomenclature.ParentID > 0)
                {
                    int added_id = 0;
                    command.CommandText = "SELECT MAX(id) FROM Nomenklature";
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            added_id = reader.GetInt32(0);
                        }
                    }

                    command.CommandText = "INSERT INTO Links (nomenklatureId, parentId, quantity) VALUES (@value1, @value2, @value3)";
                    command.Parameters.AddWithValue("@value1", added_id);
                    command.Parameters.AddWithValue("@value2", nomenclature.ParentID);
                    command.Parameters.AddWithValue("@value3", nomenclature.Quantity);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Получение все.
        /// </summary>
        /// <returns>
        /// Список экземпляров класса Nomenclature
        /// </returns>
        public List<Nomenclature> GetNomenclatures()
        {
            List<Nomenclature> result = new List<Nomenclature>();

            using (var conn = connection)
            {
                conn.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = "SELECT name, price, IFNULL(quantity, 1) as quantity " +
                    "FROM Nomenklature n LEFT JOIN Links l on n.id == l.nomenklatureId";

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var name = reader.GetString(0);
                            var price = reader.GetDouble(1);
                            var quantity = reader.GetInt32(2);

                            result.Add(new Nomenclature(name, price, quantity));
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Authenticates a user based on a username and password.
        /// </summary>
        /// <param name="identifier">ID объекта.</param>
        /// <returns>
        /// Перечисляемый объект, представляющий собой древовидную структуру данных типа HierarchicalNomenclature
        /// </returns>
        public IEnumerable<TreeItem<HierarchicalNomenclature>> GetNomenclature(int identifier)
        {
            List<LeveledNomenclature> result = new List<LeveledNomenclature>();

            int max_level = 0;

            using (var conn = connection)
            {
                conn.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = "WITH RECURSIVE " +
                    "hierarchycal(id, name, parent, quantity, price, level) AS( " +
                    $"SELECT id, name, IFNULL(parent, 0), quantity, price, 0 from subquery where id = {identifier} " +
                    "UNION ALL " +
                    "SELECT subquery.id, subquery.name, IFNULL(subquery.parent, 0), subquery.quantity, subquery.price, hierarchycal.level + 1 " +
                    "FROM hierarchycal JOIN subquery ON subquery.parent = hierarchycal.id " +
                    "ORDER BY 6 DESC " +
                    ") " +
                    "SELECT* FROM hierarchycal";
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var id = reader.GetInt32(0);
                            var name = reader.GetString(1);
                            var parentId = reader.GetInt32(2);
                            var quantity = reader.GetInt32(3);
                            var price = reader.GetDouble(4);
                            var level = reader.GetInt32(5);

                            if (level > max_level) max_level = level;

                            result.Add(new LeveledNomenclature(id, name, quantity, price, 0, level, parentId));
                        }
                    }
                }
            }

            // Чтобы сохранить сведения о "влюженности" для расчёта стоимости (первое доп.задание) используется тип LeveledNomenclature

            for (int level = max_level; level >= 0; level--)
            {
                double subtotal = 0;
                Stack<double> stack = new Stack<double>(); // Стек нужен чтобы хранить стоимости продуктов нижестоящих уровней

                for (int i = result.Count - 1; i >= 0; i--)
                {
                    if (result[i].Level == level + 1)
                        subtotal += result[i].SummarizedPrice;
                    else
                    {
                        if (result[i].Level == level)
                        {
                            result[i].SummarizedPrice = result[i].Price * result[i].Quantity + subtotal;
                            while (stack.Count > 0)
                            {
                                result[i].SummarizedPrice += stack.Pop();
                            }

                            subtotal = 0;
                        }
                        else
                        {
                            stack.Push(subtotal);
                            subtotal = 0;
                        }
                    }
                }
            }

            return ConvertToHierarhical(result.GenerateTree(c => c.Id, c => c.ParentID));
        }

        /// <summary>
        /// Вспомогательный метод для преобразования дерева из объектов LeveledNomenclature к дереву объектов HierarchicalNomenclature.
        /// </summary>
        /// <param name="tree">Дерево объектов LeveledNomenclature</param>
        /// <returns>
        /// Дерево объектов HierarchicalNomenclature.
        /// </returns>
        private IEnumerable<TreeItem<HierarchicalNomenclature>> ConvertToHierarhical(IEnumerable<TreeItem<LeveledNomenclature>> tree)
        {
            var h_tree = new List<TreeItem<HierarchicalNomenclature>>();

            // Перебираем всех узлы на одном уровне
            foreach (var item in tree)
            {
                HierarchicalNomenclature temp_data = item.Item;
                h_tree.Add(new TreeItem<HierarchicalNomenclature>(temp_data, ConvertToHierarhical(item.Children)));
            }
            return h_tree;
        }
    }
}

