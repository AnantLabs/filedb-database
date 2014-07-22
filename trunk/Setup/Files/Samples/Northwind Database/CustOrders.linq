<Query Kind="Statements">
  <Connection>
    <ID>46f0acbf-ea54-4b94-96fc-6cfdbde778c5</ID>
    <Persist>true</Persist>
    <Driver Assembly="FileDbDynamicDriver" PublicKeyToken="9f29ae367fa08336">FileDbDynamicDriverNs.FileDbDynamicDriver</Driver>
    <DriverData>
      <Extension>fdb</Extension>
      <Folder>C:\Program Files\EzTools\FileDb\Samples\Northwind Database</Folder>
    </DriverData>
  </Connection>
</Query>

// get all Customer Orders by joining the Customers, Orders and OrderDetails tables

var customerOrders =
	from c in Customers
	join o in Orders on c.CustomerID equals o.CustomerID
	into ordersGrp  // "into" gives us hierarchical result sequence
	select new
	{
		CustomerId = c.CustomerID,
		CustomerName = c.CompanyName,
		Orders =
			from order in ordersGrp
			join od in OrderDetails on order.OrderID equals od.OrderID
			into orderDetailsGrp  // "into" gives us hierarchical result sequence
			select new
			{
				OrderId = order.OrderID,
				OrderDate = order.OrderDate,
				OrderDetails =
					from ood in orderDetailsGrp
					join p in Products on ood.ProductID equals p.ProductID // pull in the Product name
					select new
					{
						ProductName = p.ProductName,
						UnitPrice = ood.UnitPrice,
						Quantity = ood.Quantity
					}
			}
	};
	
customerOrders.Dump();