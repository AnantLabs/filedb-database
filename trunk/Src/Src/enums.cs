using System;
using System.Collections.Generic;

namespace FileDbNs
{
    //=====================================================================
    /// <summary>
    /// Specifies the data type for database Fields
    /// </summary>
    /// 
    internal enum DataTypeEnum_old : short
    {
        String = 0, Byte = 1, Int = 2, UInt = 3, Float = 4, Double = 5, Bool = 6, DateTime = 7,
        Int64 = 8, Decimal = 9, Guid = 10, Undefined = 0x7FFF
    }

    public enum DataTypeEnum : short
    {
        Bool = TypeCode.Boolean,
        Byte = TypeCode.Byte,
        Int = TypeCode.Int32,
        UInt = TypeCode.UInt32,
        Int64 = TypeCode.Int64,
        Float = TypeCode.Single,
        Double = TypeCode.Double,
        Decimal = TypeCode.Decimal,
        DateTime = TypeCode.DateTime,
        String = TypeCode.String,
        Guid = 100,
        Undefined = 0x7FFF
    }

    //=====================================================================
    /// <summary>
    /// Specifies the type of match for FilterExpressions with String data types
    /// </summary>
    public enum MatchTypeEnum { UseCase, IgnoreCase }

    //=====================================================================
    /// <summary>
    /// Specifies the comparison operator to use for FilterExpressions
    /// </summary>
    public enum EqualityEnum { Equal, NotEqual, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual, In, NotIn, Like, NotLike }

    //=====================================================================
    /// <summary>
    /// Boolean operands to use to join FilterExpressions 
    /// </summary>
    public enum BoolOpEnum { And, Or }

}
