<Query Kind="Statements">
</Query>

// generate Aggregate totals from Orders

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
					OrderDate = (DateTime) order.OrderDate,
					NumItems = orderDetailsGrp.Count(),
					TotalSale = orderDetailsGrp.Sum( od => (float) od.UnitPrice * (int) od.Quantity ),
					AverageSale = orderDetailsGrp.Average( od => (float) od.UnitPrice * (int) od.Quantity )
				}
		};
		
customerOrders.Dump();