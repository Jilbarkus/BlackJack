using System.Numerics;
using System.Reflection;

namespace CardGames
{
    static class GameChooser
    {
        public static bool Choose(IGame[] games, ref int index, in PlayerSave playerSave, bool bPlayWelcomeMessage = false)
        {
            Console.Clear();
            if (bPlayWelcomeMessage == true)
            {
                new string[] { $"New guest {playerSave.Name}!", "Here is $100 to get you started" }.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Center, JustifyY.Center);
            }
            getLogoArray().PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Center, JustifyY.Top);
            string[] text = new string[] { "", $"         Welcome {playerSave.Name}, What would you like to do?", "" };
            for (int i = 0; i < games.Length; i++)
            {
                text = text.Append($"{i + 1}) {games[i].GetType().Name}").ToArray();
                text = text.Append("").ToArray();
            }
            text = text.Append($"X) Quit").ToArray();
            text = text.Append("").ToArray();
            text.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Center, JustifyY.Bottom);
            // Top Right  Text
            new string[1] { $"Bank: ${playerSave.Cash}" }.PrintArrayToConsole(new IntVector2(Console.CursorLeft, Console.CursorTop), JustifyX.Right, JustifyY.Top);

            char userInput = Console.ReadKey().KeyChar;
            Console.Clear();
            switch (userInput)
            {
                case 'x': return false;
                default:
                    if (int.TryParse(userInput.ToString(), out int result) && result > 0 && result < games.Length + 1)
                    {
                        if (games[result - 1] != null)
                        {
                            index = result - 1;
                            return true;
                        }
                        else return false;
                    }
                    //else return true;
                    else
                    {
                        index = 0;
                        return true;
                    }
            }

            string getLogo() =>
        @"       _..._                                            .-'''-.     
    .-'_..._''.                                        '   _    \   
  .' .'      '.\                     .--.   _..._    /   /` '.   \  
 / .'                                |__| .'     '. .   |     \  '  
. '                                  .--..   .-.   .|   '      |  ' 
| |                 __               |  ||  '   '  |\    \     / /  
| |              .:--.'.         _   |  ||  |   |  | `.   ` ..' /   
. '             / |   \ |      .' |  |  ||  |   |  |    '-...-'`    
 \ '.          .`"" __ | |     .   | /|  ||  |   |  |                
  '. `._____.-'/ .'.''| |   .'.'| |//|__||  |   |  |                
    `-.______ / / /   | |_.'.'.-'  /     |  |   |  |                
             `  \ \._,\ '/.'   \_.'      |  |   |  |                
                 `--'  `""                '--'   '--'                 
";
            string[] getLogoArray() =>new string[]{
@"         _..._                                            .-'''-.     ",
@"      .-'_..._''.                                        '   _    \   ",
@"    .' .'      '.\                     .--.   _..._    /   /` '.   \  ",
@"   / .'                                |__| .'     '. .   |     \  '  ",
@"  . '                                  .--..   .-.   .|   '      |  ' ",
@"  | |                 __               |  ||  '   '  |\    \     / /  ",
@"  | |              .:--.'.         _   |  ||  |   |  | `.   ` ..' /   ",
@"  . '             / |   \ |      .' |  |  ||  |   |  |    '-...-'`    ",
@"   \ '.          .`' __ | |     .   | /|  ||  |   |  |                ",
@"    '. `._____.-'/ .'.''| |   .'.'| |//|__||  |   |  |                ",
@"      `-.______ / / /   | |_.'.'.-'  /     |  |   |  |                ",
@"               `  \ \._,\ '/.'   \_.'      |  |   |  |                ",
@"                   `--'  `'                '--'   '--'                ",
@"                                                                      " };
        }
    }
}
