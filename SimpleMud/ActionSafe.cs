using static SimpleMud.Mud;

namespace SimpleMud
{
    public static class ActionSafe
    {
        public static void WhoCommand(User user, string args)
        {
            string textWho = "./txt/who";
            string text = File.ReadAllText(textWho);

            WriteLine(user, text);
            string header = "+-----------------+-----------------+-----------------+-----------------+";
            string title = "| User Name       | User State      | Room            | Domain          |";
            string divider = "+-----------------+-----------------+-----------------+-----------------+";

            WriteLine(user, $"{header}\r\n{title}\r\n{divider}");
            foreach (var ply in Mud.GetPlayers())
            {
                string userName = ply.Name;
                string userState = ply.State.ToString();
                string room = ply.Location.Split('@')[0];
                string domain = ply.Location.Split('@')[1];

                string line = String.Format("| {0,-15} | {1,-15} | {2,-15} | {3,-15} |",
                    userName, userState, room, domain);
                WriteLine(user, line); 
            }
            WriteLine(user, divider);
        }


        public static void SayCommand(User user, string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                WriteLine(user, "Say what?");
                return;
            }

            WriteLine(user,$"You say: {args}");
            foreach (var ply in Mud.GetPlayers())
            {
                if (ply != user)
                {
                    WriteLine(ply, $"{user.Name} says: {args}");
                }
            }
        }
        public static void QuitCommand(User user, string args)
        {
            WriteLine(user, "Goodbye!");
            Mud.RemoveUser(user);
        }
        
        public static void ShoutCommand(User user, string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                WriteLine(user, "Shout what?");
                return;
            }

            string domain = user.Location.Split('@')[1]; // Get the domain of the sender
            foreach (var ply in Mud.GetPlayers())
            {
                if (ply.Location.Split('@')[1] == domain && ply != user)
                {
                    WriteLine(ply, $"{user.Name} shouts: {args}");
                }
            }
            WriteLine(user, $"You shout: {args}");
        }

        public static void ChatCommand(User user, string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                WriteLine(user, "Chat what?");
                return;
            }

            foreach (var ply in Mud.GetPlayers())
            {
                if (ply != user)
                {
                    WriteLine(ply, $"{user.Name} chats: {args}");
                }
            }
            WriteLine(user, $"You chat: {args}");
        }

        public static void InitializeCommands()
        {
            Mud.AddCommand("quit", QuitCommand, "Player");
            Mud.AddCommand("who", WhoCommand, "Player");
            Mud.AddCommand("say", SayCommand, "Player");
            Mud.AddCommand("chat", ChatCommand, "Player");
            Mud.AddCommand("shout", ShoutCommand, "Player");
        }
        static ActionSafe()
        {
            Console.WriteLine("Instantiated ActionSafe command file!");
        }
    }
}