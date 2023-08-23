namespace Inworld.AI.Editor
{
    public interface IEditorState
    {
        public void DrawTitle();
        public void DrawContent();
        public void DrawButtons();
        public void PostUpdate()
        {
            
        }
    }
}
