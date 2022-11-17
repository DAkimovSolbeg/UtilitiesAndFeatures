namespace Utilities.Interfaces
{
    public interface ITrackedEntity : IEntity
    {
        public Guid? CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? UpdatedById { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int Version { get; set; }
    }
}
