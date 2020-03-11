using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Musiqual.Editor.Models;
using Scrosser.Models;

namespace NeutrinoStudio.Shell.ViewModels
{

    /// <summary>
    /// The global navigator.
    /// </summary>
    public class Navigator : INotifyPropertyChanged
    {

        #region Static Current

        public static Navigator Current { get; set; } = new Navigator();

        #endregion

        #region Scross

        private Scross _scross = new Scross();

        public Scross Scross
        {
            get => _scross;
            set
            {
                _scross = value;
                OnPropertyChanged();
            }
        }

        private EditMode _editMode = new EditMode();

        public EditMode EditMode
        {
            get => _editMode;
            set
            {
                _editMode = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }

}
