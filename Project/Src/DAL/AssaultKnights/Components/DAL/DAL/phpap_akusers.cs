namespace DAL
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("billcat.phpap_akusers")]
    public partial class phpap_akusers
    {
        [Key]
        public int UserID { get; set; }

        [Column(TypeName = "text")]
        [Required]
        [StringLength(65535)]
        public string Username { get; set; }

        [Required]
        [StringLength(16)]
        public string Password { get; set; }

        [Column(TypeName = "text")]
        [Required]
        [StringLength(65535)]
        public string Email { get; set; }

        [StringLength(32)]
        public string Name { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime JoinDate { get; set; }

        public int Money { get; set; }

        public int UserLvl { get; set; }
    }
}
