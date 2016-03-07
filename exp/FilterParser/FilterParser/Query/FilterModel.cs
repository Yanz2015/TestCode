// <copyright file="FilterModel.cs" company="Microsoft">
// Copyright (c) 2015 All Rights Reserved
// <author>yazha</author>
// </copyright>
namespace MCMediaIntelligence.Business.Models.Query
{
    using Query;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Linq.Expressions;

    /// <summary>
    /// Filter Model
    /// </summary>
    public class FilterModel : IFilter
    {
        public FilterModel()
        {
            this.Operator = "eq";
        }

        /// <summary>
        /// Gets or sets the name for the filter field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value if single for value compare.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the type for the query field.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the operator for the field and value operation
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// Build the filter expression used for query.
        /// </summary>
        /// <param name="parameter">The type for query.</param>
        /// <param name="propertyMap">The property map method to fectch the filter property.</param>
        /// <returns>The filter expression used for query.</returns>
        public Expression BuildFilterExpression(ParameterExpression parameter, Func<string, string> propertyMap)
        {
            var propertyName = propertyMap(this.Name);
            Expression result = null;
            Expression left = Expression.Property(parameter, propertyName);
            Expression right = null;
            if (this.Value == null)
            {
                right = Expression.Constant(null);
            }
            else
            {
                var propertyType = parameter.Type.GetProperty(propertyName).PropertyType;
                var val = Convert.ChangeType(this.Value, propertyType);
                right = Expression.Constant(val, propertyType);
            }

            switch (this.Operator)
            {
                case ">":
                case "gt":
                    result = Expression.GreaterThan(left, right);
                    break;
                case "<":
                case "lt":
                    result = Expression.LessThan(left, right);
                    break;
                case ">=":
                case "ge":
                    result = Expression.GreaterThanOrEqual(left, right);
                    break;
                case "<=":
                case "le":
                    result = Expression.LessThanOrEqual(left, right);
                    break;
                case "=":
                case "eq":
                    result = Expression.Equal(left, right);
                    break;
                case "ne":
                case "<>":
                case "!=":
                    result = Expression.NotEqual(left, right);
                    break;
                default:
                    throw new NotSupportedException("The given parameter does not support ");
            }

            return result;
        }
    }
}
