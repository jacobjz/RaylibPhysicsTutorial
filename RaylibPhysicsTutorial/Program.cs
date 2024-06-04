using MathNet.Numerics.Distributions;
using Raylib_cs;
using RaylibPhysicsTutorial;
using System.Numerics;
using System.Text.RegularExpressions;

internal class Program
{
    internal const int fps = 30;

    internal const int width = 1600;

    internal const int height = 800;

    public static void Main()
    {

        Raylib.InitWindow(width, height, "Kinematics");


        int sliderValue = 0;

        int nameHeight = 50;

        int fontSize = 20;

        Camera2D camera = new() { Zoom = 1f };

        Raylib.SetTargetFPS(fps);

        BarsScene bars = new()
        {
            Heights = new int[255],
            Colors = new Color[255],
            Random = new Random()
        };

        bars.InitColors();

        List<IDrawer> scenes = new()
        {
            new Blank(),
            bars,
            new GaussScene(),
            new GaussCircle(),
            new GaussianWalker(),
            new CurveDrawer(),
            new PerlinWalker(),
        };
        SceneSwitch currentScene = SceneSwitch.Start;
        IDrawer currentSceneDrawer = scenes.First(x => x.Scene == currentScene);

        while (!Raylib.WindowShouldClose())

        {
            Raylib.BeginDrawing();

            Raylib.ClearBackground(Color.White);

            Raylib.BeginMode2D(camera);

            Raylib.DrawText($"slider value: {sliderValue}", 10, 20, 20, Color.Black);

            if (Raylib.IsKeyDown(KeyboardKey.LeftControl))
            {
                if (Raylib.IsKeyPressed(KeyboardKey.R))
                {
                    currentSceneDrawer.Reset();
                }

                if (Raylib.IsKeyPressed(KeyboardKey.A))
                {
                    scenes.ForEach(x =>  x.Reset());
                }

                if (Raylib.IsKeyPressed(KeyboardKey.X))
                {                    
                    if ((int)currentScene == Enum.GetNames(typeof(SceneSwitch)).Length - 1)
                    {
                        currentScene = SceneSwitch.Start;
                    }
                    else
                    {
                        currentScene += 1;
                    }
                    currentSceneDrawer = scenes.First(x => x.Scene == currentScene);
                }
            }

            if (Raylib.IsKeyDown(KeyboardKey.Left))            
                sliderValue = Math.Max(--sliderValue, 0);
            
            if (Raylib.IsKeyDown(KeyboardKey.Right))            
                sliderValue = Math.Min(++sliderValue, 100);


            currentSceneDrawer.Draw(sliderValue);
            Raylib.DrawText(GetClassName(currentSceneDrawer), 50, 50, 20, Color.Black);

            Raylib.EndMode2D();

            Raylib.EndDrawing();

        }

        Raylib.CloseWindow();
    }

    static string GetClassName(object source) 
    {
        return string.Join(" ", Regex.Split(source.GetType().Name, @"(?<!^)(?=[A-Z])"));
    }
}

internal class Blank : IDrawer
{
    public SceneSwitch Scene => SceneSwitch.Start;

    public void Draw(int std)
    {
    }

    public void Reset()
    {
    }
}

internal class PerlinWalker : IDrawer
{
    public LinkedList<Vector2> walks = new();

    private Vector2 last = new Vector2(800, 400);

    public SceneSwitch Scene { get => SceneSwitch.PerlinWalker; }

    public PerlinWalker()
    {

        walks.AddFirst(Vector2.Zero);
    }
    
    public void Draw(int std)
    {

        var xmov = (int)new Normal(0, std).Sample();
        var ymov = (int)new Normal(0, std).Sample();
        var newPixel = new Vector2(xmov, ymov) + last;
        last = newPixel;
        walks.AddFirst(newPixel);

        if (walks.Count > 400)        
            walks.RemoveLast();
        
        int counter = 0;
        foreach (var walk in walks.Reverse())
        {
            counter++;
            Raylib.DrawCircleV(walk, 20, new Color(0, 0, 0, 50));
        }
    }

    public void Reset()
    {
        walks = new();
        last = new Vector2(800, 400);
    }
}

internal class CurveDrawer : IDrawer
{
    private enum Interpolations
    {
        None,
        Linear,
        LinearM
    }

    public LinkedList<int> heights = new();
    public SceneSwitch Scene { get => SceneSwitch.CurveDrawer; }

    private Interpolations Interpolate { get; set; }

    public void Draw(int std)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.I))
        {
            if ((int)Interpolate == Enum.GetNames(typeof(Interpolations)).Length - 1)
            {
                Interpolate = Interpolations.None;
            }
            else
            {
                Interpolate += 1;
            }
        }

        heights.AddLast((int)new Normal(0, std).Sample());

        if (heights.Count > 160)        
            heights.RemoveFirst();
        

        int counter = 0;
        var previous = 0;
        foreach (var height in heights)
        {
            counter+=10;
            Raylib.DrawPixel(counter, 400 + height, new Color(0, 0, 0, 255));

            if (Interpolate == Interpolations.Linear)
            {
                Raylib.DrawLine(counter - 10, 400 + previous, counter, 400 + height, Color.Black);
            }

            if (Interpolate == Interpolations.LinearM)
            {
                double density = 10;
                for (double i = 0; i < density; i++)
                {
                    var mu = i / density;
                    Raylib.DrawPixel(counter - 10 + (int)i, LinearInterpolate(400 + previous, 400 + height, mu), Color.Black);
                }
            }

            previous = height;
        }
    }

    private int LinearInterpolate(double y1, double y2, double mu)
    {
        return (int)(y1 * (1 - mu) + y2 * mu);
    }

    public void Reset()
    {
        heights = new();
    }
}

internal class GaussScene : IDrawer
{
    public List<int> circles = new();

    public SceneSwitch Scene { get => SceneSwitch.Gaussian; }

    public void Draw(int std)
    {

        circles.Add((int)new Normal(800, std).Sample());

        foreach (var randomGaussianValue in circles)

        {

            Raylib.DrawCircle(randomGaussianValue, 300, 20, new Color(0, 0, 0, 10));

        }
    }

    public void Reset()
    {
        circles = new();
    }
}

internal class GaussCircle : IDrawer
{
    public SceneSwitch Scene { get => SceneSwitch.GaussCircle; }
    public void Draw(int std)
    {

        Raylib.DrawCircle(800, 400, (int)new Normal(80, std).Sample(), new Color(0, 0, 0, 100));
    }

    public void Reset()
    {
    }
}

internal class GaussianWalker : IDrawer
{
    public List<Vector2> pixels = new();

    public Vector2 last = new(800, 400);
    public SceneSwitch Scene { get => SceneSwitch.GaussianWalker; }
    public void Draw(int std)
    {

        if (std > 0)

        {

            var xmov = (int)new Normal(0, std).Sample();

            var ymov = (int)new Normal(0, std).Sample();

            var newPixel = new Vector2(xmov, ymov) + last;

            last = newPixel;

            pixels.Add(newPixel);

        }

        foreach (var pixel in pixels)

        {

            Raylib.DrawPixelV(pixel, new Color(0, 0, 0, 255));

        }
    }

    public void Reset()
    {
        pixels = new();
        last = new(800, 400);
    }
}

internal class BarsScene : IDrawer
{
    public SceneSwitch Scene { get => SceneSwitch.Bars; }
    public int[] Heights { get; set; }

    public Color[] Colors { get; set; }

    public Random Random { get; set; }

    public void InitColors()
    {

        for (int i = 0; i < Heights.Length; i++)
        {
            Colors[i] = new Color(Random.Next(255), Random.Next(255), Random.Next(255), 255);
        }
    }
    public void Draw(int std)
    {
        for (int i = 0; i < Heights.Length; i++)

        {
            var inc = Random.Next(Heights.Length);

            Heights[inc] += 1;

            Raylib.DrawRectangle((i * 1600 / Heights.Length) + 10, 800 - Heights[i], 1600 / (Heights.Length + 1), Heights[i], new Color(i, i, i, 255));

        }
    }

    public void Reset()
    {
        Heights = new int[Heights.Length];
    }
}
