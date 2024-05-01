using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Security.Cryptography;
using System.Security;
using System.Text;

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
            SaveSecure();
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

        //public void Save()
        //{
        //    PlayerSaveDataFile data = new PlayerSaveDataFile();
        //    data.Name = this._name;
        //    data.Cash = this._cash;
        //    XmlSerializer serialiser = new XmlSerializer(typeof(PlayerSaveDataFile));

        //    try
        //    {
        //        using (FileStream stream = new FileStream(GlobalFunctions.GetPlayerSavePath, FileMode.Create))
        //        {
        //            System.Security.Cryptography.TripleDES DES = TripleDES.Create();
        //            //byte[] keyBytes = ASCIIEncoding.ASCII.GetBytes(GlobalFunctions.DESKey);

        //            DES.Key = ASCIIEncoding.ASCII.GetBytes(GlobalFunctions.DESKey);
        //            DES.IV = ASCIIEncoding.ASCII.GetBytes(GlobalFunctions.DESKey);

        //            ICryptoTransform desencrypt = DES.CreateEncryptor();

        //            using (CryptoStream cStream = new CryptoStream(stream, desencrypt, CryptoStreamMode.Write))
        //            {
        //                serialiser.Serialize(cStream, data);
        //                cStream.Close();
        //            }
        //            stream.Close();
        //        }
        //    }
        //    catch (Exception e) { Console.WriteLine(e); }
        //}

        //public void Save()
        //{
        //    PlayerSaveDataFile data = new PlayerSaveDataFile();
        //    data.Name = this._name;
        //    data.Cash = this._cash;
        //    XmlSerializer serialiser = new XmlSerializer(typeof(PlayerSaveDataFile));

        //    try
        //    {
        //        using (FileStream stream = new FileStream(GlobalFunctions.GetPlayerSavePath, FileMode.Create))
        //        {
        //            System.Security.Cryptography.TripleDES DES = TripleDES.Create();
        //            byte[] keyBytes = ASCIIEncoding.ASCII.GetBytes("qwertyuiopasdfghjklzx");

        //            DES.Key = keyBytes;
        //            DES.IV = keyBytes;

        //            ICryptoTransform desencrypt = DES.CreateEncryptor();

        //            using (CryptoStream cStream = new CryptoStream(stream, desencrypt, CryptoStreamMode.Write))
        //            {
        //                serialiser.Serialize(cStream, data);
        //                cStream.Close();
        //            }
        //            stream.Close();
        //        }
        //    }
        //    catch (Exception e) { Console.WriteLine(e); }
        //}

        //purposefuly obfuscated: Key = name, Value = cash
        public void SaveSecure()
        {
            PlayerSaveDataFileText data = new PlayerSaveDataFileText();
            data.Key = GlobalFunctions.Encrypt(this._name);
            data.Value = GlobalFunctions.Encrypt(this._cash.ToString("F2")); 
            XmlSerializer serialiser = new XmlSerializer(typeof(PlayerSaveDataFileText));
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

        public static PlayerSave? LoadSecure()
        {
            string path = GlobalFunctions.GetPlayerSavePath;
            // if no file
            if (File.Exists(path) == false) return null;
            XmlSerializer serialiser = new XmlSerializer(typeof(PlayerSaveDataFileText));
            serialiser.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            serialiser.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
            PlayerSaveDataFileText? saveFile = null;

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                try
                {
                    saveFile = (PlayerSaveDataFileText?)serialiser.Deserialize(fs);
                }
                catch (Exception e) { Console.WriteLine(e); }
                fs.Close();
            }

            //if (saveFile != null) return new PlayerSave(saveFile.Name, saveFile.Cash);
            if (saveFile != null)
            {
                string name = GlobalFunctions.Decrypt(saveFile.Key);
                string decryptedCashString = GlobalFunctions.Decrypt(saveFile.Value);
                if (decimal.TryParse(decryptedCashString, out decimal cash))
                {
                    return new PlayerSave(name, cash);
                }
            }
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

        //     public static PlayerSave? Load()
        //     {
        //         string path = GlobalFunctions.GetPlayerSavePath;
        //         // if no file
        //         if (File.Exists(path) == false) return null;
        //         XmlSerializer serialiser = new XmlSerializer(typeof(PlayerSaveDataFile));
        //         serialiser.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
        //         serialiser.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
        //         PlayerSaveDataFile? saveFile = null;

        //         using (FileStream fs = new FileStream(path, FileMode.Open))
        //         {
        //             try
        //             {
        //                 TripleDES DES = TripleDES.Create();
        //                 //byte[] desBytes = ASCIIEncoding.ASCII.GetBytes(GlobalFunctions.DESKey);
        //                 DES.Key = ASCIIEncoding.ASCII.GetBytes(GlobalFunctions.DESKey16);
        //                 DES.IV = ASCIIEncoding.ASCII.GetBytes(GlobalFunctions.DESKey16);

        //                 ICryptoTransform desdecrypt = DES.CreateDecryptor();
        //                 using (CryptoStream cStream = new CryptoStream(fs, desdecrypt, CryptoStreamMode.Read))
        //                 {
        //                     saveFile = (PlayerSaveDataFile?)serialiser.Deserialize(cStream);
        //                     cStream.Close();
        //                 }

        //                 //saveFile = (PlayerSaveDataFile?)serialiser.Deserialize(fs);
        //             }
        //             catch (Exception e) { Console.WriteLine(e); }
        //             fs.Close();
        //         }

        //         if (saveFile != null) return new PlayerSave(saveFile.Name, saveFile.Cash);
        //         return null;

        //         void serializer_UnknownNode
        //(object sender, XmlNodeEventArgs e)
        //         {
        //             Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);
        //         }

        //         void serializer_UnknownAttribute
        //         (object sender, XmlAttributeEventArgs e)
        //         {
        //             System.Xml.XmlAttribute attr = e.Attr;
        //             Console.WriteLine("Unknown attribute " +
        //             attr.Name + "='" + attr.Value + "'");
        //         }
        //     }
    }

    [XmlRootAttribute("PlayerSave", Namespace = "Casino", IsNullable = false)]
    public class PlayerSaveDataFile
    {
        public string Name;
        public decimal Cash;
    }

    [XmlRootAttribute("PlayerSave", Namespace = "Casino", IsNullable = false)]
    public class PlayerSaveDataFileText
    {
        public string Key;
        public string Value;
    }
}
