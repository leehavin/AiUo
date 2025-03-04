
using MySql.Data.MySqlClient;
using System;
using System.Data;
using AiUo.Data.Schema;

namespace AiUo.Data.MySql;

/// <summary>
/// MySql 数据库字段类型与.net类型转换
/// engineTypeString => TEngineType => TDbType
///                                 => DotNetType => NetTypeString
/// </summary>
public class MySqlTypeMapper : IDbTypeMapper<MySqlEngineType, MySqlDbType>
{
    /// <summary>
    /// 根据 MySQL 数据库引擎原生类型推断MySqlEngineType
    /// </summary>
    /// <param name="engineTypeString">数据库字段类型字符串表示，如 tinyint</param>
    /// <param name="engineTypeFull">数据库字段字符串全描述，如 tinyint(1)</param>
    /// <returns></returns>
    public MySqlEngineType MapEngineType(string engineTypeString, string engineTypeFull = null)
    {
        bool isUnsigned = !string.IsNullOrEmpty(engineTypeFull) ? engineTypeFull.IndexOf("unsigned") > -1 : false;
        switch (engineTypeString.ToLower())
        {
            case "bit": return MySqlEngineType.Bit;
            case "tinyint":
                if (engineTypeFull == "tinyint(1)") return MySqlEngineType.Bool;//tinyint(1)处理成bool
                if (isUnsigned) return MySqlEngineType.TinyInt_Unsigned; else return MySqlEngineType.TinyInt;
            case "smallint": return isUnsigned ? MySqlEngineType.SmallInt_Unsigned : MySqlEngineType.SmallInt;
            case "mediumint": return isUnsigned ? MySqlEngineType.MediumInt_Unsigned : MySqlEngineType.MediumInt;
            case "int": return isUnsigned ? MySqlEngineType.Int_Unsigned : MySqlEngineType.Int;
            case "bigint": return isUnsigned ? MySqlEngineType.BigInt_Unsigned : MySqlEngineType.BigInt;
            case "float": return MySqlEngineType.Float;
            case "double": return MySqlEngineType.Double;
            case "year": return MySqlEngineType.Year;
            case "date": return MySqlEngineType.Date;
            case "time": return MySqlEngineType.Time;
            case "timestamp": return MySqlEngineType.Timestamp;
            case "datetime": return MySqlEngineType.DateTime;
            case "char": return MySqlEngineType.Char;
            case "varchar": return MySqlEngineType.VarChar;
            case "binary": return MySqlEngineType.Binary;
            case "varbinary": return MySqlEngineType.VarBinary;
            case "tinytext": return MySqlEngineType.TinyText;
            case "text": return MySqlEngineType.Text;
            case "mediumtext": return MySqlEngineType.MediumText;
            case "longtext": return MySqlEngineType.LongText;
            case "tinyblob": return MySqlEngineType.TinyBlob;
            case "blob": return MySqlEngineType.Blob;
            case "mediumblob": return MySqlEngineType.MediumBlob;
            case "longblob": return MySqlEngineType.LongBlob;
            case "enum": return MySqlEngineType.Enum;
            case "set": return MySqlEngineType.Set;
            case "decimal": return MySqlEngineType.Decimal;
        }
        throw new ArgumentException("未知的MySQL原始数据类型：" + engineTypeString, "engineType");
    }

    /// <summary>
    /// 根据 MySQL 数据库引擎类型推断MySqlDbType
    /// </summary>
    /// <param name="engineType"></param>
    /// <returns></returns>
    public MySqlDbType MapDbType(MySqlEngineType engineType)
    {
        switch (engineType)
        {
            //? JSON ? Geometry ?Guid
            case MySqlEngineType.Bool:
            case MySqlEngineType._Boolean:
            case MySqlEngineType.TinyInt:
                return MySqlDbType.Byte;
            case MySqlEngineType.TinyInt_Unsigned:
                return MySqlDbType.UByte;

            case MySqlEngineType.SmallInt:
                return MySqlDbType.Int16;
            case MySqlEngineType.SmallInt_Unsigned:
                return MySqlDbType.UInt16;

            case MySqlEngineType.MediumInt:
                return MySqlDbType.Int24;
            case MySqlEngineType.MediumInt_Unsigned:
                return MySqlDbType.UInt24;

            case MySqlEngineType.Int:
            case MySqlEngineType._Integer:
                return MySqlDbType.Int32;
            case MySqlEngineType.Int_Unsigned:
                return MySqlDbType.UInt32;

            case MySqlEngineType.BigInt:
                return MySqlDbType.Int64;
            case MySqlEngineType.BigInt_Unsigned:
                return MySqlDbType.UInt64;

            case MySqlEngineType.Bit:
                return MySqlDbType.Bit;
            case MySqlEngineType.Char:
                return MySqlDbType.String; // 定长 <255
            case MySqlEngineType.VarChar:
                return MySqlDbType.VarChar;//.VarString; // VarChar <255 bytes

            case MySqlEngineType.Float:
                return MySqlDbType.Float;
            case MySqlEngineType.Double:
            case MySqlEngineType._Real: //
            case MySqlEngineType._Double_Precision:
                return MySqlDbType.Double;
            case MySqlEngineType.Decimal:
            case MySqlEngineType._Numeric:
            case MySqlEngineType._Dec:
            case MySqlEngineType._Fixed:
                return MySqlDbType.NewDecimal;// MySql5.0.3后用NewDecimal之前Decimal;

            case MySqlEngineType.Timestamp:
                return MySqlDbType.Timestamp;
            case MySqlEngineType.Date:
                return MySqlDbType.Date;
            case MySqlEngineType.Time:
                return MySqlDbType.Time;
            case MySqlEngineType.DateTime:
                return MySqlDbType.DateTime;
            case MySqlEngineType.Year:
                return MySqlDbType.Year;
            case MySqlEngineType.Binary:
                return MySqlDbType.Binary;
            case MySqlEngineType.VarBinary:
                return MySqlDbType.VarBinary;
            case MySqlEngineType.TinyText:
                return MySqlDbType.TinyText;
            case MySqlEngineType.MediumText:
                return MySqlDbType.MediumText;
            case MySqlEngineType.LongText:
                return MySqlDbType.LongText;
            case MySqlEngineType.Text:
                return MySqlDbType.Text;
            case MySqlEngineType.TinyBlob:
                return MySqlDbType.TinyBlob;
            case MySqlEngineType.MediumBlob:
                return MySqlDbType.MediumBlob;
            case MySqlEngineType.LongBlob:
                return MySqlDbType.LongBlob;
            case MySqlEngineType.Blob:
                return MySqlDbType.Blob;
            case MySqlEngineType.Enum:
                return MySqlDbType.Enum;
            case MySqlEngineType.Set:
                return MySqlDbType.Set;
        }
        throw new Exception("未知的 MySQL 数据库引擎类型。" + engineType.ToString());
    }

    /// <summary>
    /// 根据 MySQL 数据库引擎原生类型推断MySqlDbType
    /// </summary>
    /// <param name="engineTypeString"></param>
    /// <param name="engineTypeFull"></param>
    /// <returns></returns>
    public MySqlDbType MapDbType(string engineTypeString, string engineTypeFull = null)
        => MapDbType(MapEngineType(engineTypeString, engineTypeFull));

    /// <summary>
    /// 根据 DbType 类型推断MySqlDbType
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public MySqlDbType MapDbType(DbType type)
    {
        switch (type)
        {
            case DbType.Guid: return MySqlDbType.Guid;
            case DbType.AnsiString: case DbType.String: return MySqlDbType.VarChar;
            case DbType.AnsiStringFixedLength: case DbType.StringFixedLength: return MySqlDbType.String;
            case DbType.Boolean: case DbType.Byte: return MySqlDbType.UByte;
            case DbType.SByte: return MySqlDbType.Byte;
            case DbType.Date: return MySqlDbType.Date;
            case DbType.DateTime: return MySqlDbType.DateTime;
            case DbType.Time: return MySqlDbType.Time;
            case DbType.Single: return MySqlDbType.Float;
            case DbType.Double: return MySqlDbType.Double;
            case DbType.Int16: return MySqlDbType.Int16;
            case DbType.UInt16: return MySqlDbType.UInt16;
            case DbType.Int32: return MySqlDbType.Int32;
            case DbType.UInt32: return MySqlDbType.UInt32;
            case DbType.Int64: return MySqlDbType.Int64;
            case DbType.UInt64: return MySqlDbType.UInt64;
            case DbType.Decimal: case DbType.Currency: return MySqlDbType.Decimal;
            case DbType.Object: case DbType.VarNumeric: case DbType.Binary: default: return MySqlDbType.Blob;
        }
        //Geometry
        throw new Exception("未知的 DbType 类型。" + type.ToString());
    }

    /// <summary>
    /// 根据 MySqlDbType 类型推断 DbType
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public DbType MapSysDbType(MySqlDbType type)
    {
        switch (type)
        {
            case MySqlDbType.NewDecimal:
            case MySqlDbType.Decimal:
                return DbType.Decimal;
            case MySqlDbType.Byte:
                return DbType.SByte;
            case MySqlDbType.UByte:
                return DbType.Byte;
            case MySqlDbType.Int16:
                return DbType.Int16;
            case MySqlDbType.UInt16:
                return DbType.UInt16;
            case MySqlDbType.Int24:
            case MySqlDbType.Int32:
                return DbType.Int32;
            case MySqlDbType.UInt24:
            case MySqlDbType.UInt32:
                return DbType.UInt32;
            case MySqlDbType.Int64:
                return DbType.Int64;
            case MySqlDbType.UInt64:
                return DbType.UInt64;
            case MySqlDbType.Bit:
                return DbType.Int32;
            //return DbType.UInt64;
            case MySqlDbType.Float:
                return DbType.Single;
            case MySqlDbType.Double:
                return DbType.Double;
            case MySqlDbType.Timestamp:
            case MySqlDbType.DateTime:
                //case MySqlDbType.Datetime:
                return DbType.DateTime;
            case MySqlDbType.Date:
            case MySqlDbType.Newdate:
            case MySqlDbType.Year:
                return DbType.Date;
            case MySqlDbType.Time:
                return DbType.Time;
            case MySqlDbType.Enum:
            case MySqlDbType.Set:
            case MySqlDbType.VarChar: // 变长 <255
            case MySqlDbType.VarString: // 变长  <65535
            case MySqlDbType.JSON:
            case MySqlDbType.TinyText:
            case MySqlDbType.MediumText:
            case MySqlDbType.LongText:
            case MySqlDbType.Text:
                return DbType.String; // Unicode
            case MySqlDbType.TinyBlob:
            case MySqlDbType.MediumBlob:
            case MySqlDbType.LongBlob:
            case MySqlDbType.Blob:
                return DbType.Object;
            case MySqlDbType.String: //定长
                return DbType.StringFixedLength; //Unicode定长
            case MySqlDbType.Guid:
                return DbType.Guid;
            case MySqlDbType.Binary:
            case MySqlDbType.VarBinary:
                return DbType.Binary;
            //case MySqlDbType.Geometry:
        }
        throw new Exception("未知的 MySqlDbType 类型。" + type.ToString());
    }

    /// <summary>
    /// 根据 MySQL 数据库引擎类型推断ORM映射的.NET类型，依据可存下，尽量少类型，不考虑无符号类型
    /// </summary>
    /// <param name="engineType"></param>
    /// <returns></returns>
    public Type MapDotNetType(MySqlEngineType engineType)
    {
        switch (engineType)
        {
            case MySqlEngineType.Bit:
                //return typeof(ulong);
                return typeof(int);
            case MySqlEngineType.Bool:
            case MySqlEngineType._Boolean:
                return typeof(bool);
            case MySqlEngineType.TinyInt:
            //return typeof(sbyte);
            case MySqlEngineType.TinyInt_Unsigned:
            //return typeof(byte);
            case MySqlEngineType.SmallInt:
            //return typeof(short);
            case MySqlEngineType.SmallInt_Unsigned:
            //return typeof(ushort);
            case MySqlEngineType.MediumInt:
            case MySqlEngineType.Int:
            case MySqlEngineType._Integer:
            case MySqlEngineType.Year:
            case MySqlEngineType.MediumInt_Unsigned:
                return typeof(int);
            case MySqlEngineType.Int_Unsigned:
            //return typeof(uint);
            case MySqlEngineType.BigInt:
                return typeof(long);
            case MySqlEngineType.BigInt_Unsigned:
                return typeof(ulong);
            // ? decimal
            case MySqlEngineType.Decimal:
            case MySqlEngineType._Numeric:
            case MySqlEngineType._Dec:
            case MySqlEngineType._Fixed:
                return typeof(decimal);
            case MySqlEngineType.Float:
                return typeof(float);
            case MySqlEngineType.Double:
            case MySqlEngineType._Double_Precision:
                return typeof(double);
            case MySqlEngineType.DateTime:
            case MySqlEngineType.Date:
            case MySqlEngineType.Timestamp:
                return typeof(DateTime);
            case MySqlEngineType.Time:
                return typeof(TimeSpan);
            case MySqlEngineType.Char:
            case MySqlEngineType.VarChar:
            case MySqlEngineType.TinyText:
            case MySqlEngineType.Text:
            case MySqlEngineType.MediumText:
            case MySqlEngineType.LongText:
            case MySqlEngineType.Enum:
            case MySqlEngineType.Set:
                return typeof(string);
            case MySqlEngineType.Binary:
            case MySqlEngineType.VarBinary:
            case MySqlEngineType.TinyBlob:
            case MySqlEngineType.Blob:
            case MySqlEngineType.MediumBlob:
            case MySqlEngineType.LongBlob:
                return typeof(byte[]);
        }
        throw new Exception("未知的 MySqlEngineType 类型。" + engineType.ToString());
    }

    /// <summary>
    /// 根据MySqlDbType 类型推断.NET类型
    /// </summary>
    /// <param name="dbType"></param>
    /// <returns></returns>
    public Type MapDotNetType(MySqlDbType dbType)
    {
        // 不支持Geometry
        // MySQL的Decimal是精确值，.NET的decimal有精度限制，需要注意
        // 日期类型全部转换成Datetime类型不太合理
        switch (dbType)
        {
            case MySqlDbType.Byte:
            case MySqlDbType.UByte:
            case MySqlDbType.Int16:
            case MySqlDbType.UInt16:
            case MySqlDbType.Int24:
            case MySqlDbType.Int32:
            case MySqlDbType.Year:
            case MySqlDbType.UInt24:
            case MySqlDbType.UInt32:
            case MySqlDbType.Bit:
                return typeof(int);
            case MySqlDbType.Int64:
                return typeof(long);
            case MySqlDbType.UInt64:
                return typeof(ulong);
            case MySqlDbType.Time:
                return typeof(TimeSpan);
            case MySqlDbType.Date:
            case MySqlDbType.DateTime:
            case MySqlDbType.Newdate:
            case MySqlDbType.Timestamp:
                return typeof(DateTime);//
            case MySqlDbType.Decimal:
            case MySqlDbType.NewDecimal:
                return typeof(decimal);//
            case MySqlDbType.Float:
                return typeof(float);
            case MySqlDbType.Double:
                return typeof(double);
            case MySqlDbType.Set:
            case MySqlDbType.Enum:
            case MySqlDbType.String:
            case MySqlDbType.VarString:
            case MySqlDbType.VarChar:
            case MySqlDbType.Text:
            case MySqlDbType.TinyText:
            case MySqlDbType.MediumText:
            case MySqlDbType.LongText:
            case MySqlDbType.JSON:
                return typeof(string);
            case MySqlDbType.Blob:
            case MySqlDbType.MediumBlob:
            case MySqlDbType.LongBlob:
            case MySqlDbType.TinyBlob:
            case MySqlDbType.Binary:
            case MySqlDbType.VarBinary:
                return typeof(byte[]);
            case MySqlDbType.Guid:
                return typeof(Guid);
            default:
                throw new Exception("未知的 MySqlDbType 类型" + dbType.ToString());
        }
    }

    public MySqlDbType MapDotNetTypeToDbType(Type type)
    {
        switch (type.FullName)
        {
            case SimpleTypeNames.Byte:
                return MySqlDbType.Byte;
            case SimpleTypeNames.SByte:
                return MySqlDbType.UByte;
            case SimpleTypeNames.Int16:
                return MySqlDbType.Int16;
            case SimpleTypeNames.UInt16:
                return MySqlDbType.UInt16;
            case SimpleTypeNames.Int32:
                return MySqlDbType.Int32;
            case SimpleTypeNames.UInt32:
                return MySqlDbType.UInt32;
            case SimpleTypeNames.Int64:
                return MySqlDbType.Int64;
            case SimpleTypeNames.UInt64:
                return MySqlDbType.UInt64;
            case SimpleTypeNames.Single:
                return MySqlDbType.Float;
            case SimpleTypeNames.Double:
                return MySqlDbType.Double;
            case SimpleTypeNames.Boolean:
                return MySqlDbType.Byte;
            case SimpleTypeNames.Char:
                return MySqlDbType.VarChar;
            case SimpleTypeNames.Decimal:
                return MySqlDbType.Decimal;
            case SimpleTypeNames.TimeSpan:
                return MySqlDbType.Time;
            case SimpleTypeNames.DateTime:
                return MySqlDbType.DateTime;
            case SimpleTypeNames.Guid:
                return MySqlDbType.Guid;
            case SimpleTypeNames.String:
                return MySqlDbType.VarString;
            case SimpleTypeNames.Bytes:
                return MySqlDbType.Blob;
            default:
                return default;
        }
    }
}