using UnityEngine.UIElements;

namespace Assets.Scripts
{
    public interface IGuiProvider
    {
        VisualElement CreateGui(GuiContext ctx);
    }
}