using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AutocompleteTextBox
{
    /// <summary>
    /// Textbox with support for custom autocomplete items.
    /// <remarks>Cannot show more than six autocomplete matches at a time.</remarks>
    /// </summary>
    public sealed class AutocompleteTextBox : TextBox
    {
        #region Properties

        private AutocompleteSearchMode _autoCompleteSearchMode;

        /// <summary>
        /// Sets the mode to use for finding autocomplete terms using the inputted string
        /// </summary>
        public AutocompleteSearchMode AutoCompleteSearchMode
        {
            get { return _autoCompleteSearchMode; }
            set { _autoCompleteSearchMode = value; }
        }

        private bool _isMenuTriggered;
        /// <summary>
        /// Gets the value that determines if the autocomplete menu is visible
        /// </summary>
        public bool IsMenuTriggered
        {
            get { return _isMenuTriggered; }
        }

        private HashSet<string> _items;
        /// <summary>
        /// Gets the collection used to generate the items in the autocomplete menu
        /// </summary>
        public IEnumerable<string> Items
        {
            get { return _items; }
        }

        private static readonly DependencyProperty _itemsSourceProperty = DependencyProperty.Register(
            "ItemsSource",
            typeof(object),
            typeof(AutocompleteTextBox),
            new PropertyMetadata(Guid.Empty, new PropertyChangedCallback(ItemsSourceChanged)));
        /// <summary>
        /// Gets or sets the object source used to generate the items in the autocomplete menu
        /// </summary>
        public object ItemsSource
        {
            get
            {
                return (object)GetValue(_itemsSourceProperty);
            }
            set
            {
                SetValue(_itemsSourceProperty, value);
            }
        }

        private float _popupDelayDuration;
        /// <summary>
        /// Gets or sets the time in seconds for which to delay the autocomplete popup
        /// menu after each keystroke in the textbox.
        /// <remarks>Acceptable values are between 0.0 and 1.0.</remarks>
        /// </summary>
        public float PopupDelayDuration
        {
            get { return _popupDelayDuration; }
            set
            {
                if (value >= 0.0f && value <= 1.0f)
                {
                    _popupDelayDuration = value;
                }
            }
        }

        #endregion

        #region Class variables

        // Could use MenuFlyout (win 8.1+) for richer items
        private PopupMenu _popupMenu;

        private CancellationTokenSource _keyDownCancellationSource;
        private Task _keydownWaitTimer;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the AutocompleteTextbox with no dictionary terms
        /// </summary>
        public AutocompleteTextBox()
            : this(null)
        { }

        /// <summary>
        /// Initializes the AutocompleteTextbox with the given dictionary terms
        /// </summary>
        /// <param name="dictionaryItems">The string terms to add to the autocomplete dictionary</param>
        public AutocompleteTextBox(IEnumerable<string> dictionaryItems)
            : base()
        {
            if (dictionaryItems != null)
            {
                _items = new HashSet<string>(dictionaryItems);
            }
            else
            {
                _items = new HashSet<string>();
            }

            // Disable native text prediction
            IsTextPredictionEnabled = false;

            _popupMenu = new PopupMenu();
            _popupDelayDuration = 0.3f;
            _autoCompleteSearchMode = AutocompleteSearchMode.SearchesWithContains;
            _keyDownCancellationSource = new CancellationTokenSource();
        }

        #endregion

        #region Private methods

        protected override void OnKeyUp(Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);

            int keyValue = (int)e.Key;
            if ((keyValue >= 0x30 && keyValue <= 0x39) // numbers
                || (keyValue >= 0x41 && keyValue <= 0x5A) // letters
                || (keyValue >= 0x60 && keyValue <= 0x69) // numpad
                || keyValue == 0x0020)  // space
            {

                if (_keydownWaitTimer != null &&
                    (_keydownWaitTimer.Status == TaskStatus.WaitingForActivation ||
                    _keydownWaitTimer.Status == TaskStatus.WaitingToRun ||
                    _keydownWaitTimer.Status == TaskStatus.Running))
                {
                    _keyDownCancellationSource.Cancel();
                }

                _keyDownCancellationSource = new CancellationTokenSource();
                _keydownWaitTimer = ShowMatchedItemsPopup(_keyDownCancellationSource.Token);
            }
        }

        private async Task ShowMatchedItemsPopup(CancellationToken cancellationToken)
        {
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(_popupDelayDuration));

            // Check task cancellation
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _popupMenu.Commands.Clear();

            _isMenuTriggered = false;

            string inputString = this.Text.Trim();

            // Check input
            if (inputString == null || inputString.Length < 1)
            {
                return;
            }

            IEnumerable<string> matchingStrings = null;

            if (_autoCompleteSearchMode == AutocompleteSearchMode.SearchesWithContains)
            {
                matchingStrings = _items.Where(f => f.ToLower().Contains(inputString.ToLower())).Take(6);
            }
            else if (_autoCompleteSearchMode == AutocompleteSearchMode.SearchesWithStartsWith)
            {
                matchingStrings = _items.Where(f => f.ToLower().StartsWith(inputString.ToLower())).Take(6);
            }

            foreach (string match in matchingStrings)
            {
                _popupMenu.Commands.Add(new UICommand(
                    match,
                    new UICommandInvokedHandler(MatchItemCommandInvokedHandler)));
            }

            var transform = this.TransformToVisual(Window.Current.Content);
            Point screenCoords = transform.TransformPoint(new Point(0 + (this.ActualWidth / 2), 0 + this.ActualHeight));

            // Check task cancellation for the last time
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await _popupMenu.ShowForSelectionAsync(new Rect(screenCoords, screenCoords), Placement.Below);
                _isMenuTriggered = true;
            }
            catch (InvalidOperationException)
            {
                /* This exception may occur when we have too many calls to ShowForSelectionAsync.
                 * It will seldom ever happen since, the async calls to ShowMatchedItemsPopup are
                 * managed.
                 */
            }

            matchingStrings = null;
        }

        private void MatchItemCommandInvokedHandler(IUICommand command)
        {
            this.Text = command.Label;
        }

        private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue is IEnumerable<string>)
            {
                var thisControl = (AutocompleteTextBox)d;

                thisControl._items = new HashSet<string>((IEnumerable<string>)e.NewValue);
            }
        }

        #endregion
    }

    public enum AutocompleteSearchMode
    {
        SearchesWithContains,
        SearchesWithStartsWith
    }
}
