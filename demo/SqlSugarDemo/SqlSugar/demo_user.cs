using SqlSugar;

namespace SqlSugarDemo.DAL;

///<summary>
///用户表
/// 多行描述！！！
///</summary>
[SugarTable("demo_user")]
public partial class Sdemo_userEO
{
	public Sdemo_userEO(){

		this.FBit =1;
		this.FTinyInt =127;
		this.FTinyInt_Unsigned =255;
		this.F_Boolean =true;
		this.FBool_TinyInt =false;
		this.FSmallInt =-32768;
		this.FSmallInt_Unsigned =65535;
		this.FMediumInt =-8388608;
		this.FInt =-2147483648;
		this.FInt_Unsigned =4294967295;
		this.FBigInt =-9223372036854775808;
		this.FFloat =12.3457f;
		this.FDouble =123456789.1234567d;
		this.FTimestamp =DateTime.Now;
		this.FDecimal =123m;

	}
	/// <summary>
	/// Desc:用户编码（自增字段）
	/// Default:
	/// Nullable:False
	/// </summary>           
	[SugarColumn(IsPrimaryKey=true)]
	public long UserID {get;set;}

	/// <summary>
	/// Desc:类别编码
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? ClassID {get;set;}

	/// <summary>
	/// Desc:字段1
	///                      多行1
	///                      多行2
	/// Default:b'1'
	/// Nullable:False
	/// </summary>           
	public int FBit {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public int? FBit_Max {get;set;}

	/// <summary>
	/// Desc:
	/// Default:127
	/// Nullable:False
	/// </summary>           
	public int FTinyInt {get;set;}

	/// <summary>
	/// Desc:
	/// Default:255
	/// Nullable:True
	/// </summary>           
	public int? FTinyInt_Unsigned {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public bool? FBool {get;set;}

	/// <summary>
	/// Desc:
	/// Default:1
	/// Nullable:False
	/// </summary>           
	public bool F_Boolean {get;set;}

	/// <summary>
	/// Desc:
	/// Default:0
	/// Nullable:True
	/// </summary>           
	public bool? FBool_TinyInt {get;set;}

	/// <summary>
	/// Desc:
	/// Default:-32768
	/// Nullable:False
	/// </summary>           
	public int FSmallInt {get;set;}

	/// <summary>
	/// Desc:
	/// Default:65535
	/// Nullable:True
	/// </summary>           
	public int? FSmallInt_Unsigned {get;set;}

	/// <summary>
	/// Desc:
	/// Default:-8388608
	/// Nullable:False
	/// </summary>           
	public int FMediumInt {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public int? FMediumInt_Unsigned {get;set;}

	/// <summary>
	/// Desc:
	/// Default:-2147483648
	/// Nullable:False
	/// </summary>           
	public int FInt {get;set;}

	/// <summary>
	/// Desc:
	/// Default:4294967295
	/// Nullable:True
	/// </summary>           
	public long? FInt_Unsigned {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public int? F_Integer {get;set;}

	/// <summary>
	/// Desc:
	/// Default:-9223372036854775808
	/// Nullable:False
	/// </summary>           
	public long FBigInt {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public ulong? FBigInt_Unsigned {get;set;}

	/// <summary>
	/// Desc:
	/// Default:12.3457
	/// Nullable:False
	/// </summary>           
	public float FFloat {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public float? FFloat_Max {get;set;}

	/// <summary>
	/// Desc:
	/// Default:123456789.1234567
	/// Nullable:False
	/// </summary>           
	public double FDouble {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public double? FDouble_Max {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public double? F_Real {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public double? F_Double_Precision {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public int? FYear {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public DateTime? FDate {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public TimeSpan? FTime {get;set;}

	/// <summary>
	/// Desc:
	/// Default:CURRENT_TIMESTAMP
	/// Nullable:True
	/// </summary>           
	public DateTime? FTimestamp {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public DateTime? FDateTime {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? FChar {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? FVarChar {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public byte[] FBinary {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public byte[] FVarBinary {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? FTinyText {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? FText {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? FMediumText {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? FLongText {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public byte[] FTinyBlob {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public byte[] FBlob {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public byte[] FMediumBlob {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public byte[] FLongBlob {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public string? FEnum {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public object FSet {get;set;}

	/// <summary>
	/// Desc:
	/// Default:123
	/// Nullable:False
	/// </summary>           
	public decimal FDecimal {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public decimal? FDecimal_Max {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public decimal? F_Numeric {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public decimal? F_Dec {get;set;}

	/// <summary>
	/// Desc:
	/// Default:
	/// Nullable:True
	/// </summary>           
	public decimal? F_Fixed {get;set;}

}