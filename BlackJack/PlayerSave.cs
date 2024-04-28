using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CardGames
{
    [System.Serializable]
    public class PlayerSave
    {
        public string Name => _name;
        public decimal Cash => _cash;
        public void AddCash(decimal deltaCash) => _cash = _cash += deltaCash;

        protected decimal _cash = 0.0M;
        protected string _name = "PlayerName";

        [JsonConstructor]
        public PlayerSave(string _name, decimal _cash)
        {
            this._cash = _cash;
            this._name = _name ?? throw new ArgumentNullException(nameof(_name));
            if (_name.Split('.', ' ').Length > 1) this._name = _name.Split('.', ' ')[0];
        }


        //public string Name = "playerNmae";
        //public decimal Cash = 0.0M;

        //public void AddCash(decimal deltaCash) => Cash = Cash += deltaCash;

        //[JsonConstructor]
        //public PlayerSave(string _name, decimal _cash)
        //{
        //    this.Cash = _cash;
        //    this.Name = _name ?? throw new ArgumentNullException(nameof(_name));
        //    if (_name.Split('.', ' ').Length > 1) this.Name = _name.Split('.', ' ')[0];
        //}
    }
}
