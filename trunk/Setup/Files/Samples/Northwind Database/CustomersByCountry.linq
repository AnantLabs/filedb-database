<Query Kind="Statements">
</Query>

// get all Customers grouped by Country

var custGrps =
	from c in Customers
	group c by c.Country into custGrp
	select new
	{
		Country = custGrp.Key,
		Customers = custGrp
	};

custGrps.Dump();