// <copyright file="FilterModelAndCollection.cs" company="Microsoft">
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
    /// The filter model and collection.
    /// </summary>
    public class FilterModelAndCollection : FilterModelCollection
    {
        /// <summary>
        /// Gets or sets the logic operator for the collection.
        /// </summary>
        public override LogicOperator Operator
        {
            get
            {
                return LogicOperator.And;
            }

            set
            {
                throw new NotSupportedException("can not set this operator");
            }
        }
    }
}
