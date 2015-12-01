namespace DAL
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("billcat.phpap_akservers")]
    public partial class phpap_akservers
    {
        public int? ServerID { get; set; }

        [Key]
        [Column(Order = 0, TypeName = "char")]
        [StringLength(15)]
        public string ServerIP { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(45)]
        public string ServerName { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ServerPort { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ServerPrivate { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(45)]
        public string ServerPassword { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(60)]
        public string ServerMapname { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? ServerTime { get; set; }
    }
}
