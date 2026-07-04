namespace SVSim.Database.Models.Config;

/// <summary>
/// Marks a POCO as a top-level GameConfig section. The <see cref="Name"/> is the storage key —
/// it's the primary key in the <c>GameConfigs</c> table and the appsettings.json section name
/// under <c>"GameConfig"</c>. Renaming a class is safe; renaming the section name here is a
/// breaking change to stored data and config files.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ConfigSectionAttribute : Attribute
{
    public string Name { get; }
    public ConfigSectionAttribute(string name) => Name = name;
}
