using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KinectV2EmguCV.Utils
{
    /// <summary>
    /// Relay command class used to bind UI commands to code in a easy to read manner
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Properties

        private readonly Action<object> executeAction;
        private readonly Predicate<object> canExecuteAction;

        #endregion

        /// <summary>
        /// Default constructor for the class
        /// </summary>
        /// <param name="execute">binding action, for UI typically is call lambda</param>
        public RelayCommand(Action<object> execute)
            : this(execute, _ => true)
        {
        }
        /// <summary>
        /// Alternate constructor that verifies if the action can be executed
        /// </summary>
        /// <param name="action">binding action, for UI typically is call lambda</param>
        /// <param name="canExecute">Predicade object that defines the execution criterias</param>
        public RelayCommand(Action<object> action, Predicate<object> canExecute)
        {
            executeAction = action;
            canExecuteAction = canExecute;
        }

        #region Methods

        /// <inheritdoc />
        public bool CanExecute(object parameter)
        {
            return canExecuteAction(parameter);
        }


        /// <inheritdoc />
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <inheritdoc />
        public void Execute(object parameter)
        {
            executeAction(parameter);
        }

        #endregion
    }
}
