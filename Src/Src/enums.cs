using System;
using System.Collections.Generic;

namespace FileDbNs
{
    //=====================================================================
    /// <summary>
    /// Specifies the data type for database Fields
    /// </summary>
    public enum DataTypeEnum { String, Byte, Int, UInt, Float, Double, Bool, DateTime }

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
