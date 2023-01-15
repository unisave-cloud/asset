namespace Unisave.Editor.Windows.Main
{
    public interface ITabContentController
    {
        public void OnCreateGUI();
        
        public void OnObserveExternalState();

        public void OnWriteExternalState();
    }
}