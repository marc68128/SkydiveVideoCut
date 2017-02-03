using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace SkydivingVideoCut.Helpers
{
    public class ActionCommand : ICommand
    {
        #region Events

        public event EventHandler CanExecuteChanged;

        #endregion // Events

        #region Instance variables

        readonly private Action m_Handler;

        private bool m_IsEnabled;

        #endregion // Instance variables

        #region Properties

        /// <summary>
        /// Set or get the execution status of the command
        /// Trigger an event if the value changed in order the caller can process necessary updates
        /// </summary>
        public bool IsEnabled
        {
            get { return m_IsEnabled; }
            set
            {
                if (value != m_IsEnabled)
                {
                    m_IsEnabled = value;
                    if (CanExecuteChanged != null)
                    {
                        Dispatcher.CurrentDispatcher.Invoke(() => CanExecuteChanged(this, EventArgs.Empty));
                    }
                }
            }
        }

        #endregion // Properties

        #region Constructors

        /// <summary>
        /// <see cref="ActionCommand"/> constructor.
        /// </summary>
        /// <param name="mHandler">Action to be executed</param>
        /// <param name="mIsEnabled">Define if the command is enabled or not. Default value is true</param>
        public ActionCommand(Action mHandler, bool mIsEnabled = true)
        {
            m_Handler = mHandler;
            m_IsEnabled = mIsEnabled;
        }

        #endregion // Constructors

        #region Methods

        /// <summary>
        /// Define if the command the command can be executed or not
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns>return true or false</returns>
        public bool CanExecute(object parameter)
        {
            return IsEnabled;
        }

        public void Execute(object parameter)
        {
            IsEnabled = false;
            Dispatcher.CurrentDispatcher.Invoke(m_Handler);
            IsEnabled = true;
        }

        #endregion // Methods
    }

    /// <summary>
    /// Implementation of a generic ActionCommand<T> that can be bounded to a View via a ViewModel
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    public class ActionCommand<TParam> : ICommand
    {
        #region Events

        public event EventHandler CanExecuteChanged;

        #endregion // Events

        #region Instance variables

        readonly private Action<TParam> m_Handler;
        private bool m_IsEnabled;

        #endregion // Instance variables

        #region Properties

        /// <summary>
        /// Set or get the execution status of the command
        /// Trigger an event if the value changed in order the caller can process necessary updates
        /// </summary>
        public bool IsEnabled
        {
            get { return m_IsEnabled; }
            set
            {
                if (value != m_IsEnabled)
                {
                    m_IsEnabled = value;
                    if (CanExecuteChanged != null)
                    {
                        Dispatcher.CurrentDispatcher.Invoke(() => CanExecuteChanged(this, EventArgs.Empty));
                    }
                }
            }
        }

        #endregion // Properties

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="mHandler">Action to be executed</param>
        /// <param name="mIsEnabled">Define if the command is enabled or not. Default value is true</param>
        public ActionCommand(Action<TParam> mHandler, bool mIsEnabled = true)
        {
            this.m_Handler = mHandler;
            m_IsEnabled = mIsEnabled;
        }

        #endregion // Constructors

        #region Methods

        /// <summary>
        /// Define if the command the command can be executed or not
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns>return true or false</returns>
        public bool CanExecute(object parameter)
        {
            return IsEnabled;
        }


        public void Execute(object parameter)
        {
            Dispatcher.CurrentDispatcher.Invoke(() => m_Handler((TParam)parameter));
        }

        #endregion // Methods
    }
}
