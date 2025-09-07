using Avalonia;

namespace ImageFilters;

public class TopLevelService(Visual topLevel)
{
    public Visual TopLevel { get; set; } = topLevel;
}