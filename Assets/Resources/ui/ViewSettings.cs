namespace UnitySnes
{
    public class ViewSettings : Views
    {
        public void OnTouchBack()
        {
            Frontend.OnMenuOpen("ui/menus");
        }

        public void OnTouchFilters()
        {
            Frontend.OnMenuFilter();
        }

        public void OnTouchController1P()
        {
            Frontend.OnMenuController(1);
        }
        
        public void OnTouchController2P()
        {
            Frontend.OnMenuController(2);
        }
    }
}