using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
/// <summary>
/// object型の拡張メソッドを管理するクラス
/// </summary>
public static class ObjectExtensions
{
    private const string SEPARATOR = "\n";       // 区切り記号として使用する文字列
    private const string FORMAT = "{0}:{1}";    // 複合書式指定文字列

    static string fieldInfo<T>(T obj , FieldInfo c)
    {
        return string.Format(FORMAT, c.Name, c.GetValue(obj));
    }
    /// <summary>
    /// すべての公開フィールドの情報を文字列にして返します
    /// </summary>
    public static string ToStringFields<T>(this T obj)
    {
        return string.Join(SEPARATOR, obj
            .GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.Public)
            .Select(c => fieldInfo(obj,c ) ) );
    }

    static bool isPrimitive(this object o)
    {
        TypeCode code = Type.GetTypeCode(o.GetType());
        switch (code)
        {
            case TypeCode.Boolean:
                Console.WriteLine("bool");
                return true;
            case TypeCode.Char:
                Console.WriteLine("char");
                return true;
            case TypeCode.Int16:
                Console.WriteLine("short");
                return true;
            case TypeCode.Int32:
                Console.WriteLine("int");
                return true;
            case TypeCode.Int64:
                Console.WriteLine("long");
                return true;
            case TypeCode.Double:
                Console.WriteLine("ddouble");
                return true;
            case TypeCode.Decimal:
                Console.WriteLine("decimal");
                return true;
            case TypeCode.DateTime:
                Console.WriteLine("DateTime");
                return true;
            case TypeCode.String:
                Console.WriteLine("string");
                return true;
            case TypeCode.Object:
                Console.WriteLine("object");
                //return true;
                return false;
            default:
                Console.WriteLine("other");
                return false;
        }
    }

    static string propInfo<T>(T obj , PropertyInfo c)
    {
        object arg1 = null;
        string temp = "";
        try
        {
            arg1 = c.GetValue(obj, null);
            temp = string.Format(FORMAT, c.Name, arg1);
            return temp;
        }
        catch
        {
            return "";
        }
        //if (arg1 == null) return "";
        //if (isPrimitive(arg1) )
        //{
        //    Console.WriteLine("arg1 is prim :" + arg1);
        //    return "";
        //}
        //else if(arg1.GetType().IsArray )
        //{
        //    var arr = arg1 as IEnumerable;
        //    foreach (var item in arr)
        //    {
        //        Console.WriteLine("arg1 is arr :" + item);

        //    }
        //}
        //else
        //{
        //    Console.WriteLine("temp is not prim :"+ temp);
        //    temp += arg1.ToStringReflection();
        //}
        //return temp;
    }

    /// <summary>
    /// すべての公開プロパティの情報を文字列にして返します
    /// </summary>
    public static string ToStringProperties<T>(this T obj)
    {
        return string.Join(SEPARATOR, obj
            .GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(c => c.CanRead)
            .Select(c => propInfo(obj,c ) ) );
    }

    /// <summary>
    /// すべての公開フィールドと公開プロパティの情報を文字列にして返します
    /// </summary>
    public static string ToStringReflection<T>(this T obj)
    {
        return string.Join(SEPARATOR,
            "fields",
            obj.ToStringFields(),
            "props",
            obj.ToStringProperties());
    }

    public static string ToJson( this object obj )
    {
            var jsonString = JsonConvert.SerializeObject( obj , Formatting.Indented ,
                new JsonConverter[] { new StringEnumConverter( ) } );
            return jsonString;
    }
}
