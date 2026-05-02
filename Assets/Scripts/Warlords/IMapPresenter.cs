using UnityEngine;


namespace Assets.Scripts.Warlords
{
    public interface IMapPresenter
    {
        void BuildMap(WarlordsMapData mapData);
        void UpdateTile(WarlordsMapData mapData, int x, int y);
        void PlacePOI(PointOfInterest poi);
        void ClearMap();
        Vector2Int GetGridPositionFromMouse(Vector2 mouseScreenPosition);
    }
}