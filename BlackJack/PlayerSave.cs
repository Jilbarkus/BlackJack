using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace CardGames
{
    public class PlayerSave
    {
        public string Name => _name;
        public decimal Cash => _cash;
        public void AddCash(decimal deltaCash) => _cash = _cash += deltaCash;
        public void AddCashAndSave(decimal deltaCash)
        {
            _cash = _cash += deltaCash;
            Save();
        }

        protected decimal _cash = 0.0M;
        protected string _name = "PlayerName";
        public PlayerSave(string name, decimal cash)
        {
            this._cash = cash;
            if (name == null || name.Length == 0) { name = "Guest"; }
            this._name = VerifyName(name);
        }

        private string VerifyName(string nameIn)=> (nameIn.Split('.', ' ').Length > 1)? nameIn.Split('.', ' ')[0] : nameIn;

        public void Save()
        {
            PlayerSaveDataFile data = new PlayerSaveDataFile();
            data.Name = this._name;
            data.Cash = this._cash;
            XmlSerializer serialiser = new XmlSerializer(typeof(PlayerSaveDataFile));
            try
            {
                TextWriter writer = new StreamWriter(GlobalFunctions.GetPlayerSavePath);
                serialiser.Serialize(writer, data);
                writer.Close();
            }
            catch (Exception e) { Console.WriteLine(e); }
        }

        public static PlayerSave? Load()
        {
            string path = GlobalFunctions.GetPlayerSavePath;
            // if no file
            if (File.Exists(path) == false) return null;
            XmlSerializer serialiser = new XmlSerializer(typeof(PlayerSaveDataFile));
            serialiser.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serialiser.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
            PlayerSaveDataFile? saveFile = null;

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                try
                {
                    saveFile = (PlayerSaveDataFile?)serialiser.Deserialize(fs);
                }
                catch (Exception e) { Console.WriteLine(e); }
                fs.Close();
            }
            
            if (saveFile != null) return new PlayerSave(saveFile.Name, saveFile.Cash);
            return null;
            
            void serializer_UnknownNode
   (object sender, XmlNodeEventArgs e)
            {
                Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);
            }

            void serializer_UnknownAttribute
            (object sender, XmlAttributeEventArgs e)
            {
                System.Xml.XmlAttribute attr = e.Attr;
                Console.WriteLine("Unknown attribute " +
                attr.Name + "='" + attr.Value + "'");
            }
        }
    }

    [XmlRootAttribute("PlayerSave", Namespace = "Casino", IsNullable = false)]
    public class PlayerSaveDataFile
    {
        public string Name;
        public decimal Cash;
    }
}
