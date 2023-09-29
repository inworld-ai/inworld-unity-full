namespace Inworld.AI.Editor
{
    public interface IEditorState
    {
        public void OnOpenWindow();
        public void DrawTitle();
        public void DrawContent();
        public void DrawButtons();
        public void OnExit();
        public void OnEnter();
        public void PostUpdate();

    }
}
