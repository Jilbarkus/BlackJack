using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Threading.Tasks;

namespace CardGames
{
    internal class TestGame : IGame
    {
        public bool Run(ref PlayerSave player)
        {
            Console.Clear();

            //return EmptyDeckValueTest();
            //return BestHandChooserTest();
            //return SplitViabilityTest();
            //return DeckValueTest();
            return SaveLoadTest(player);
        }

        private bool BestHandChooserTest()
        {
            Console.Clear();
            new string[] { $"Best Hand Out Of Two Chooser", "","Type Deck A value then press Enter:", "", "X to Quit","" }.PrintArrayToConsole(GlobalFunctions.ConsoleCursorLocation, JustifyX.Center, JustifyY.Center);
            string? usrInput = Console.ReadLine();
            if (usrInput == null || (usrInput != null && (usrInput == "x" || usrInput == "X"))) { return false; }
            else if (usrInput != null && int.TryParse(usrInput, out int handA))
            {
                Console.Clear();
                new string[] { "Type Deck B value then press Enter:", "", "X to Quit", "" }.PrintArrayToConsole(GlobalFunctions.ConsoleCursorLocation, JustifyX.Center, JustifyY.Center);
                usrInput = Console.ReadLine();
                if (usrInput == null || (usrInput != null && (usrInput == "x" || usrInput == "X"))) { return false; }
                else if (usrInput != null && int.TryParse(usrInput, out int handB))
                {
                    Console.Clear();
                    new string[] { $"A:{handA} versus B:{handB},     RESULT: {BlackJack.GetBestBlackJackValue(handA, handB)}", "", "X to Quit", "" }.PrintArrayToConsole(GlobalFunctions.ConsoleCursorLocation, JustifyX.Center, JustifyY.Center);
                    char usrInput2 = GetUserKeyToChar();
                    if (usrInput2=='x'|| usrInput2 == 'X') return false;
                    else return true;
                }
                else return true;
            }
            else return true;
        }

        private bool SplitViabilityTest()
        {
            Console.Clear();
            new string[] { 
                $"Two Aces Splittable: {BlackJack.CardsAreSplittable(new NumberCard(aCard.Suit.Spade, 1), new NumberCard(aCard.Suit.Heart, 1))}",
                $"Two 10s Splittable: {BlackJack.CardsAreSplittable(new NumberCard(aCard.Suit.Spade, 10), new NumberCard(aCard.Suit.Heart, 10))}",
                $"An ace and a 10 Splittable: {BlackJack.CardsAreSplittable(new NumberCard(aCard.Suit.Spade, 10), new NumberCard(aCard.Suit.Heart, 1))}",
                $"Two Kings: {BlackJack.CardsAreSplittable(new FaceCard(aCard.Suit.Spade, FaceCard.Face.King), new FaceCard(aCard.Suit.Diamond, FaceCard.Face.King))}",
                $"King and Jack: {BlackJack.CardsAreSplittable(new FaceCard(aCard.Suit.Spade, FaceCard.Face.Jack), new FaceCard(aCard.Suit.Diamond, FaceCard.Face.King))}",
                $"Ace and Jack: {BlackJack.CardsAreSplittable(new NumberCard(aCard.Suit.Spade, 1), new FaceCard(aCard.Suit.Diamond, FaceCard.Face.King))}"
            }.PrintArrayToConsole(GlobalFunctions.ConsoleCursorLocation, JustifyX.Center, JustifyY.Center);
            char keyToChar = GetUserKeyToChar();
            if (keyToChar == 'X' || Char.ToUpper(keyToChar) == 'X') return false;
            else return true;
        }

        private bool EmptyDeckValueTest()
        {
            aCard[] emptyDeck = new aCard[0];
            new string[] { $"empty deck value: {BlackJack.GetBlackJackValue(emptyDeck)}", "", "X to Quit" }.PrintArrayToConsole(GlobalFunctions.ConsoleCursorLocation, JustifyX.Center, JustifyY.Center);
            char keyToChar = GetUserKeyToChar();
            if (keyToChar == 'X' || Char.ToUpper(keyToChar) == 'X') return false;
            else return true;
        }

        private bool DeckValueTest()
        {
            Console.Clear();
            aCard[] hand = new aCard[0];
            bool bKeepGoing = true;
            Console.WriteLine("Enter value for number card");
            Console.WriteLine("Letter for face card");
            Console.WriteLine("Blank to reset hand");
            Console.WriteLine("X to Quit");
            while (bKeepGoing)
            {
                Console.WriteLine($"hand value: {BlackJack.GetBlackJackValue(hand)}");
                string? usrInput = Console.ReadLine();
                if (usrInput == null || (usrInput != null && (usrInput == "x" || usrInput == "X"))) { return false; }
                else if (usrInput != null &&
                    int.TryParse(usrInput, out int numCardValue) &&
                    0 < numCardValue &&
                    numCardValue < 11)
                {
                    NumberCard numberCard = new NumberCard(aCard.Suit.Heart, numCardValue);
                    if (numberCard != null)
                    {
                        hand = hand.Append(numberCard).ToArray();
                    }
                }
                else if (usrInput != null && 
                    (usrInput == "K" || usrInput == "k"))
                {
                    FaceCard faceCard = new FaceCard(aCard.Suit.Heart, FaceCard.Face.King);
                    if (faceCard != null)
                    {
                        hand = hand.Append(faceCard).ToArray();
                    }
                }
                else if (usrInput != null &&
                    (usrInput == "Q" || usrInput == "q"))
                {
                    FaceCard faceCard = new FaceCard(aCard.Suit.Heart, FaceCard.Face.Queen);
                    if (faceCard != null)
                    {
                        hand = hand.Append(faceCard).ToArray();
                    }
                }
                else if (usrInput != null &&
                    (usrInput == "J" || usrInput == "j"))
                {
                    FaceCard faceCard = new FaceCard(aCard.Suit.Heart, FaceCard.Face.Jack);
                    if (faceCard != null)
                    {
                        hand = hand.Append(faceCard).ToArray();
                    }
                }
                else if (usrInput != null &&
                    (usrInput == "" || usrInput == " "))
                {
                    hand = new aCard[0];
                }
            }

            return false;
        }

        private char GetUserKeyToChar()
        {
            ConsoleKeyInfo keyPressed;
            char keyToChar = '-';
            try
            {
                keyPressed = Console.ReadKey();
                keyToChar = keyPressed.KeyChar;
            }
            catch
            {
                keyToChar = '-';
            }
            return keyToChar;
        }

        private bool SaveLoadTest(PlayerSave player)
        {
            Console.Clear();
            Console.WriteLine("D to display file path");
            Console.WriteLine("S to save");
            Console.WriteLine("L to load");
            Console.WriteLine("X to quit");
            
            while (true)
            {
                char usrInput = GetUserKeyToChar();
                if (usrInput == 'x' || usrInput == 'X') return false;
                if (usrInput == 'd' || usrInput == 'D') DisplayPath();
                //if (usrInput == 's' || usrInput == 'S') Save2();
                //if (usrInput == 'l' || usrInput == 'L') Load2();
                //if (usrInput == 's' || usrInput == 'S') Save3();
                //if (usrInput == 'l' || usrInput == 'L') Load3();
                if (usrInput == 's' || usrInput == 'S') Save4(player);
                if (usrInput == 'l' || usrInput == 'L') Load4(ref player);
            }
            return false;
        }

        private void DisplayPath()
        {
            Console.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        }

        //private void Save()
        //{
        //    //PlayerSave player = new PlayerSave(69.0m, "test player");
        //    Random random = new Random();
        //    PlayerSave player = new PlayerSave("test player", (decimal)random.Next(0, 5000));
            
        //    string jsonTxt = JsonSerializer.Serialize<PlayerSave>(player);
        //    byte[] jsonBytes = Encoding.ASCII.GetBytes(jsonTxt);
        //    string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\bjData.bjd";

        //    try
        //    {
        //        File.WriteAllText(path, jsonTxt);
        //        Console.WriteLine($"saved player: {player.Name}, cash: ${player.Cash}");
        //        //using (FileStream fs = File.Create(path))
        //        //{
        //        //    fs.Write(jsonBytes, 0, jsonBytes.Length);
        //        //    Console.WriteLine($"saved player: {player.Name}, cash: ${player.Cash}");
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //    }
            
            
        //}

        //private void Load()
        //{
        //    string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\bjData.bjd";
        //    if (File.Exists(path) == false) return;

        //    try
        //    {
        //        //using (StreamReader sr = new StreamReader(path))
        //        //{
        //        //    string jsonTxt = sr.ReadToEnd();
        //        //    if (jsonTxt != null && jsonTxt.Length > 0)
        //        //    {
        //        //        PlayerSave? outputDemo = (PlayerSave?)JsonSerializer.Deserialize<PlayerSave>(jsonTxt);
        //        //        if (outputDemo == null) { Console.WriteLine("FAIL!"); return; }
        //        //        Console.WriteLine($"Loaded player: {outputDemo.Name}, cash: ${outputDemo.Cash}");

        //        //        //string jSonTxt = JsonSerializer.Deserialize<string>(jsonTxt);
        //        //        //if (outputDemo == null) { Console.WriteLine("FAIL!"); return; }
        //        //        //Console.WriteLine($"Loaded player: {outputDemo.Name}, cash: ${outputDemo.Cash}");
        //        //    }
        //        //}

        //        string jsonString = File.ReadAllText(path);
        //        PlayerSave? player = JsonSerializer.Deserialize<PlayerSave>(jsonString);
        //        if (player == null) { Console.WriteLine("FAIL!"); return; }
        //        Console.WriteLine($"Loaded player: {player.Name}, cash: ${player.Cash}");
        //    }
        //    catch(Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //    }

            
        //}

        

        private string GetSavePath => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\bjData.xml";

        private void Save2()
        {
            XmlSerializer serialiser = new XmlSerializer(typeof(TestSaveFile1));
            TextWriter writer = new StreamWriter(GetSavePath);
            TestSaveFile1 saveFile1 = new TestSaveFile1();
            Random random = new Random();
            saveFile1.testValue = (decimal)random.Next(0, 10000);
            serialiser.Serialize(writer, saveFile1);
            writer.Close();
        }

        private void Load2()
        {
            string path = GetSavePath;
            if (File.Exists(path) == false) return;
            XmlSerializer serialiser = new XmlSerializer(typeof(TestSaveFile1));
            serialiser.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serialiser.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
            FileStream fs = new FileStream(path, FileMode.Open);
            TestSaveFile1 saveFile;
            saveFile = (TestSaveFile1)serialiser.Deserialize(fs);
            Console.WriteLine($"Loaded save: {saveFile.FileName}, value: {saveFile.testValue}");
        }

        private void Save3()
        {
            XmlSerializer serialiser = new XmlSerializer(typeof(PlayerSave));
            TextWriter writer = new StreamWriter(GetSavePath);
            Random random = new Random();
            PlayerSave saveFile1 = new PlayerSave("some guy", (decimal)random.Next(0, 10000));
            saveFile1.AddCashAndSave((decimal)random.Next(0, 10000));
            Console.WriteLine($"created save: {saveFile1.Name}, value: {saveFile1.Cash}");
            serialiser.Serialize(writer, saveFile1);
            writer.Close();
        }

        private void Load3()
        {
            string path = GetSavePath;
            if (File.Exists(path) == false) return;
            XmlSerializer serialiser = new XmlSerializer(typeof(PlayerSave));
            serialiser.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serialiser.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
            FileStream fs = new FileStream(path, FileMode.Open);
            PlayerSave saveFile;
            saveFile = (PlayerSave)serialiser.Deserialize(fs);
            Console.WriteLine($"Loaded save: {saveFile.Name}, value: {saveFile.Cash}");
        }

        private void Save4(PlayerSave player)
        {
            //XmlSerializer serialiser = new XmlSerializer(typeof(PlayerSave));
            //TextWriter writer = new StreamWriter(GetSavePath);    // this fail
            //serialiser.Serialize(writer, player);
            //writer.Close();
            player.Save();
        }

        private void Load4(ref PlayerSave player)
        {
            //string path = GetSavePath;
            //if (File.Exists(path) == false) return;
            //XmlSerializer serialiser = new XmlSerializer(typeof(PlayerSave));
            //serialiser.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            //serialiser.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
            //FileStream fs = new FileStream(path, FileMode.Open);
            //PlayerSave? saveFile;
            //saveFile = (PlayerSave?)serialiser.Deserialize(fs);
            //if (saveFile != null) player = saveFile;

            PlayerSave? saveFile = PlayerSave.Load();
            
            if (saveFile != null)
            {
                //player = saveFile;
                player = new PlayerSave(saveFile.Name, saveFile.Cash);
                Console.WriteLine($"Loaded save: {saveFile.Name}, value: {saveFile.Cash}");
            }
        }

        private void serializer_UnknownNode
   (object sender, XmlNodeEventArgs e)
        {
            Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);
        }

        private void serializer_UnknownAttribute
        (object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            Console.WriteLine("Unknown attribute " +
            attr.Name + "='" + attr.Value + "'");
        }

        //private void Save()
        //{
        //    using (var memoryStream = new System.IO.MemoryStream())
        //    {
        //        var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        //        PlayerSave player = new PlayerSave(69.0m, "test player");
        //        formatter.Serialize(memoryStream, player);

        //        // This resets the memory stream position for the following read operation
        //        memoryStream.Seek(0, SeekOrigin.Begin);

        //        // Get the bytes
        //        var bytes = new byte[memoryStream.Length];
        //        memoryStream.Read(bytes, 0, (int)memoryStream.Length);

        //        // TODO: encrypt data here
        //        // - also haven't yet added a Load() method

        //        using (FileStream fs = File.Create(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\bjData.bjd"))
        //        {
        //            fs.Write(bytes, 0, bytes.Length);
        //        }
        //    }
        //}


    }

    [XmlRootAttribute("TestSaveFile1", Namespace = "Casino", IsNullable = false)]
    public class TestSaveFile1
    {
        public string FileName = "testFile";
        public decimal testValue = decimal.Zero;
    }
}
