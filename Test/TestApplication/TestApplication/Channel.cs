namespace TestApplication
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Channel")]
    public partial class Channel
    {
        public int id { get; set; }

        public int? rank { get; set; }

        [StringLength(50)]
        public string name { get; set; }

        public int? Number { get; set; }
    }
}
