using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

public class BuildingContext : MonoBehaviour, IStateContext
{
    public bool IsValid { get; set; } = false;
    public string Name { get; set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
