namespace SVSim.Database.Common;

public interface ITimeTrackedEntity
{
    /// <summary>
    /// The <see cref="DateTime"/> this entity was first added to the database.
    /// </summary>
    DateTime DateCreated { get; set; }

    /// <summary>
    /// The <see cref="DateTime"/> this entity was last updated.
    /// </summary>
    DateTime? DateUpdated { get; set; }
}
