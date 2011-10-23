using System;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace FileDbNs
{

    //=====================================================================
    /// <summary>
    /// Use this class for single field searches.
    /// </summary>
    /// 
    public class FilterExpression
    {
        /// <summary>
        /// Create a FilterExpression with the indicated values
        /// </summary>
        /// <param name="fieldName">The name of the Field to filter on</param>
        /// <param name="searchVal">The Field value to filter on</param>
        /// <param name="equality">The Equality operator to use in the value comparison</param>
        /// 
        public FilterExpression( string fieldName, object searchVal, EqualityEnum equality )
            : this( fieldName, searchVal, equality, MatchTypeEnum.UseCase )
        {
        }

        /// <summary>
        /// Create a FilterExpression with the indicated values
        /// </summary>
        /// <param name="fieldName">The name of the Field to filter on</param>
        /// <param name="searchVal">The Field value to filter on</param>
        /// <param name="equality">The Equality operator to use in the value comparison</param>
        /// <param name="matchType">The match type, eg. MatchType.Exact</param>
        /// 
        public FilterExpression( string fieldName, object searchVal, EqualityEnum equality, MatchTypeEnum matchType )
        {
            FieldName = fieldName;
            SearchVal = searchVal;
            MatchType = matchType;
            Equality = equality;
        }

        internal BoolOpEnum BoolOp { get; set; }

        public string FieldName { get; set; }
        public object SearchVal { get; set; }
        public MatchTypeEnum MatchType { get; set; }
        public EqualityEnum Equality { get; set; }

        /// <summary>
        /// Parse the expression string to create a FilterExpressionGroup representing a simple expression.
        /// </summary>
        /// <param name="expression">The string expression.  Example: LastName = 'Fuller'</param>
        /// <returns>A new FilterExpression representing the simple expression</returns>
        /// 
        public static FilterExpression Parse( string expression )
        {
            FilterExpression fexp = null;
            FilterExpressionGroup fexpg = FilterExpressionGroup.Parse( expression );
            if( fexpg != null )
                fexp = fexpg.Expressions[0] as FilterExpression;

            return fexp;
        }

        /// <summary>
        /// Create a FilterExpression of type "IN". This type is a HashSet of the values which will be used
        /// to filter the query.
        /// </summary>
        /// <param name="fieldName">The name of the Field which will be used in the FilterExpressions</param>
        /// <param name="table">A Table to use to build the IN FilterExpressions</param>
        /// <param name="fieldNameInTable">The name of the Field in the Table which holds the value to be used to build the IN FilterExpressions</param>
        /// <returns>A new FilterExpression</returns>
        /// 
        public static FilterExpression CreateInExpressionFromTable( string fieldName, Table table, string fieldNameInTable )
        {
            if( !table.Fields.ContainsKey( fieldNameInTable ) )
                throw new Exception( string.Format( "Field {0} is not in the table", fieldNameInTable ) );

            FilterExpression fexp = null;

            var hashSet = new HashSet<object>();

            foreach( Record record in table )
            {
                object val = record[fieldNameInTable];
                hashSet.Add( val );
            }

            fexp = new FilterExpression( fieldName, hashSet, EqualityEnum.In );

            return fexp;
        }

        /// <summary>
        /// Create a FilterExpression of type "IN". This type is a HashSet of the values which will be used
        /// to filter the query.
        /// </summary>
        /// <param name="fieldName">The name of the Field which will be used in the FilterExpressions</param>
        /// <param name="list">A List of custom objects to use to build the IN FilterExpression</param>
        /// <param name="propertyName">The name of the Property of the custom class which holds the value to be 
        /// used to build the IN FilterExpression</param>
        /// <returns>A new FilterExpression</returns>
        /// 
        public static FilterExpression CreateInExpressionFromTable<T>( string fieldName, IList<T> list, string propertyName )
        {
            FilterExpression fexp = null;

            var hashSet = new HashSet<object>();

            PropertyInfo prop = typeof( T ).GetProperty( propertyName );

            if( prop == null )
                throw new Exception( string.Format( "Field {0} is not a property", propertyName ) );

            foreach( T obj in list )
            {
                object val = prop.GetValue( obj, null );
                hashSet.Add( val );
            }

            fexp = new FilterExpression( fieldName, hashSet, EqualityEnum.In );

            return fexp;
        }
    }

    //=====================================================================
    /// <summary>
    /// Use this class to group FilterExpression and FilterExpressionGroup to form compound search expressions.
    /// All expressions in the group will be evaluated by the same boolean And/Or operation.  Use multiple
    /// FilterExpressionGroups to form any combination of And/Or logic.
    /// </summary>
    /// 
    public class FilterExpressionGroup
    {
        List<object> _expressions = new List<object>();

        public FilterExpressionGroup()
        {
        }
        
        internal BoolOpEnum BoolOp { get; set; }

        public void Add( BoolOpEnum boolOp, object searchExpressionOrGroup )
        {
            // set the BoolOp into the expression or group
            if( searchExpressionOrGroup is FilterExpression )
                ((FilterExpression) searchExpressionOrGroup).BoolOp = boolOp;
            else if( searchExpressionOrGroup is FilterExpressionGroup )
                ((FilterExpressionGroup) searchExpressionOrGroup).BoolOp = boolOp;

            _expressions.Add( searchExpressionOrGroup );
        }

        public List<object> Expressions
        {
            get { return _expressions; }
        }

        /// <summary>
        /// Parse the expression string to create a FilterExpressionGroup representing a compound expression.
        /// </summary>
        /// <param name="expression">The string compound expression.  Example: (FirstName ~= 'andrew' OR FirstName ~= 'nancy') AND LastName = 'Fuller'</param>
        /// <returns>A new FilterExpressionGroup representing the compound expression</returns>
        /// 
        public static FilterExpressionGroup Parse( string expression )
        {
            FilterExpressionGroup srchExpGrp = null;

            if( expression == null )
                return srchExpGrp;

            srchExpGrp = new FilterExpressionGroup();

            int n = 0;
            parseExpression( expression, ref n, srchExpGrp );

            while( true )
            {
                if( srchExpGrp.Expressions.Count != 1 )
                    break;

                // if there is only one Group in a Group, then return that one

                FilterExpressionGroup grp = srchExpGrp.Expressions[0] as FilterExpressionGroup;
                if( grp == null )
                    break; // its an Expression, NOT a group

                // remove this group from its parent
                srchExpGrp.Expressions.Clear();
                srchExpGrp = grp;
            }

            return srchExpGrp;
        }

        internal enum ParseState { Left, Right, CompareOp, BoolOp }

        static void parseExpression( string filter, ref int pos, FilterExpressionGroup parentSrchExpGrp )
        {
            ParseState state = ParseState.Left;
            bool hasBracket = false,
                 inString = false,
                 not = false;
            string fieldName = null;
            object searchVal = null;
            var sbTemp = new StringBuilder();
            EqualityEnum comparisonOp = EqualityEnum.Equal;
            MatchTypeEnum matchType = MatchTypeEnum.UseCase;
            BoolOpEnum curBoolOp = BoolOpEnum.And;
            int startPos = pos;

            // skip past any leading spaces
            while( pos < filter.Length && char.IsWhiteSpace( filter[pos] ) ) pos++;

            for( ; pos < filter.Length; )
            {
                //////////////////////////////////////////////////////////
                #region Left
                if( state == ParseState.Left )
                {                    
                    if( filter[pos] == '[' )  // field names with ' ' in them must be wrapped with brackets
                    {
                        hasBracket = true;
                        pos++;
                        startPos = pos;
                    }
                    
                    if( hasBracket )
                    {
                        // look for ending bracket
                        if( filter[pos] == ']' )
                        {
                            fieldName = filter.Substring( startPos, pos - startPos ).Trim();
                            pos++; // skip past bracket
                        }
                    }
                    else // no bracket - look for non-alpha
                    {
                        if( filter[pos] == '(' )
                        {
                            // start of a new FilterExpressionGroup
                            pos++;
                            var newSrchExpGrp = new FilterExpressionGroup();
                            parentSrchExpGrp.Add( curBoolOp, newSrchExpGrp );
                            parseExpression( filter, ref pos, newSrchExpGrp );
                            state = ParseState.BoolOp;
                        }
                        else if( char.IsWhiteSpace( filter[pos] ) ||
                               (!char.IsLetterOrDigit( filter[pos] ) && filter[pos] != '_' && filter[pos] != '~') ) // field names with spaces in them must be wrapped with brackets
                        {
                            fieldName = filter.Substring( startPos, pos - startPos ).Trim();
                        }
                    }

                    if( fieldName != null )
                    {
                        if( fieldName[0] == '~' )
                        {
                            fieldName = fieldName.Substring( 1 );
                            matchType = MatchTypeEnum.IgnoreCase;
                        }
                        state = ParseState.CompareOp;
                    }
                    else
                    {
                        pos++;
                    }
                }
                #endregion Left
                //////////////////////////////////////////////////////////
                #region CompareOp
                else if( state == ParseState.CompareOp )
                {
                    // skip whitespace
                    while( pos < filter.Length && char.IsWhiteSpace( filter[pos] ) ) pos++;

                    if( char.IsLetter( filter[pos] ) ) // LIKE
                    {
                        // should be LIKE or IN
                        if( pos+4 >= filter.Length )
                            throw new FileDbException( string.Format( FileDbException.InvalidFilterConstruct, filter.Substring( pos ) ),
                                FileDbExceptionsEnum.InvalidFilterConstruct );

                        // NOT
                        if( char.ToUpper(filter[pos]) == 'N' && char.ToUpper(filter[pos + 1]) == 'O' && char.ToUpper(filter[pos + 2]) == 'T' && char.IsWhiteSpace( filter[pos+3] ) )
                        {
                            pos += 3;
                            not = true;
                            continue;
                        }
                        // IN
                        else if( char.ToUpper(filter[pos]) == 'I' && char.ToUpper(filter[pos + 1]) == 'N' && (char.IsWhiteSpace( filter[pos + 2] ) || filter[pos + 2] == '(') )
                        {
                            pos += 2;
                            if( char.IsWhiteSpace( filter[pos] ) ) // skip whitespace
                            {
                                while( pos < filter.Length && char.IsWhiteSpace( filter[pos] ) ) pos++;
                                if( filter[pos] != '(' )
                                    throw new FileDbException( string.Format( FileDbException.InvalidFilterConstruct, filter.Substring( pos - 4 ) ),
                                        FileDbExceptionsEnum.InvalidFilterConstruct );
                            }
                            comparisonOp = not ? EqualityEnum.NotIn : EqualityEnum.In;
                        }
                        // LIKE
                        else if( char.ToUpper( filter[pos] ) == 'L' && char.ToUpper( filter[pos + 1] ) == 'I' && char.ToUpper( filter[pos + 2] ) == 'K' && char.ToUpper( filter[pos + 3] ) == 'E' && char.IsWhiteSpace( filter[pos + 4] ) )
                        {
                            pos += 4;
                            comparisonOp = not? EqualityEnum.NotLike : EqualityEnum.Like;
                        }
                        else
                            throw new FileDbException( string.Format( FileDbException.InvalidFilterConstruct, filter.Substring( pos - 4 ) ),
                                FileDbExceptionsEnum.InvalidFilterConstruct );
                    }
                    /* deprecated
                    else if( filter[pos] == '~' ) // ~=
                    {
                        matchType = MatchType.NoCase;
                        if( ++pos >= filter.Length )
                            throw new FileDbException( string.Format( FileDbException.InvalidFilterConstruct, filter.Substring( pos ) ),
                                FileDbExceptions.InvalidFilterConstruct );
                        // next char must be =
                        if( filter[pos] != '=' )
                            throw new FileDbException( string.Format( FileDbException.InvalidFilterConstruct, filter.Substring( pos ) ),
                                FileDbExceptions.InvalidFilterConstruct );
                        comparisonOp = Equality.Equal;
                    }*/
                    else if( filter[pos] == '!' ) // !=
                    {
                        if( ++pos >= filter.Length )
                            throw new FileDbException( string.Format( FileDbException.InvalidFilterConstruct, filter.Substring( pos ) ),
                                FileDbExceptionsEnum.InvalidFilterConstruct );

                        // next char must be =
                        if( filter[pos] != '=' )
                            throw new FileDbException( string.Format( FileDbException.InvalidFilterConstruct, filter.Substring( pos ) ),
                                FileDbExceptionsEnum.InvalidFilterConstruct );

                        comparisonOp = EqualityEnum.NotEqual;
                    }
                    else if( filter[pos] == '=' )
                    {
                        comparisonOp = EqualityEnum.Equal;
                    }
                    else if( filter[pos] == '<' ) // <, <= or <>
                    {
                        if( pos + 1 >= filter.Length )
                            throw new FileDbException( string.Format( FileDbException.InvalidFilterConstruct, filter.Substring( pos ) ),
                                FileDbExceptionsEnum.InvalidFilterConstruct );

                        if( filter[pos + 1] == '>' )
                        {
                            pos++;
                            comparisonOp = EqualityEnum.NotEqual;
                        }
                        else if( filter[pos + 1] == '=' )
                        {
                            pos++;
                            comparisonOp = EqualityEnum.LessThanOrEqual;
                        }
                        else
                            comparisonOp = EqualityEnum.LessThan;
                    }
                    else if( filter[pos] == '>' ) // > or >=
                    {
                        if( pos + 1 >= filter.Length )
                            throw new FileDbException( string.Format( FileDbException.InvalidFilterConstruct, filter.Substring( pos ) ),
                                FileDbExceptionsEnum.InvalidFilterConstruct );

                        if( filter[pos + 1] == '=' )
                        {
                            pos++;
                            comparisonOp = EqualityEnum.GreaterThanOrEqual;
                        }
                        else
                            comparisonOp = EqualityEnum.GreaterThan;
                    }
                    else
                    {
                        throw new FileDbException( string.Format( FileDbException.InvalidFilterConstruct, filter.Substring( pos ) ),
                            FileDbExceptionsEnum.InvalidFilterConstruct );
                    }
                    pos++;
                    state = ParseState.Right;
                }
                #endregion CompareOp
                //////////////////////////////////////////////////////////
                #region Right
                else if( state == ParseState.Right )
                {
                    if( comparisonOp == EqualityEnum.In || comparisonOp == EqualityEnum.NotIn )
                    {
                        // skip whitespace
                        while( pos < filter.Length && char.IsWhiteSpace( filter[pos] ) ) pos++;

                        // filter[pos] should look like this now: (val1, val2, val3)
                        // or like this: ('val1', 'val2', 'val3')

                        if( filter[pos] == '(' )
                            pos++;

                        // find the end
                        int endPos = pos;
                        
                        while( endPos < filter.Length && filter[endPos] != ')' )
                        {
                            endPos++;
                        }
                        if( endPos >= filter.Length )
                            throw new FileDbException( string.Format( FileDbException.InvalidFilterConstruct, filter.Substring( pos ) ),
                                FileDbExceptionsEnum.InvalidFilterConstruct );

                        string inVals = filter.Substring( pos, endPos - pos );

                        searchVal = parseInVals( inVals, matchType );

                        pos = endPos;
                    }
                    else
                    {
                        if( !inString )
                        {
                            // skip whitespace only if we haven't found anything yet
                            if( sbTemp.Length == 0 )
                                while( pos < filter.Length && char.IsWhiteSpace( filter[pos] ) ) pos++;

                            // look for end of ExpressionGroup
                            if( sbTemp.Length > 0 && (filter[pos] == ')' || char.IsWhiteSpace( filter[pos] )) )
                            {
                                // Expression completed
                                searchVal = sbTemp.ToString();
                                sbTemp.Length = 0;
                                var srchExp = new FilterExpression( fieldName, searchVal, comparisonOp, matchType );
                                parentSrchExpGrp.Add( curBoolOp, srchExp );
                                if( filter[pos] == ')' )
                                    return;
                                fieldName = null;
                                state = ParseState.BoolOp;
                            }
                            else if( sbTemp.Length == 0 && filter[pos] == '\'' )
                            {
                                // just starting to get the value
                                inString = /*isString=*/ true;
                            }
                            else
                                sbTemp.Append( filter[pos] );
                        }
                        else // inString == true
                        {
                            if( filter[pos] == '\'' )
                            {
                                Debug.Assert( sbTemp.Length > 0 );

                                // if the next char is NOT another ' (escaped) then the string is completed
                                if( (pos + 1 < filter.Length) && filter[pos + 1] == '\'' )
                                    pos++;
                                else
                                {
                                    inString = false;
                                    searchVal = sbTemp.ToString();
                                    sbTemp.Length = 0;
                                    var srchExp = new FilterExpression( fieldName, searchVal, comparisonOp, matchType );
                                    parentSrchExpGrp.Add( curBoolOp, srchExp );
                                    fieldName = null;
                                    state = ParseState.BoolOp;
                                    goto Advance;
                                }
                            }
                            sbTemp.Append( filter[pos] );
                        }
                    }
                Advance:
                    // advance
                    pos++;
                }
                #endregion Right
                //////////////////////////////////////////////////////////
                #region Next
                else // if( state == ParseState.BoolOp )
                {
                    Debug.Assert( state == ParseState.BoolOp );

                    if( sbTemp.Length == 0 )
                        // skip whitespace
                        while( pos < filter.Length && char.IsWhiteSpace( filter[pos] ) ) pos++;

                    if( filter[pos] == ')' )
                        return; // we must be finished

                    if( char.IsWhiteSpace( filter[pos] ) )
                    {
                        // we must be finished
                        if( sbTemp.Length == 0 )
                            throw new FileDbException( string.Format( FileDbException.InvalidFilterConstruct, filter.Substring( pos ) ),
                                FileDbExceptionsEnum.InvalidFilterConstruct );

                        string sOp = sbTemp.ToString();
                        sbTemp.Length = 0;

                        if( string.Compare( sOp, "AND", StringComparison.OrdinalIgnoreCase ) == 0 )
                        {
                            curBoolOp = BoolOpEnum.And;
                        }
                        else if( string.Compare( sOp, "OR", StringComparison.OrdinalIgnoreCase ) == 0 )
                        {
                            curBoolOp = BoolOpEnum.Or;
                        }
                        else
                        {
                            throw new FileDbException( string.Format( FileDbException.InvalidFilterConstruct, filter.Substring( pos ) ),
                                FileDbExceptionsEnum.InvalidFilterConstruct );
                        }

                        state = ParseState.Left; // start over on next expression

                        // skip whitespace
                        while( pos < filter.Length && char.IsWhiteSpace( filter[pos] ) ) pos++;

                        // reset vars
                        startPos = pos;
                        hasBracket = false;
                    }
                    else
                    {
                        sbTemp.Append( filter[pos] );
                        pos++;
                    }
                }
                #endregion Next

            } // for...

            // did we just complete an Expression?
            if( state == ParseState.Right )
            {
                if( comparisonOp != EqualityEnum.In && comparisonOp != EqualityEnum.NotIn )
                {
                    searchVal = sbTemp.ToString();
                    sbTemp.Length = 0;
                }
                var srchExp = new FilterExpression( fieldName, searchVal, comparisonOp, matchType );
                parentSrchExpGrp.Add( curBoolOp, srchExp );
            }
        }

        // Note: we will parse the InClause values but we don't know the Field type at this time
        // so we will add the values as strings and convert them later (see FileDb.evaluate)
        //
        private static HashSet<object> parseInVals( string inVals, MatchTypeEnum matchType )
        {
            var hashSet = new HashSet<object>();

            bool inString = false,
                 isStringVal = false; // used to know if we must trim

            StringBuilder sb = new StringBuilder(100);

            // skip whitespace
            int pos = 0;
            while( pos < inVals.Length && char.IsWhiteSpace( inVals[pos] ) ) pos++;

            for( ; pos < inVals.Length; pos++ )
            {
                char ch = inVals[pos];

                if( ch == '\'' )
                {
                    if( inString )
                    {
                        // is this an escaped single quote?
                        if( pos < inVals.Length - 1 && inVals[pos + 1] == '\'' )
                        {
                            // it is escaped - skip it and add
                            pos++;
                            goto AddChar;
                        }
                        
                        // not escaped - means end of string
                        inString = false;
                    }
                    else
                    {
                        inString = isStringVal = true;
                    }

                    continue;
                }
                else if( ch == ',' )
                {
                    if( !inString )
                    {
                        // end of current value
                        string val = sb.ToString();
                        if( isStringVal )
                        {
                            if( matchType == MatchTypeEnum.IgnoreCase )
                                val = val.ToUpper();
                        }
                        else // we can trim non-string values
                            val = val.Trim();

                        hashSet.Add( val );
                        sb.Length = 0;

                        continue;
                    }
                }

            AddChar:
                // only add the char if we should - a space should only be added if in a string
                if( !(ch == ' ' && !inString) )
                    sb.Append( ch );
            }

            // add the last one
            if( sb.Length > 0 )
            {
                string val = sb.ToString();
                if( isStringVal )
                {
                    if( matchType == MatchTypeEnum.IgnoreCase )
                        val = val.ToUpper();
                }
                else // we can trim non-string values
                    val = val.Trim();

                hashSet.Add( val );
            }

            return hashSet;
        }

    }

#if WINDOWS_PHONE
    public class HashSet<T> : ICollection<T>
    {
        private Dictionary<T, byte> _dict;

        public HashSet()
        {
            _dict = new Dictionary<T, byte>();
        }

        // Methods
        public void Add(T item)
        {
            // We don't care for the value in dictionary, Keys matter.
            _dict.Add(item, 0);
        }

        public void Clear()
        {
            _dict.Clear();
        }

        public bool Contains(T item)
        {
            return _dict.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            return _dict.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        // Properties
        public int Count
        {
            get { return _dict.Keys.Count; } 
        }

        public bool IsReadOnly 
        {
            get { return false; }
        }
    }

#endif

}
