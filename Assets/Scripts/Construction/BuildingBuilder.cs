using System.Collections.Generic;

public class BuildingBuilder
{
    private string buildingName = "Unknown Building";
    private List<string> levels = new List<string>();
    private List<WallBuilder> walls;
    private BuildingType buildingType;
    private List<FloorBuilder> floors;
    private List<RoofBuilder> roofs;
    private string filter = string.Empty;
    private static List<BuildingBuilder> builders = new List<BuildingBuilder>();
    private TechnologyLevel technologyLevel;

    public BuildingBuilder(string buildingName, string filter = "Unfiltered")
    {
        this.buildingName = buildingName;
        this.filter = filter;
        builders.Add(this);
    }

    public BuildingBuilder WithLevels(List<string> levelNames)
    {
        if (levelNames != null && levelNames.Count > 0)
        {
            this.levels = levelNames;
        }
        else
        {
            this.levels.Add("Ground Floor");
        }
        return this;
    }

    public BuildingBuilder WithFloorToFloorHeights(int startLevelID,  int endLevelID, float floorHeight)
    {

        return this;
    }

    public BuildingBuilder WithBuildingType(BuildingType buildingType)
    {
        this.buildingType = buildingType;
        return this; 
    }

    public BuildingBuilder WithWalls(List<WallBuilder> walls)
    {
        this.walls = walls;
        return this;
    }

    public BuildingBuilder WithFloors(List<FloorBuilder> floors)
    {
        this.floors = floors;
        return this;
    }

    public BuildingBuilder WithRoofs(List<RoofBuilder> roofs)
    {
        this.roofs = roofs;
        return this;
    }
}
