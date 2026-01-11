using UnityEngine;

public class StudBuilder
{
    //A stud's dimensions are based upon how it would stabilize, i.e. longest dimension along the ground plane
    private float width;
    private float length;
    private float depth;
    private BuildingMaterial studMaterial;
    private bool vertical = false;
    private float cutLength;
    private static int studCount = 0;

    public StudBuilder(BuildingMaterial studMaterial, float width, float length, float depth)
    {
        this.width = width;
        this.length = length;
        this.depth = depth;
        this.studMaterial = studMaterial;
    }

    public StudBuilder(BuildingMaterial studMaterial)
    { 
       this.studMaterial = studMaterial;
        this.length = 3.5f; //Default length in feet
        this.width = 1.5f;  //Default width in feet
        this.depth = 0.5f;  //Default depth in feet
    }

    public StudBuilder()
    {
        this.length = 3.5f; //Default length in feet
        this.width = 1.5f;  //Default width in feet
        this.depth = 0.5f;  //Default depth in feet
        this.studMaterial = new BuildingMaterial(); //Default material
    }

    public StudBuilder WithMaterial(BuildingMaterial studMaterial)
    {
        this.studMaterial = studMaterial;
        return this;
    }

    public StudBuilder WithLength(float length)
    {
        this.length = length;
        return this;
    }

    public StudBuilder WithDepth(float depth)
    {
        this.depth = depth;
        return this;
    }
    public StudBuilder WithWidth(float width)
    {
        this.width = width;
        return this;
    }

    public StudBuilder AsVertical()
    {
        this.vertical = true;
        return this;
    }

    public StudBuilder AsHorizontal()
    {
        this.vertical = false;
        return this;
    }

    public StudBuilder WithCut(float cutLength)
    {
        this.cutLength = cutLength;
        return this;
    }

    public StudContext BuildStud()
    {
        //Implementation to build and return a StudContext
        return new GameObject($"Stud{studCount++}").AddComponent<StudContext>();
    }
}