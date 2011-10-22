using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if false
namespace LINQPad.User
{
public class FileDbContext //: System.Data.Linq.DataContext
{

string _dbPath;

public FileDbContext( string dbPath ) //: base( dbPath )
{
_dbPath = dbPath;
}

public FileDbNs.Table Categories
{
  get
  {
    FileDbNs.Table _Categories;
    FileDbNs.FileDb db = new FileDbNs.FileDb();
    try
    {
      db.Open( @"C:\Dev\EzTools.NET\FileDb\LinqPad Driver\FileDbDynamicDriver\Northwind Database\Categories.fdb" );
      _Categories = db.GetAllRecords();
    }
    finally
    {
      if( db.IsOpen )
        db.Close();
    }
    return _Categories;
  }
}

public FileDbNs.Table Customers
{
  get
  {
    FileDbNs.Table _Customers;
    FileDbNs.FileDb db = new FileDbNs.FileDb();
    try
    {
      db.Open( @"C:\Dev\EzTools.NET\FileDb\LinqPad Driver\FileDbDynamicDriver\Northwind Database\Customers.fdb" );
      _Customers = db.GetAllRecords();
    }
    finally
    {
      if( db.IsOpen )
        db.Close();
    }
    return _Customers;
  }
}

public FileDbNs.Table Employees
{
  get
  {
    FileDbNs.Table _Employees;
    FileDbNs.FileDb db = new FileDbNs.FileDb();
    try
    {
      db.Open( @"C:\Dev\EzTools.NET\FileDb\LinqPad Driver\FileDbDynamicDriver\Northwind Database\Employees.fdb" );
      _Employees = db.GetAllRecords();
    }
    finally
    {
      if( db.IsOpen )
        db.Close();
    }
    return _Employees;
  }
}

public FileDbNs.Table OrderDetails
{
  get
  {
    FileDbNs.Table _OrderDetails;
    FileDbNs.FileDb db = new FileDbNs.FileDb();
    try
    {
      db.Open( @"C:\Dev\EzTools.NET\FileDb\LinqPad Driver\FileDbDynamicDriver\Northwind Database\OrderDetails.fdb" );
      _OrderDetails = db.GetAllRecords();
    }
    finally
    {
      if( db.IsOpen )
        db.Close();
    }
    return _OrderDetails;
  }
}

public FileDbNs.Table Orders
{
  get
  {
    FileDbNs.Table _Orders;
    FileDbNs.FileDb db = new FileDbNs.FileDb();
    try
    {
      db.Open( @"C:\Dev\EzTools.NET\FileDb\LinqPad Driver\FileDbDynamicDriver\Northwind Database\Orders.fdb" );
      _Orders = db.GetAllRecords();
    }
    finally
    {
      if( db.IsOpen )
        db.Close();
    }
    return _Orders;
  }
}

public FileDbNs.Table Products
{
  get
  {
    FileDbNs.Table _Products;
    FileDbNs.FileDb db = new FileDbNs.FileDb();
    try
    {
      db.Open( @"C:\Dev\EzTools.NET\FileDb\LinqPad Driver\FileDbDynamicDriver\Northwind Database\Products.fdb" );
      _Products = db.GetAllRecords();
    }
    finally
    {
      if( db.IsOpen )
        db.Close();
    }
    return _Products;
  }
}

public FileDbNs.Table Shippers
{
  get
  {
    FileDbNs.Table _Shippers;
    FileDbNs.FileDb db = new FileDbNs.FileDb();
    try
    {
      db.Open( @"C:\Dev\EzTools.NET\FileDb\LinqPad Driver\FileDbDynamicDriver\Northwind Database\Shippers.fdb" );
      _Shippers = db.GetAllRecords();
    }
    finally
    {
      if( db.IsOpen )
        db.Close();
    }
    return _Shippers;
  }
}

public FileDbNs.Table Suppliers
{
  get
  {
    FileDbNs.Table _Suppliers;
    FileDbNs.FileDb db = new FileDbNs.FileDb();
    try
    {
      db.Open( @"C:\Dev\EzTools.NET\FileDb\LinqPad Driver\FileDbDynamicDriver\Northwind Database\Suppliers.fdb" );
      _Suppliers = db.GetAllRecords();
    }
    finally
    {
      if( db.IsOpen )
        db.Close();
    }
    return _Suppliers;
  }
}

}
}
#endif
