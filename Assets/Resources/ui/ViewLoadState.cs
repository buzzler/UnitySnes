namespace UnitySnes
{
    public class ViewLoadState : Views
    {
        public void OnTouchBack()
        {
            Frontend.OnMenuOpen("ui/menus");
        }
    }
}