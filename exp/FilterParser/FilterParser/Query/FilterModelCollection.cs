// <copyright file="FilterModelCollection.cs" company="Microsoft">
// Copyright (c) 2015 All Rights Reserved
// <author>yazha</author>
// </copyright>
namespace MCMediaIntelligence.Business.Models.Query
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// The filter model collection.
    /// </summary>
    public class FilterModelCollection : IEnumerable<IFilter> , IFilter
    {
        /// <summary>
        /// Initialize the instances of 
        /// </summary>
        public FilterModelCollection()
        {
            this.Filters = new List<IFilter>();
        }

        /// <summary>
        /// Gets the filters in this collection.
        /// </summary>
        public List<IFilter> Filters { get; private set; }

        /// <summary>
        /// Gets or sets the logic operator for this collection/
        /// </summary>
        public virtual LogicOperator Operator { get; set; }

        /// <summary>
        /// Build the filter expression based on this collection.
        /// </summary>
        /// <param name="parameter">The parameter expression.</param>
        /// <param name="propertyMap">The delegate to get the property mapping.</param>
        /// <returns>The filter expression.</returns>
        public Expression BuildFilterExpression(ParameterExpression parameter, Func<string, string> propertyMap)
        {
            var nonNullFilters = Filters.Where(i => i != null).ToList();

            if (nonNullFilters.Count > 1)
            {
                var left = nonNullFilters[0].BuildFilterExpression(parameter, propertyMap);
                for (int i = 1; i < nonNullFilters.Count; i++)
                {
                    var right = nonNullFilters[i].BuildFilterExpression(parameter, propertyMap);
                    if (this.Operator == LogicOperator.And)
                    {
                        left = Expression.And(left, right);
                    }
                    else if (this.Operator == LogicOperator.Or)
                    {
                        left = Expression.Or(left, right);
                    }
                }

                return left;
            }
            else if (nonNullFilters.Count == 1)
            {
                return nonNullFilters[0].BuildFilterExpression(parameter, propertyMap);
            }
            else
            {
                Expression left = Expression.Constant(1);
                Expression right = Expression.Constant(1);
                return Expression.Equal(left, right);
            }
        }

        /// <summary>
        /// Get the emuerator of the collection.
        /// </summary>
        /// <returns>Return the emurator of the collection.</returns>
        public IEnumerator<IFilter> GetEnumerator()
        {
            return ((IEnumerable<IFilter>)Filters).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<IFilter>)Filters).GetEnumerator();
        }
    }
}
