///Daniel Coimbra Salomão 
///Yanxia Bu
///CS3500 PS8
///
/// Class that contains the drawing methods.
/// 

/// <summary>
/// set up the background and initialize the world and also set up the color and drawing in the snake 
/// client 
/// </summary>
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using IImage = Microsoft.Maui.Graphics.IImage;
#if MACCATALYST
using Microsoft.Maui.Graphics.Platform;
#else
using Microsoft.Maui.Graphics.Win2D;
#endif
using Color = Microsoft.Maui.Graphics.Color;
using System.Reflection;
using Microsoft.Maui;
using System.Net;
using Font = Microsoft.Maui.Graphics.Font;
using SizeF = Microsoft.Maui.Graphics.SizeF;
using System.Runtime.ExceptionServices;
namespace SnakeGame;
/// <summary>
/// This is the constructor of the world panel. it has 6 object in this part the wall, 
/// and the map, sushu for powerup the the world we also
/// set the bool flag to evaluate the wheather is drawing or not the initial size to the view is 90
/// </summary>
public class WorldPanel : IDrawable
{
    private IImage wall;
    private IImage background;
    private IImage sushi;
    private World theWorld;
    private bool initializedForDrawing = false;
    private int viewSize = 900;
    public delegate void ObjectDrawer(object o, ICanvas canvas);
#if MACCATALYST
    private IImage loadImage(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeGame.Resources.Images";
        return PlatformImage.FromStream(assembly.GetManifestResourceStream($"{path}.{name}"));
    }
#else
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private IImage loadImage(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeGame.Resources.Images";
        var service = new W2DImageLoadingService();
        return service.FromStream(assembly.GetManifestResourceStream($"{path}.{name}"));
    }
#endif

    public WorldPanel()
    {
    }
    /// <summary>
    ///setWorld method to get from the view
    /// </summary>
    /// <param name="w"></param>
    public void SetWorld(World w)
    {
        theWorld = w;
    }
    /// <summary>
    /// InitializeDrawing method that will allow the drawing to only hapen once it is true
    /// </summary>
    private void InitializeDrawing()
    {
        wall = loadImage("WallSprite.png");
        background = loadImage("Background.png");
        sushi = loadImage("sushi.png");
        initializedForDrawing = true;
    }
    private void DrawObjectWithTransform(ICanvas canvas, object o, double worldX, double worldY, double angle, ObjectDrawer drawer)
    {
        // "push" the current transform
        canvas.SaveState();

        canvas.Translate((float)worldX, (float)worldY);
        canvas.Rotate((float)angle);
        drawer(o, canvas);

        // "pop" the transform
        canvas.RestoreState();
    }
    //creating a dictionary that will hold the id-colors reference for new snakes
    private Dictionary<int, int> colorsDictionary = new();
    /// <summary>
    /// Method that will determine the snakes colors based on connection order and id. Using 8 different colors for first snakes and random after that.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    private Color SnakeColor(int count, int id)
    {
        switch (count)
        {
            case(1):
                return Colors.Red;
            case(2):
                return Colors.Green;
            case(3):
                return Colors.Blue;
            case (4):
                return Colors.Purple;
            case (5):
                return Colors.Brown;
            case (6):
                return Colors.Yellow;
            case (7):
                return Colors.White;
            case (8):
                return Colors.Beige;
            case (9):
                return Colors.Cyan;
            default:
                Random rnd = new();
                //using the dictionary and a random number generator, add a color to the unique snake id
                if(!colorsDictionary.ContainsKey(id))
                    colorsDictionary.Add(id, rnd.Next(1, 9));
                return SnakeColor(colorsDictionary[id],id);
        }
        
    }
   
    /// <summary>
    /// Method to draw the death animation, it uses the direction and a fake timer that will be called on every frame when snake is dead
    /// that timer will increase the particles distance until 25 unit calls has been reached
    /// </summary>
    /// <param name="o"></param>
    /// <param name="canvas"></param>
    /// <param name="timer"></param>
    /// <param name="count"></param>
    private void deathDrawer(object o, ICanvas canvas,ref int timer, int count)
    {
        Snake snake = o as Snake;
        canvas.FillColor = SnakeColor(count, snake.snake);
        canvas.StrokeSize = 4;
        //using the snake head position as reference for explosion
        Vector2D head = snake.body[snake.body.Count - 1];
        if (timer < 25)
        {
            //check for snake diraction to draw the explosion particles 
            if (snake.dir.x == -1)
            {
                canvas.FillEllipse((float)head.x + timer, (float)head.y + timer, 8, 8);
                canvas.FillEllipse((float)head.x + timer, (float)head.y - timer, 10, 10);
                canvas.FillEllipse((float)(head.x + timer*1.5), (float)head.y, 8, 8);
                canvas.FillEllipse((float)(head.x + timer / 1.5), (float)(head.y + timer / 1.5), 8, 8);
                canvas.FillEllipse((float)(head.x + timer/3), (float)head.y + timer, 10, 10);
                canvas.FillEllipse((float)(head.x + timer/2), (float)head.y - timer, 8, 8);
            }
            if (snake.dir.x == 1)
            {
                canvas.FillEllipse((float)head.x - timer, (float)head.y + timer, 10, 10);
                canvas.FillEllipse((float)head.x - timer, (float)head.y - timer, 8, 8);
                canvas.FillEllipse((float)(head.x - timer * 1.5), (float)head.y, 10, 10);
                canvas.FillEllipse((float)(head.x - timer / 2), (float)(head.y + timer/2), 8, 8);
                canvas.FillEllipse((float)(head.x - timer / 3), (float)head.y + timer, 8, 8);
                canvas.FillEllipse((float)(head.x - timer / 2), (float)head.y - timer, 10, 10);
            }
            if (snake.dir.y == 1)
            {
                canvas.FillEllipse((float)head.x - timer, (float)head.y - timer, 8, 8);
                canvas.FillEllipse((float)head.x + timer, (float)head.y - timer, 10, 10);
                canvas.FillEllipse((float)head.x, (float)(head.y - timer*1.5), 8, 8);
                canvas.FillEllipse((float)(head.x + timer/2), (float)(head.y - timer / 1.5), 8, 8);
                canvas.FillEllipse((float)head.x + timer, (float)(head.y - timer/1.5), 10, 10);
                canvas.FillEllipse((float)head.x - timer, (float)(head.y- timer/2), 8, 8);

            }
            if (snake.dir.y == -1)
            {
                canvas.FillEllipse((float)head.x - timer, (float)head.y + timer, 10, 10);
                canvas.FillEllipse((float)head.x + timer, (float)head.y + timer, 8, 8);
                canvas.FillEllipse((float)head.x, (float)(head.y + timer * 1.5), 10, 10);
                canvas.FillEllipse((float)(head.x + timer/2), (float)(head.y + timer / 1.5), 8, 8);
                canvas.FillEllipse((float)head.x+ timer, (float)(head.y + timer / 1.5), 8, 8);
                canvas.FillEllipse((float)head.x - timer, (float)(head.y +timer / 2), 10, 10);
            }
            timer += 1;
        }

    }
    
    /// <summary>
    /// method for drawing snake, using the foreach vector2D for every body part and drawing it from tail to head
    /// </summary>
    /// <param name="o"></param>
    /// <param name="canvas"></param>
    /// <param name="count"></param>
    private void snakeDrawer(object o, ICanvas canvas,int count)
    {
        //setup the line attributes
        Snake snake = o as Snake;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeColor = SnakeColor(count,snake.snake);
        canvas.StrokeSize = 10;
        //get the head and tail position as a vector2D.
        Vector2D tail = snake.body[0];
        Vector2D head = snake.body[snake.body.Count-1];

            foreach (Vector2D q in snake.body)
            {
                float x = (float)(q.x / tail.x);
                float y = (float)(q.y / tail.y);
            //check for a condition that snake go from one side to the other, that way it will not connect both sides
            if (Math.Abs(tail.x - q.x) < theWorld.worldSize && Math.Abs(tail.y - q.y) < theWorld.worldSize)
            {
                canvas.DrawLine((float)tail.x, (float)tail.y, (float)q.x, (float)q.y);
            }
                tail = q;
            }
            //drawing the snake eyes
            if (snake.dir.x == 0)
            {
                canvas.FillColor = Colors.White;
                canvas.FillEllipse((float)head.x + 2, (float)head.y, (float)3.5, (float)3.5);
                canvas.FillEllipse((float)head.x - 6, (float)head.y, (float)3.5, (float)3.5);
                canvas.FillColor = Colors.Black;
                canvas.FillEllipse((float)head.x + 2, (float)head.y, (float)2, (float)2);
                canvas.FillEllipse((float)head.x - 6, (float)head.y, (float)2, (float)2);
            }

            if (snake.dir.y == 0)
            {
                canvas.FillColor = Colors.White;
                canvas.FillEllipse((float)head.x, (float)head.y + 2, (float)3.5, (float)3.5);
                canvas.FillEllipse((float)head.x, (float)head.y - 6, (float)3.5, (float)3.5);
                canvas.FillColor = Colors.Black;
                canvas.FillEllipse((float)head.x, (float)head.y + 2, (float)2, (float)2);
                canvas.FillEllipse((float)head.x, (float)head.y - 6, (float)2, (float)2);
            }

            //drawing the snake name and score 
            canvas.FontColor = Colors.White;
            canvas.FontSize = 12;
            canvas.Font = Font.Default;
            canvas.DrawString(snake.name + ": " + snake.score.ToString(), (float)head.x - 20, (float)head.y, 380, 100, HorizontalAlignment.Left, VerticalAlignment.Top);


    }
    private int timer = 0;
    
    /// <summary>
    /// Draw method, using a lock to prevent code run and jittering.
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="dirtyRect"></param>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        lock (theWorld)
        {
            //check if world is fully loaded (controller had fully read the json from the server)
            if (!theWorld.loaded)
            {
                return;
            }
            if (!initializedForDrawing)
            {
                InitializeDrawing();
            }
            canvas.ResetState();
            //get the position of the snake head for camera centralization
            float playerX = (float)theWorld.snakes[theWorld.id].body.Last().GetX();
            float playerY = (float)theWorld.snakes[theWorld.id].body.Last().GetY();


            //draw image centralizing camera on the snake
            canvas.Translate(-playerX + (viewSize / 2), -playerY + (viewSize / 2));

            canvas.DrawImage(background, -theWorld.worldSize / 2, -theWorld.worldSize / 2, theWorld.worldSize, theWorld.worldSize);
            
            int width = 50;
            //drawing the walls, there are only 4 options of wall drawing, left to right, right to left, up to down, down to up
            foreach (var p in theWorld.walls.Values)
            {
                for (double x = p.p1.x - (width / 2); x <= p.p2.x; x += width)
                {
                    canvas.DrawImage(wall, (float)x, (float)p.p1.y - (width) / 2, width, width);
                }
                for (double x = p.p2.x - (width / 2); x <= p.p1.x; x += width)
                {
                    canvas.DrawImage(wall, (float)x, (float)p.p1.y- width / 2, width, width);
                }
                for (double x = p.p1.y - width / 2; x <= p.p2.y; x += width)
                {
                    canvas.DrawImage(wall, (float)p.p1.x - (width) / 2, (float)x, width, width);
                }
                for (double x = p.p2.y - width / 2; x <= p.p1.y; x += width)
                {
                    canvas.DrawImage(wall, (float)p.p1.x - width / 2, (float)x, width, width);
                }
            }
            int count = 1;
            //snake drawing, and death drawing
            foreach (var p in theWorld.snakes.Values)
            {
                //if alive call snake drawer
                if (p.alive)
                {
                    snakeDrawer(p, canvas, count);
                }
                //when died, set the timer counter to 0
                if (p.died)
                {
                    timer = 0;
                }
                //every frame after dead will call the death/explosion drawer
                if (!p.alive)
                {
                    deathDrawer(p, canvas,ref timer, count);
                }
                count++;

            }
            //powerup drawing using an image
            foreach (var p in theWorld.powerups.Values)
            {
                if (!p.died)
                    canvas.DrawImage(sushi, (float)p.loc.GetX()-15, (float)p.loc.GetY()-15, 30, 30);

            }
            //drawing score on top of screen
            canvas.FontColor = Colors.White;
            canvas.FontSize = 25;
            canvas.Font = Font.DefaultBold;
            canvas.DrawString("Score: " + theWorld.snakes[theWorld.id].score.ToString(), (float)playerX - 10, (float)playerY - 440, 380, 100, HorizontalAlignment.Left, VerticalAlignment.Top);



        }


    }
}
