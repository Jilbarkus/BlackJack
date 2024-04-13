using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGames
{
    public class PlayerSave
    {
        public string Name => name;
        public decimal Cash => cash;
        public void AddCash(decimal deltaCash) => cash = cash += deltaCash;

        protected decimal cash = 0.0M;
        protected string name = "PlayerName";

        public PlayerSave(decimal cash, string nameIn)
        {
            this.cash = cash;
            name = nameIn ?? throw new ArgumentNullException(nameof(nameIn));
            if (nameIn.Split('.', ' ').Length > 1) name = nameIn.Split('.', ' ')[0];
        }
    }
}
