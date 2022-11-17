using System.ComponentModel.DataAnnotations.Schema;
using Utilities.Interfaces;

namespace Utilities.Models
{
    public abstract class TrackedEntity : ITrackedEntity
    {
        [Column(Order = 0)]
        public Guid Id { get; set; }
        [Column(Order = 1)]
        public Guid? CreatedById { get; set; }
        [Column(Order = 2)]
        public DateTime CreatedOn { get; set; }
        [Column(Order = 3)]
        public Guid? UpdatedById { get; set; }
        [Column(Order = 4)]
        public DateTime? UpdatedOn { get; set; }
        public int Version { get; set; }
    }
}
