class Program
{
    static void Main()
    {
        GameEngine engine = GameEngine.GetInstance(50, 25);
        engine.Mainloop();
    }
}

public class GameEngine
{
    Canvas canvas;
    public DisplayableEntity[,] DisplayedState {get {return canvas.GetDisplayedState();}}
    ConsoleKey lastPressedArrowKey = ConsoleKey.None;
    public ConsoleKey GetLastPressedArrowKey{get {return lastPressedArrowKey;} }

    int score = 0;
    public bool foodEaten = false;
    public bool gameOver = false;

    static GameEngine? instance;
    GameEngine(int canvasWidth, int canvasHeight)
    {
        canvas = Canvas.GetInstance(canvasWidth, canvasHeight);
    }

    public static GameEngine GetInstance(int canvasWidth, int canvasHeight)
    {
        if (instance == null)
        {
            instance = new GameEngine(canvasWidth, canvasHeight);
        }
        return instance;
    }

    public void DisplayOnCanvas(DisplayableEntity entity)
    {
        canvas.Display(entity);
    }

    void Tick()
    {
        foreach (DisplayableEntity entity in DisplayedState)
        {
            if (entity is EntityNeedsUpdate updatableEntity)
            {
                canvas.Display(new EmptySpace(entity.x, entity.y)); // Display empty space on the old position of the entity
                canvas.Display(updatableEntity.Update(this)); // Display the entity at the new position
            }
        }

        canvas.topText = $"SCORE: {score}";

        canvas.Render();
        if (foodEaten)
        {
            score+=1;   
            SpawnFood();
        }
        // Reset after every tick
        lastPressedArrowKey = ConsoleKey.None;
        foodEaten = false;
    }

    void DetectKeyPressesForNMilliseconds(int milliseconds) // An ugly way to avoid multithreading
    {
        for (int i = 0; i < milliseconds; i++)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                if (
                    keyInfo.Key == ConsoleKey.UpArrow ||
                    keyInfo.Key == ConsoleKey.DownArrow ||
                    keyInfo.Key == ConsoleKey.LeftArrow ||
                    keyInfo.Key == ConsoleKey.RightArrow
                )
                {
                    lastPressedArrowKey = keyInfo.Key;
                }
            }
            Thread.Sleep(1);
        }
    }

    void SpawnFood()
    {
        Random random = new Random();
        while (true)
        {
            foreach (DisplayableEntity entity in DisplayedState)
            {
                if (entity is EmptySpace && random.Next(canvas.width * canvas.height) == 1)
                {
                    canvas.Display(new Food(entity.x, entity.y));
                    return;
                }
            }
        }
    }

    public void Mainloop()
    {
        canvas.LoadLayout("./layouts/default.txt");

        // Spawn snake
        int headX = canvas.width / 2;
        int headY = canvas.height / 2;
        canvas.Display(new SnakeHead(canvas.width / 2, canvas.height / 2, EntityNeedsUpdate.Orientation.Right));
        canvas.Display(new SnakeTailSegment(headX-1, headY, EntityNeedsUpdate.Orientation.Right));
        canvas.Display(new SnakeTailSegment(headX-2, headY, EntityNeedsUpdate.Orientation.Right));
        canvas.Display(new SnakeTailSegment(headX-3, headY, EntityNeedsUpdate.Orientation.Right));

        canvas.Render();
        SpawnFood();
        canvas.Render();

        while (!gameOver)
        {
            Tick();

            DetectKeyPressesForNMilliseconds(150);
        }
    }
}

public class Canvas
{
    public readonly int width;
    public readonly int height;

    DisplayableEntity[,] state; // Canvas state to be displayed on the next re-render.
    DisplayableEntity[,] displayedState; // Canvas state currently displayed on the screen.
    public string topText = "";

    private static Canvas? instance;
    private Canvas(int width, int height)
    {
        this.width = width;
        this.height = height;
        this.state = new DisplayableEntity[width,height];
        this.displayedState = new DisplayableEntity[width,height];
    }
    public static Canvas GetInstance(int width, int height)
    {
        if (instance == null)
        {
            instance = new Canvas(width, height);
        }
        return instance;
    }

    // Queues up the character to be displayed during the next render
    public void Display(DisplayableEntity newEntity)
    {
        state[newEntity.x, newEntity.y] = newEntity;
    }

    public void Render()
    {
        Console.Clear();
        Console.WriteLine(topText);
        for (int y = 0; y < state.GetLength(1); y++)
        {
            for (int x = 0; x < state.GetLength(0); x++)
            {
                Console.Write(state[x, y].representation);
            }
            Console.Write('\n');
        }
        displayedState = CloneState(state);
    }
    
    static DisplayableEntity[,] CloneState(DisplayableEntity[,] state)
    {
        DisplayableEntity[,] deepCopy = new DisplayableEntity[state.GetLength(0), state.GetLength(1)];
        for (int i = 0; i < state.GetLength(0); i++)
        {
            for (int j = 0; j < state.GetLength(1); j++)
            {
                deepCopy[i, j] = state[i, j].DeepClone();
            }
        }
        return deepCopy;
    }

    public DisplayableEntity[,] GetDisplayedState()
    {
        return Canvas.CloneState(displayedState);
    }

    public void LoadLayout(string filepath)
    {
        string[] layout = File.ReadAllLines(filepath);
        for (int y = 0; y < layout.Length; y++)
        {
            for (int x = 0; x < layout[y].Length; x++)
            {
                DisplayableEntity entity;
                switch (layout[y][x])
                {
                    case ' ':
                        entity = new EmptySpace(x, y);
                        break;
                    case '█':
                        entity = new Wall(x, y);
                        break;
                    default:
                        entity = new EmptySpace(x, y);
                        break;
                }
                Display(entity);
            }
        }
    }
}

public interface EntityNeedsUpdate
{
    enum Orientation{Up, Down, Right, Left};
    public DisplayableEntity Update(GameEngine engineState);
}

public abstract class DisplayableEntity
{

    public char representation;
    public int x;
    public int y;

    public abstract DisplayableEntity DeepClone(); // Returns deep copy of the instance
}

public class SnakeHead : DisplayableEntity, EntityNeedsUpdate
{
    EntityNeedsUpdate.Orientation orientation;
    // public enum EntityNeedsUpdate.Orientation{Up, Down, Left, Right}
    static readonly Dictionary<EntityNeedsUpdate.Orientation, char> orientationToRepresentation = new Dictionary<EntityNeedsUpdate.Orientation, char>{
        {EntityNeedsUpdate.Orientation.Up, '^'},
        {EntityNeedsUpdate.Orientation.Down, 'v'},
        {EntityNeedsUpdate.Orientation.Right, '>'},
        {EntityNeedsUpdate.Orientation.Left, '<'}
    };

    static readonly Dictionary<ConsoleKey, EntityNeedsUpdate.Orientation> pressedKeyToOrientation = new Dictionary<ConsoleKey, EntityNeedsUpdate.Orientation>{
        {ConsoleKey.UpArrow, EntityNeedsUpdate.Orientation.Up},
        {ConsoleKey.DownArrow, EntityNeedsUpdate.Orientation.Down},
        {ConsoleKey.RightArrow, EntityNeedsUpdate.Orientation.Right},
        {ConsoleKey.LeftArrow, EntityNeedsUpdate.Orientation.Left},
    };

    static readonly Dictionary<EntityNeedsUpdate.Orientation, ConsoleKey> invalidKeyForOrientation = new Dictionary<EntityNeedsUpdate.Orientation, ConsoleKey>{
        {EntityNeedsUpdate.Orientation.Up, ConsoleKey.DownArrow},
        {EntityNeedsUpdate.Orientation.Down, ConsoleKey.UpArrow},
        {EntityNeedsUpdate.Orientation.Right, ConsoleKey.LeftArrow},
        {EntityNeedsUpdate.Orientation.Left, ConsoleKey.RightArrow}
    };

    public SnakeHead(int x, int y, EntityNeedsUpdate.Orientation orientation)
    {
        this.orientation = orientation;
        this.representation = orientationToRepresentation[orientation];
        this.x = x;
        this.y = y;
    }
    public DisplayableEntity Update(GameEngine engineState)
    {
        DisplayableEntity[,] state = engineState.DisplayedState;

        // Update EntityNeedsUpdate.orientation
        if (engineState.GetLastPressedArrowKey != ConsoleKey.None && engineState.GetLastPressedArrowKey != invalidKeyForOrientation[orientation])
        {
            orientation = pressedKeyToOrientation[engineState.GetLastPressedArrowKey];
            representation = orientationToRepresentation[orientation];
        }

        // Determine the entity in front of the snake
        DisplayableEntity blockAhead;
        switch (orientation)
        {
            case EntityNeedsUpdate.Orientation.Up:
                blockAhead = state[this.x, this.y-1];
                break;
            case EntityNeedsUpdate.Orientation.Down:
                blockAhead = state[this.x, this.y+1];
                break;
            case EntityNeedsUpdate.Orientation.Right:
                blockAhead = state[this.x+1, this.y];
                break;
            case EntityNeedsUpdate.Orientation.Left:
                blockAhead = state[this.x-1, this.y];
                break;
            default:
                throw new InvalidOperationException("Invalid entity EntityNeedsUpdate.orientation");
        }

        if (blockAhead is EmptySpace || blockAhead is Food) // Move forward
        {
            if (blockAhead is Food) {engineState.foodEaten = true;}
            engineState.DisplayOnCanvas(new SnakeTailSegment(this.x, this.y, this.orientation));
            this.x = blockAhead.x;
            this.y = blockAhead.y;
            return this;
        }
        if (blockAhead is Wall || blockAhead is SnakeTailSegment)
        {
            engineState.gameOver = true;
            return this;
        }
        throw new InvalidOperationException($"No action implemented to handle the {blockAhead.GetType()} entity");
    }
    public override DisplayableEntity DeepClone()
    {
        return new SnakeHead(this.x, this.y, this.orientation);
    }

}

public class SnakeTailSegment : DisplayableEntity, EntityNeedsUpdate
{
    EntityNeedsUpdate.Orientation orientation;
    static readonly Dictionary<EntityNeedsUpdate.Orientation, char> orientationToRepresentation = new Dictionary<EntityNeedsUpdate.Orientation, char>{
        {EntityNeedsUpdate.Orientation.Up, '|'},
        {EntityNeedsUpdate.Orientation.Down, '|'},
        {EntityNeedsUpdate.Orientation.Right, '-'},
        {EntityNeedsUpdate.Orientation.Left, '-'}
    };

    public SnakeTailSegment(int x, int y, EntityNeedsUpdate.Orientation orientation)
    {
        this.orientation = orientation;
        this.representation = orientationToRepresentation[orientation];
        this.x = x;
        this.y = y;
    }

    public DisplayableEntity Update(GameEngine engineState)
    {
        Dictionary<int[], EntityNeedsUpdate.Orientation> orientationIfConnectedBehind = new Dictionary<int[], EntityNeedsUpdate.Orientation>{
            {[0, 1], EntityNeedsUpdate.Orientation.Up},
            {[0, -1], EntityNeedsUpdate.Orientation.Down},
            {[-1, 0], EntityNeedsUpdate.Orientation.Right},
            {[1, 0], EntityNeedsUpdate.Orientation.Left},
        };

        if (engineState.foodEaten)
        {
            // Do not destroy the segment if the food was eaten
            return this;
        }

        DisplayableEntity[,] displayedState = engineState.DisplayedState;
        foreach (int[] offset in orientationIfConnectedBehind.Keys)
        {
            DisplayableEntity entity = displayedState[this.x+offset[0], this.y+offset[1]];
            if (entity is SnakeTailSegment snakeTailSegment && snakeTailSegment.orientation == orientationIfConnectedBehind[offset])
            {
                // Do not destroy the segment if there is another segment behind the current
                return this;
            }
        }

        // Otherwise - destroy the segment
        return new EmptySpace(this.x, this.y);
    }

    public override DisplayableEntity DeepClone()
    {
        return new SnakeTailSegment(this.x, this.y, this.orientation);
    }
}

public class Wall : DisplayableEntity
{
    public Wall(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.representation = '█';
    }

    public override DisplayableEntity DeepClone()
    {
        return new Wall(this.x, this.y);
    }
}

public class Food : DisplayableEntity
{
    public Food(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.representation = '*';
    }

    public override DisplayableEntity DeepClone()
    {
        return new Food(this.x, this.y);
    }
}

public class EmptySpace : DisplayableEntity
{
    public EmptySpace(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.representation = ' ';
    }

    public override DisplayableEntity DeepClone()
    {
        return new EmptySpace(this.x, this.y);
    }
}
