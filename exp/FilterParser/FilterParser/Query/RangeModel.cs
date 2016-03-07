// <copyright file="RangeModel.cs" company="Microsoft">
// Copyright (c) 2015 All Rights Reserved
// <author>yazha</author>
// </copyright>

namespace MCMediaIntelligence.Business.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// The range model to retrieve data.
    /// </summary>
    public class RangeModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RangeModel" /> class.
        /// </summary>
        public RangeModel()
        {
            this.Skip = 0;
            this.Size = 10;
        }

        /// <summary>
        /// Gets or sets the start range.
        /// </summary>
        public int Skip { get; set; }

        /// <summary>
        /// Gets or sets the end range.
        /// </summary>
        public int Size { get; set; }
    }
}
