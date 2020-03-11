using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutrinoStudio.Shell.ViewModels
{

    /// <summary>
    /// The global navigator.
    /// </summary>
    public class Navigator
    {

        #region Static Current

        public static Navigator Current { get; set; } = new Navigator();

        #endregion

    }

}
