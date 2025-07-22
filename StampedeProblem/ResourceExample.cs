namespace StampedeProblem;

/// <summary>
/// Represents an example resource that can be used in the Stampede problem.
/// </summary>
public class ResourceExample
{
    /// <summary>
    /// Gets or sets the unique identifier for the resource.
    /// </summary>
    public Guid Guid { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name of the resource.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public override string ToString() => $"{{\"id\":{Guid}, {Environment.NewLine} \"name\":{Name}}}";
}

/// <summary>
/// Represents static data for resources in the Stampede problem.
/// </summary>
public class ResourceStaticData
{
    /// <summary>
    /// An array of resource names for the example.
    /// </summary>
    public static string[] ResourceNames = [
        "Apple",
        "Banana",
        "Cherry",
        "Orange",
        "Grape",
        "Strawberry",
        "Blueberry",
        "Pineapple",
        "Mango",
        "Peach"
        ];

}