
class Program
{
    static void Main()
    {
        GameEngine engine = GameEngine.GetInstance(80, 25);
        engine.Mainloop();
    }
}

public class GameEngine
{
    // General setup
    const string saveDir = "./saves/";
    int canvasWidth;
    int canvasHeight;
    Canvas canvas;
    Menu menu;
    public string layoutFileName = "default.txt";

    // Engine state fields
    public DisplayableEntity[,] DisplayedState {get {return canvas.GetDisplayedState();}}
    ConsoleKey lastPressedArrowKey = ConsoleKey.None;
    public ConsoleKey GetLastPressedArrowKey{get {return lastPressedArrowKey;} }

    // Game fields
    int score = 0;
    int highScore = 0;
    public int speed = 250; // Milliseconds between ticks
    public bool foodEaten = false;
    public bool gameOver = false;

    static GameEngine? instance;
    GameEngine(int canvasWidth, int canvasHeight)
    {
        this.canvasWidth = canvasWidth;
        this.canvasHeight = canvasHeight;
        canvas = Canvas.GetInstance(canvasWidth, canvasHeight);
        menu = Menu.GetInstance(canvasWidth, canvasHeight);
    }

    public static GameEngine GetInstance(int canvasWidth, int canvasHeight)
    {
        if (instance == null)
        {
            instance = new GameEngine(canvasWidth, canvasHeight);
        }
        if (instance.canvasWidth != canvasWidth || instance.canvasHeight != canvasHeight)
        {
            throw new ArgumentException("Wrong width and height for an existing sigleton instance.");
        }
        return instance;
    }

    public void DisplayOnCanvas(DisplayableEntity entity)
    {
        canvas.Display(entity);
    }

    int ReadHighScore(string filename = "highScore.txt")
    {
        string filePath = Path.Combine(saveDir, filename);
        string backupFilePath = Path.Combine(saveDir, filename)+".bak";
        int score;
        try
        {
            bool parseSuccess = false;
            if (File.Exists(filePath))
            {
                string text = File.ReadAllText(filePath);
                parseSuccess = int.TryParse(text, out score);
                if (parseSuccess) {return score;}
            }
            if (File.Exists(backupFilePath))
            {
                string text = File.ReadAllText(backupFilePath);
                parseSuccess = int.TryParse(text, out score);
                if (parseSuccess) {return score;}
            }
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    void WriteHighScore(string filename = "highScore.txt")
    {
        if (!Directory.Exists(saveDir))
        {
            Directory.CreateDirectory(saveDir);
        }
        string filePath = Path.Combine(saveDir, filename);
        string backupFilePath = Path.Combine(saveDir, filename)+".bak";
        File.WriteAllText(filePath, score.ToString());
        File.WriteAllText(backupFilePath, score.ToString());
    }

    void Tick()
    {
        // Update layer 1 - SnakeHead
        foreach (DisplayableEntity entity in DisplayedState)
        {
            if (entity is SnakeHead updatableEntity)
            {
                canvas.Display(new EmptySpace(entity.x, entity.y)); // Display empty space on the old position of the entity
                canvas.Display(updatableEntity.Update(this)); // Display the entity at the new position
            }
        }
        // Update layer 2 - Everything else
        foreach (DisplayableEntity entity in DisplayedState)
        {
            if (entity is EntityNeedsUpdate updatableEntity && entity is not SnakeHead)
            {
                canvas.Display(new EmptySpace(entity.x, entity.y)); // Display empty space on the old position of the entity
                canvas.Display(updatableEntity.Update(this)); // Display the entity at the new position
            }
        }

        canvas.Render();
        if (foodEaten)
        {
            score+=1; 
            if (highScore < score)
            {
                highScore = score;
                WriteHighScore();
            }  
            SpawnFood();
        }
        canvas.topText = $"HIGH SCORE: {highScore}\nSCORE: {score}";
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

    void SpawnSnake()
    {
        bool ValidSpawn(int x, int y)
        {
            return (
                DisplayedState[x, y] is EmptySpace &&
                DisplayedState[x-1, y] is EmptySpace &&
                DisplayedState[x-2, y] is EmptySpace &&
                DisplayedState[x-3, y] is EmptySpace
            );
        }

        int headX = canvas.width / 2;
        int headY = canvas.height / 2;

        if (!ValidSpawn(headX, headY))
        {
            headY = 0;
            while (!ValidSpawn(headX, headY))
            {
                headY+=2;
            }
        }

        canvas.Display(new SnakeHead(headX, headY, EntityNeedsUpdate.Orientation.Right));
        canvas.Display(new SnakeTailSegment(headX-1, headY, EntityNeedsUpdate.Orientation.Right));
        canvas.Display(new SnakeTailSegment(headX-2, headY, EntityNeedsUpdate.Orientation.Right));
        canvas.Display(new SnakeTailSegment(headX-3, headY, EntityNeedsUpdate.Orientation.Right));
    }

    public void GameLoop()
    {
        canvas.LoadLayout(layoutFileName);
        canvas.Render();
        
        SpawnSnake();
        canvas.Render();

        SpawnFood();
        canvas.Render();

        while (!gameOver)
        {
            Tick();

            DetectKeyPressesForNMilliseconds(speed);
        }
    }

    public void Mainloop()
    {
        highScore = ReadHighScore();
        while (true)
        {
            canvas.topText = $"HIGH SCORE: {highScore}\nSCORE: {score}";

            menu.MenuLoop(this, gameOver);

            gameOver = false;
            GameLoop();
            WriteHighScore();
            score = 0;
        }
    }
}

public class Menu
{
    int canvasWidth;
    int canvasHeight;

    Canvas canvas;

    struct Difficulty
    {
        public string name;
        public int speed;
    }
    Difficulty[] difficulties = [
        new Difficulty {name = "EASY", speed = 300},
        new Difficulty {name = "MEDIUM", speed = 150},
        new Difficulty {name = "HARD", speed = 75}
    ];

    struct Level
    {
        public string name;
        public string FileName;
    }
    Level[] levels = [
        new Level {name = "BOX", FileName = "default.txt"},
        new Level {name = "BRIDGE", FileName = "bridge.txt"},
        new Level {name = "PANTS", FileName = "pants.txt"},
    ];

    static Menu? instance;
    Menu(int canvasWidth, int canvasHeight)
    {
        this.canvasWidth = canvasWidth;
        this.canvasHeight = canvasHeight;
        canvas = Canvas.GetInstance(canvasWidth, canvasHeight);
    }
    public static Menu GetInstance(int canvasWidth, int canvasHeight)
    {
        if (instance == null){
            instance = new Menu(canvasWidth, canvasHeight);
        }
        if (instance.canvasWidth != canvasWidth || instance.canvasHeight != canvasHeight)
        {
            throw new ArgumentException("Wrong width and height for an existing sigleton instance.");
        }
        return instance;
    }

    void RenderMenu(string[] text, int selectedOption)
    {
        canvas.LoadLayout("default.txt");
        int startY = canvas.height / 2 - text.Length / 2;
        for (int i = 0; i < text.Length; i++)
        {
            string lineToDisplay  = i == selectedOption ? $"* {text[i]} *" : text[i];
            int startX = canvas.width / 2 - lineToDisplay.Length / 2;
            for (int j = 0; j < lineToDisplay.Length; j++ )
            {
                canvas.Display(new TextCharacter(startX+j, startY+i, lineToDisplay[j]));
            }
        }
        canvas.Render();
    }

    void GameOverMenuLoop()
    {
        string[] menutext = ["Game Over"];
        int selectedOption = 0;
        RenderMenu(menutext, selectedOption);
        while(true)
        {
            if (Console.KeyAvailable && Console.ReadKey(intercept: true).Key == ConsoleKey.Enter)
            {
                return;
            }
        }
    }

    public void MenuLoop(GameEngine engineState, bool gameOver)
    {
        if (gameOver)
        {
            GameOverMenuLoop();
        }

        int selectedOption = 0;
        int difficultyIndex = 0;
        int levelIndex = 0;

        engineState.speed = difficulties[difficultyIndex].speed;
        engineState.layoutFileName = levels[levelIndex].FileName;

        string[] menuText = [
            "PLAY",
            difficulties[difficultyIndex].name,
            $"LEVEL: {levels[levelIndex].name}",
            "EXIT"
        ];

        RenderMenu(menuText, selectedOption);
        while(true)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (selectedOption > 0)
                        {
                            selectedOption-=1;
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (selectedOption < menuText.Length-1)
                        {
                            selectedOption+=1;
                        }
                        break;
                    case ConsoleKey.Enter:
                        switch (selectedOption)
                        {
                            case 0:
                                return;
                            case 1:
                                if (difficultyIndex < difficulties.Length-1)
                                {
                                    difficultyIndex+=1;
                                    engineState.speed = difficulties[difficultyIndex].speed;
                                    break;
                                }
                                difficultyIndex = 0;
                                engineState.speed = difficulties[difficultyIndex].speed;
                                break;
                            case 2:
                                if (levelIndex < levels.Length-1)
                                {
                                    levelIndex+=1;
                                    engineState.layoutFileName = levels[levelIndex].FileName;
                                    break;
                                }
                                levelIndex = 0;
                                engineState.layoutFileName = levels[levelIndex].FileName;
                                break;
                            case 3:
                                Environment.Exit(0);
                                break;
                        }
                        break;
                }
                menuText = [
                    "PLAY",
                    difficulties[difficultyIndex].name,
                    $"LEVEL: {levels[levelIndex].name}",
                    "EXIT"
                ];
                RenderMenu(menuText, selectedOption);
            }
        }
    }
}

public class Canvas
{
    public readonly int width;
    public readonly int height;
    const string layoutsDir = "./layouts/";

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
        if (instance.width != width || instance.width != width)
        {
            throw new ArgumentException("Wrong width and height for an existing sigleton instance.");
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
        return CloneState(displayedState);
    }

    public void LoadLayout(string layoutName)
    {
        string[] layout = File.ReadAllLines(Path.Combine(layoutsDir, layoutName));
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
                    case '*':
                        entity = new Barrier(x, y);
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

public class TextCharacter : DisplayableEntity
{
    public TextCharacter(int x, int y, char representation)
    {
        this.x = x;
        this.y = y;
        this.representation = representation;
    }

    public override DisplayableEntity DeepClone()
    {
        return new TextCharacter(this.x, this.y, this.representation);
    }
}

public class Barrier : DisplayableEntity
{
    public Barrier(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.representation = ' ';
    }

    public override DisplayableEntity DeepClone()
    {
        return new Barrier(this.x, this.y);
    }
}
