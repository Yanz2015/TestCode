// <copyright file="IFilter.cs" company="Microsoft">
// Copyright (c) 2015 All Rights Reserved
// <author>yazha</author>
// </copyright>
namespace MCMediaIntelligence.Business.Models.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// The interface for the filter.
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Build the filter expression based on the filter infor and given information.
        /// </summary>
        /// <param name="parameter">The parameter expression.</param>
        /// <param name="propertyMap">The property map.</param>
        /// <returns>The expression built.</returns>
        Expression BuildFilterExpression(ParameterExpression parameter, Func<string, string> propertyMap = null);
    }
}
