

using System.Collections.Generic;
using System.Numerics;

public class WallBuilder
{
    private List<StudBuilder> framing = new List<StudBuilder>();
    private List<OpeningBuilder> openings = new List<OpeningBuilder>();
    private Vector3 start;
    private Vector3 end;
    private bool exterior = false;
    private BuildingMaterial finishMaterial;
    private float studSpacing;
    private int onLevel;
    private bool loadBearing;

    public WallBuilder(Vector3 start, Vector3 end)
    {
        this.start = start;
        this.end = end;
    }

    public WallBuilder WithFraming(List<StudBuilder> framing)
    {
        this.framing = framing;
        return this;
    }

    public WallBuilder WithOpenings(List<OpeningBuilder> openings)
    {
        this.openings = openings;
        return this;
    }

    public WallBuilder WithFinish(BuildingMaterial finishMaterial)
    {
        this.finishMaterial = finishMaterial;
        return this;
    }

    public WallBuilder WithStudSpacing(float studSpacing)
    {
        this.studSpacing = studSpacing;
        return this;
    }

    public WallBuilder OnLevel(int levelID)
    {
        this.onLevel = levelID;
        return this;
    }

    public WallBuilder AsExterior()
    {
        this.exterior = true;
        return this;
    }

    public WallBuilder AsLoadBearing()
    {
        this.loadBearing = true;
        return this;
    }

    public WallContext BuildWall()
    {

        return new WallContext();
    } 
}