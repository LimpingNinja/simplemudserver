using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace BittyMud;

public delegate void CommandHandler(User user, string args);
public delegate void StateHandler(User user);

[Flags]
public enum UserGroups
{
    Guest = 0,
    Player = 1,
    Admin = 2,
}

public enum UserState
{
    Login,
    Menu,
    Online,
    Idle,
    Offline,
    Disconnected,
}

public static class Mud
{
    private static readonly TcpListener listener;
    private static readonly Dictionary<Socket, User> Sockets = new Dictionary<Socket, User>();
    private static CancellationTokenSource cts = new CancellationTokenSource();

    private const int PULSE_PER_SECOND = 10;
    private const int HEARTBEAT_INTERVAL = 2;  // in seconds

    private static Dictionary<string, (CommandHandler, string)> commandHandlers;
    private const string DEFAULT_ERROR = "What?";

    static Mud()
    {
        listener = new TcpListener(IPAddress.Any, 5555);
        InitializeCommands();
    }

    private static void InitializeCommands()
    {
        commandHandlers = new Dictionary<string, (CommandHandler, string)>
        {
            {"shutdown", (ShutdownCommand, "Admin")} // new shutdown command
        };
    }

    public static void WriteLine(Object ob, string msg)
    {
        if (ob is User user) 
        {
            user.ReceiveMessage($"{msg}\r\n");
        }
    }

    public static void Write(Object ob, string msg)
    {
        if (ob is User user) 
        {
            user.ReceiveMessage(msg);
        }
    }

    public static void Start()
    {
        listener.Start();
        Console.WriteLine("Server started...");

        // Instantiate Commands
        ActionSafe.InitializeCommands();
        HelpSystem.InitializeCommands();
        
        // Start the game loop in a separate thread
        Thread gameLoopThread = new Thread(GameLoop);
        gameLoopThread.Start();

        // Accept players
        while (!cts.Token.IsCancellationRequested)
        {
            TcpClient client = listener.AcceptTcpClient();
            Socket socket = new Socket(client);
            User user = new User();
            Sockets.Add(socket, user);
            HandleUser(user, socket);
            
            // Cleanup disconnected players
            foreach (var kv in Sockets)
            {
                if (!kv.Key.Connected)
                {
                    Sockets.Remove(kv.Key);
                }
            }
        }
    }
    public static void AddCommand(string command, CommandHandler handler, string userGroup)
    {
        if (!commandHandlers.ContainsKey(command))
        {
            commandHandlers[command] = (handler, userGroup);
        }
    }

    public static void RemoveCommand(string command)
    {
        if (commandHandlers.ContainsKey(command))
        {
            commandHandlers.Remove(command);
        }
    }
    public static void ShutdownCommand(User user, string args)
    {
        WriteLine(user, "Server is shutting down...");
        cts.Cancel();
    }
    
    private static void HandleUser(User user, Socket socket)
    {
        // Check if the player is reconnecting
        var existingUser = Sockets.Values.FirstOrDefault(p => p.Name == user.Name && p.State == UserState.Disconnected && (DateTime.Now - p.LastSeen).TotalMinutes <= 15);
        if (existingUser != null)
        {
            // Reattach the player to the existing Player object
            user = existingUser;
            user.State = UserState.Disconnected;
            Console.WriteLine($"{user.Name} has reconnected.");
        }
        user.Socket = socket;
        user.State = UserState.Login;
        WriteLine(user, HelpSystem.Greeting);
        user.Socket.SendPrompt("Enter your user name:");
        user.StateHandlers.Push(CommandStateHandler);  // default handler
        user.StateHandlers.Push(LoginStateHandler);   // login handler
    }

    public static void AddUserGroup(User user, string group)
    {
        if (Enum.TryParse(group, true, out UserGroups userGroup))
        {
            user.UserGroups |= userGroup;
        }
    }

    public static UserGroups GetUserGroups(User user)
    {
        return user.UserGroups;
    }

    public static bool HasUserGroup(User user, string group)
    {
        if (Enum.TryParse(group, true, out UserGroups userGroup))
        {
            return (user.UserGroups & userGroup) == userGroup;
        }
        return false;
    }
    
    private static void LoginStateHandler(User user)
    {
        // Now you should read from the InputBuffer instead of using StreamReader directly
        // You need a method in your Socket class to read a line from the InputBuffer
        user.Name = user.Socket.ReadLine();
        WriteLine(user, $"Welcome {user.Name}!");
        if(user.Name == "KC") AddUserGroup(user, "admin");
        user.State = UserState.Online;
        user.Socket.SendPrompt("> ");
        // Pop the login state handler, now command state handler is on top
        user.StateHandlers.Pop();
    }
    
    private static void CommandStateHandler(User user)
    {
        string input = user.Socket.ReadLine();
        if (string.IsNullOrEmpty(input))
        {
            user.Socket.SendPrompt("> ");
            return;
        }

        var command = input.Split(' ')[0].ToLower();
        var args = input.Length > command.Length ? input.Substring(command.Length + 1) : string.Empty;

        try
        {
            if (commandHandlers.TryGetValue(command, out var handler))
            {
                var (commandHandler, requiredGroup) = handler;
                if (HasUserGroup(user, requiredGroup))
                {
                    commandHandler(user, args);
                }
                else
                {
                    WriteLine(user, DEFAULT_ERROR);
                }
            }
            else
            {
                WriteLine(user, DEFAULT_ERROR);
            }
            user.Socket.SendPrompt("> ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CommandStateHandler: {ex.Message}");
            WriteLine(user, "An error has occurred and been logged!");
            WriteLine(user, DEFAULT_ERROR);
        }
    }
    public static void RemoveUser(User user)
    {
        user.StateHandlers.Clear();
        user.Socket.Client.Close();
        user.Quit();
        var socketToRemove = Sockets.Keys.FirstOrDefault(s => Sockets[s] == user);
        if (socketToRemove != null)
        {
            Sockets.Remove(socketToRemove);
        }
    }
    private static void GameLoop()
    {
        int pulseCounter = 0;
        var stopwatch = Stopwatch.StartNew();

        while (!cts.Token.IsCancellationRequested)
        {
            var pulseStart = stopwatch.Elapsed;

            // Simulated game update logic
            CheckEvents();

            pulseCounter++;
            if (pulseCounter >= PULSE_PER_SECOND * HEARTBEAT_INTERVAL)
            {
                Heartbeat();
                pulseCounter = 0;
            }

            var socketsToRemove = new List<Socket>();
            foreach (var kv in Sockets)
            {
                var userSocket = kv.Key;
                var user = kv.Value;

                userSocket.Poll(user);

                if (userSocket.Client is {Connected: false} && user.State != UserState.Disconnected)
                {
                    user.State = UserState.Disconnected;
                    user.LastSeen = DateTime.Now;
                }

                if (user.State == UserState.Disconnected && (DateTime.Now - user.LastSeen).TotalMinutes > 15)
                {
                    socketsToRemove.Add(userSocket);
                    Console.WriteLine($"{user.Name} has been forcibly disconnected.");
                }
            }
            foreach (var socket in socketsToRemove)
            {
                Sockets.Remove(socket);
            }
            
            var elapsed = stopwatch.Elapsed - pulseStart;
            var sleepDuration = TimeSpan.FromMilliseconds(1000.0 / PULSE_PER_SECOND) - elapsed;

            if (sleepDuration > TimeSpan.Zero)
                Thread.Sleep(sleepDuration);
        }

        stopwatch.Stop();
    }

    private static void CheckEvents()
    {
        // Placeholder for any event checks that need to happen on each pulse
    }

    private static void Heartbeat()
    {
        // Cleanup disconnected players
        var disconnectedUsers = Sockets.Where(kv => kv.Value.State == UserState.Disconnected).ToList();
        foreach (var kv in disconnectedUsers)
        {
            var userSocket = kv.Key;
            var user = kv.Value;

            if ((DateTime.Now - user.LastSeen).TotalMinutes > 15)
            {
                Sockets.Remove(userSocket);
                Console.WriteLine($"{user.Name} has been forcibly disconnected.");
            }
        }
    }

    public static void Stop()
    {
        cts.Cancel();
        cts.Dispose();
        listener.Stop();
        Console.WriteLine("Server stopped.");
    }

    public static void Main()
    {
        Start();
    }

    public static List<User> GetPlayers()
    {
        return new List<User>(Sockets.Values);
    }
    
}
public class User
{
    public string Name { get; set; }
    public Stack<StateHandler> StateHandlers { get; } = new Stack<StateHandler>();
    public UserState State { get; set; }
    public UserGroups UserGroups { get; set; } = UserGroups.Player;
    public Socket? Socket { get; set; }
    public string Location { get; set; } = "limbo@limbo";
    public DateTime LastSeen { get; set; }

    public void ReceiveMessage(string msg)
    {
        Socket.Send(msg);
    }
    public void Quit()
    {
        ReceiveMessage("Goodbye!\r\n");
    }
}
