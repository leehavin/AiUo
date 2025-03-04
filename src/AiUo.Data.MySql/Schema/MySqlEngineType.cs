namespace AiUo.Data.MySql;

/// <summary>
/// MySql 数据库引擎类型，将表字段类型转换成MySqlEngineType类型时不要生成_开头的类型
/// </summary>
public enum MySqlEngineType
{
    /// <summary>
    /// 未知
    /// </summary>
    Unknow,

    #region Numeric 
    /// <summary>
    /// BIT[(M)] 存储位字段值序列，M-Range(1-64)，如 b'111'就等于数值7 空间=(M+7)/8 byte
    /// </summary>
    Bit,
    /// <summary>
    /// TINYINT(1) 获取表库字段类型时将TINYINT(1)转换成bool
    /// 1代表TRUE,0代表FALSE
    /// </summary>
    Bool,
    /// <summary>
    /// 同Bool
    /// </summary>
    _Boolean,
    /// <summary>
    /// TINYINT[(M)] [UNSIGNED] [ZEROFILL] 1byte (-128,127)
    /// </summary>
    TinyInt, 
    /// <summary>
    /// (0,255)
    /// </summary>
    TinyInt_Unsigned,
    /// <summary>
    /// SMALLINT[(M)] [UNSIGNED] [ZEROFILL] 2byte (-32 768，32 767)
    /// </summary>
    SmallInt,
    /// <summary>
    /// (0，65 535)
    /// </summary>
    SmallInt_Unsigned,
    /// <summary>
    /// MEDIUMINT[(M)] [UNSIGNED] [ZEROFILL] 3byte (-8 388 608，8 388 607)
    /// </summary>
    MediumInt,
    /// <summary>
    /// (0，16777215)
    /// </summary>
    MediumInt_Unsigned,

    /// <summary>
    /// INT[(M)] [UNSIGNED] [ZEROFILL] (-2 147 483 648，2 147 483 647)
    /// </summary>
    Int,
    /// <summary>
    /// (0，4 294 967 295) 
    /// </summary>
    Int_Unsigned,
    /// <summary>
    /// 同Int
    /// </summary>
    _Integer,

    /// <summary>
    /// BIGINT[(M)] [UNSIGNED] [ZEROFILL] 8byte (-9223372036854775808，9 223 372 036 854 775 807) 
    /// </summary>
    BigInt,
    /// <summary>
    /// (0，18446744073709551615) 
    /// </summary>
    BigInt_Unsigned,

    /// <summary>
    /// DECIMAL[(M[,D])] [UNSIGNED] [ZEROFILL] 
    /// M(0,65)默认10，D(0,30)默认0，
    /// 存储空间:每9位长度则需要4byte存储,比如：DECIMAL(20,6)小数点左侧14位中9位需要4byte+5位需要3byte+小数6位需要3byte=10byte
    /// </summary>
    Decimal, // 
    /// <summary>
    /// 同Decimal
    /// </summary>
    _Numeric, // NUMERIC[(M[,D])] [UNSIGNED] [ZEROFILL]
    /// <summary>
    /// 同Decimal
    /// </summary>
    _Dec, // DEC[(M[,D])] [UNSIGNED] [ZEROFILL]
    /// <summary>
    /// 同Decimal
    /// </summary>
    _Fixed, // FIXED[(M[,D])] [UNSIGNED] [ZEROFILL]
    /// <summary>
    /// FLOAT[(M,D)] [UNSIGNED] [ZEROFILL] 4byte 精度最多7
    /// 如果不指定M,D，则按实际精度保存，如果指定则四舍五入保存
    /// </summary>
    Float,
    /// <summary>
    /// DOUBLE[(M,D)] [UNSIGNED] [ZEROFILL] 8byte 精度最多15
    /// 如果不指定M,D，则按实际精度保存，如果指定则四舍五入保存
    /// </summary>
    Double,
    /// <summary>
    /// 同Double 根据m,d确定float or double
    /// </summary>
    _Real, // REAL[(M,D)] [UNSIGNED] [ZEROFILL]
    /// <summary>
    /// 同Double
    /// </summary>
    _Double_Precision, //DOUBLE PRECISION[(M,D)] [UNSIGNED] [ZEROFILL]
    #endregion

    #region Date and Time Type
    /// <summary>
    /// 1byte YEAR[(4)] 1901 to 2155 
    /// </summary>
    Year,
    /// <summary>
    /// 3bytes '1000-01-01' to '9999-12-31'
    /// </summary>
    Date,
    /// <summary>
    /// 3bytes TIME[(fsp)] '-838:59:59.000000' to '838:59:59.000000'
    /// </summary>
    Time,
    /// <summary>
    /// 4bytes TIMESTAMP[(fsp)] '1970-01-01 00:00:01.000000' UTC to '2038-01-19 03:14:07.999999' UTC
    /// </summary>
    Timestamp,
    /// <summary>
    /// 8bytes DATETIME[(fsp)] '1000-01-01 00:00:00.000000' to '9999-12-31 23:59:59.999999' fsp取值0-6表示取毫秒位数
    /// </summary>
    DateTime,
    #endregion

    #region String Type
    /// <summary>
    /// CHAR(M) M(0,255) M*w bytes
    /// </summary>
    Char,
    /// <summary>
    /// VARCHAR(M) M(0,65 535)
    /// </summary>
    VarChar,
    /// <summary>
    /// BINARY(M) M(0,255) Mbytes
    /// </summary>
    Binary,
    /// <summary>
    /// VARBINARY(M) M(0, 65 535)
    /// </summary>
    VarBinary,
    /// <summary>
    /// 最大长度 255bytes，建议VarChar替代
    /// </summary>
    TinyText,
    /// <summary>
    /// 最大长度 65535bytes，建议VarChar替代
    /// </summary>
    Text,
    /// <summary>
    /// 最大长度 16777215bytes
    /// </summary>
    MediumText,
    /// <summary>
    /// 最大长度 4294967295bytes
    /// </summary>
    LongText,
    /// <summary>
    /// 最大长度 255bytes
    /// </summary>
    TinyBlob,
    /// <summary>
    /// 最大长度 65535bytes
    /// </summary>
    Blob,
    /// <summary>
    /// 最大长度 16777215bytes
    /// </summary>
    MediumBlob,
    /// <summary>
    /// 最大长度 4294967295bytes
    /// </summary>
    LongBlob,
    /// <summary>
    /// 1或2个字节，这取决于枚举值的数目（最多65,535的值）
    /// 如： ENUM('A', 'B') 内部会存储成数值（NULL->NULL, ''->0, 'A'->1, 'B'->2）
    /// </summary>
    Enum,
    /// <summary>
    /// 1，2，3，4，或8个字节，这取决于组成员的数目（最多64个成员） 
    /// 如：SET('one', 'two') NOT NULL 字段可以存''，'one'，'two'，'one,two'
    /// </summary>
    Set
    #endregion
}