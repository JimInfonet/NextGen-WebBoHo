using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace InfonetCSCR.Reporting.Services
{
  public static class Extensions
  {
    public static DataTable ToDataTable<T>(this List<T> list)
    {
      DataTable dataTable = new DataTable(typeof(T).Name);
      PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
      foreach (var prop in props)
      {
        dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
      }
      foreach (var item in list)
      {
        var values = new object[props.Length];
        for (int i = 0; i < props.Length; i++)
        {
          values[i] = props[i].GetValue(item, null);
        }
        dataTable.Rows.Add(values);
      }
      return dataTable;
    }
  }
}
