using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGames
{
    public interface IGame
    {
        public abstract bool Run(ref PlayerSave player);
    }
}
