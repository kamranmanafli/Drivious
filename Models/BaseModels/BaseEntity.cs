namespace Drivious.Models.BaseModels
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; }

        /// <summary>
        /// True when this row was soft deleted because its parent was, rather than
        /// on its own. Restoring the parent only brings these back, so a record
        /// deleted deliberately stays deleted.
        /// </summary>
        public bool DeletedByCascade { get; set; }
    }
}
