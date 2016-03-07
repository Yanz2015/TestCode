// <copyright file="FilterModelOrCollection.cs" company="Microsoft">
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
    /// Filter Model Or Collection.
    /// </summary>
    public class FilterModelOrCollection : FilterModelCollection
    {
        /// <summary>
        /// Gets or sets the Logic Operator of this collection.
        /// </summary>
        public override LogicOperator Operator
        {
            get
            {
                return LogicOperator.Or;
            }

            set
            {
                throw new NotSupportedException("can not set this operator");
            }
        }
    }
}
