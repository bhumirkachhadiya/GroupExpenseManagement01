using System.Data;

namespace GroupExpenseManagement01.CommonClasses
{
    //select list item
    public class DropdownClass
    {
        public static List<T> GetDropdownList<T>(DataTable dataTable) where T : new()
        {
            List<T> list = new List<T>();

            foreach (DataRow row in dataTable.Rows)
            {
                T item = new T(); // Create a new instance of T

                // Use reflection to set properties
                foreach (var prop in typeof(T).GetProperties())
                {
                    if (dataTable.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                    {
                        prop.SetValue(item, Convert.ChangeType(row[prop.Name], prop.PropertyType));
                    }
                }

                list.Add(item);
            }

            return list;
        }

    }
}
