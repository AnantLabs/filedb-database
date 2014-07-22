<Query Kind="Statements">
</Query>

// Simple join of Customers, Orders, OrderDetails and Products tables
// with no grouping

var custOrderDetails =
	from c in Customers
	join o in Orders on c.CustomerID equals o.CustomerID
	join od in OrderDetails on o.OrderID equals od.OrderID
	join p in Products on od.ProductID equals p.ProductID
	select new
	{
		ID = c.CustomerID,
		CompanyName = c.CompanyName,
		OrderID = o.OrderID,
		OrderDate = o.OrderDate,
		ProductName = p.ProductName,
		UnitPrice = od.UnitPrice,
		Quantity = od.Quantity
	};

custOrderDetails.Dump();