using RaylibPhysicsTutorial;

public interface IDrawer
{
    SceneSwitch Scene { get; }
    void Draw(int std);
    void Reset();
}