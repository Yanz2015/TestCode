// <copyright file="QueryModel.cs" company="Microsoft">
// Copyright (c) 2015 All Rights Reserved
// <author>yazha</author>
// </copyright>
namespace MCMediaIntelligence.Business.Models.Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// The query model to query specific news information.
    /// </summary>
    public class QueryModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryModel" /> class.
        /// </summary>
        public QueryModel()
        {
            this.Filter = new FilterModelCollection();
            this.Filter.Operator = LogicOperator.And;
            this.Range = new RangeModel();
        }

        /// <summary>
        /// Gets or sets the query range.
        /// </summary>
        public RangeModel Range { get; private set; }

        /// <summary>
        /// Gets the filters model.
        /// </summary>
        public FilterModelCollection Filter { get; private set; }
    }
}
