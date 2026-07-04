using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SVSim.Database.Common;

public class BaseEntity<TKey> : ITimeTrackedEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public virtual TKey Id { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.MinValue;

    public DateTime? DateUpdated { get; set; }
}
