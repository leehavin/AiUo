using AiUo.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;

namespace AiUo.Extensions.EPPlus;

public class ExcelPackageEx : IDisposable
{
    #region Constructors
    private ExcelPackage _pkg;
    public ExcelPackageEx() 
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        _pkg = new ExcelPackage(); 
    }
    public ExcelPackageEx(string file, string password = null) 
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        _pkg = string.IsNullOrEmpty(password) 
            ? new ExcelPackage(file)
            : new ExcelPackage(file, password);
    }
    public ExcelPackageEx(Stream stream, string password = null) 
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        _pkg = new ExcelPackage();
        if (string.IsNullOrEmpty(password))
            _pkg.Load(stream);
        else
            _pkg.Load(stream, password);
    }
    #endregion

    #region Save
    public void Save()
    {
        _pkg.Save();
        Dispose();
    }
    public void Save(string file, string password = null) 
    {
        if (System.IO.File.Exists(file))
            System.IO.File.SetAttributes(file, FileAttributes.Normal);

        if (string.IsNullOrEmpty(password))
            _pkg.SaveAs(new FileInfo(file));
        else
            _pkg.SaveAs(new FileInfo(file), password);
        Dispose();
    }
    public void Save(Stream stream, string password = null)
    {
        if (string.IsNullOrEmpty(password))
            _pkg.SaveAs(stream);
        else
            _pkg.SaveAs(stream, password);
        Dispose();
    }
    #endregion

    public ExcelWorkbook Workbook => _pkg.Workbook;
    public FileInfo File => _pkg.File;

    #region ExcelWorksheet

    public bool ContainsSheet(int index)
    {
        return Workbook.Worksheets.Count > index;
    }
    public bool ContainsSheet(string name)
    {
        return Workbook.Worksheets[name] != null;
    }
    public bool TryGetWorksheet(string sheetName, out ExcelWorksheet sheet)
    {
        sheet = Workbook.Worksheets[sheetName];
        return sheet != null;
    }
    public bool TryGetWorksheet(int sheetIndex, out ExcelWorksheet sheet)
    {
        sheet = ContainsSheet(sheetIndex) ? Workbook.Worksheets[sheetIndex] : null;
        return sheet != null;
    }
    public List<string> GetWorksheetNames()
    {
        var ret = new List<string>();
        var emt = Workbook.Worksheets.GetEnumerator();
        while (emt.MoveNext())
        {
            ret.Add(emt.Current.Name);
        }
        return ret;
    }
    public List<ExcelWorksheet> GetWorksheets()
    {
        var ret = new List<ExcelWorksheet>();
        var enumerator = Workbook.Worksheets.GetEnumerator();
        while (enumerator.MoveNext())
        {
            ret.Add(enumerator.Current);
        }
        return ret;
    }
    #endregion

    #region IDisposable
    private bool _disposed = false;
    public void Dispose()
    {
        if (_disposed) return;
        _pkg?.Dispose();
        GC.SuppressFinalize(this);
        _disposed = true;
    }
    ~ExcelPackageEx()
    {
        LogUtil.Error($"ExcelPackageEx没有释放资源。");
    }
    #endregion
}