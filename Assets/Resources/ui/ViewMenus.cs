namespace UnitySnes
{
    public class ViewMenus : Views
    {
        public void OnTouchLoadGame()
        {
            Frontend.OnMenuLoadGame();
        }

        public void OnTouchReset()
        {
            Frontend.OnMenuReset();
        }

        public void OnTouchSaveState()
        {
            Frontend.OnMenuSaveState();
        }

        public void OnTouchSetting()
        {
            Frontend.OnMenuSetting();
        }

        public void OnTouchClose()
        {
            Frontend.OnMenuOpen("");
        }
    }
}