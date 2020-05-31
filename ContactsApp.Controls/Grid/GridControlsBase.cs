using Microsoft.AspNetCore.Components;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ContactsApp.Controls.Grid
{
    /// <summary>
    /// Simplifies responding to changes to the grid sorts. Related
    /// controls can inherit from this.
    /// </summary>
    public class GridControlsBase : ComponentBase, IDisposable
    {
        private bool _disposedValue;

        /// <summary>
        /// Get the corresponding controls.
        /// </summary>
        [Inject]
        public GridControls Controls { get; set; }

        /// <summary>
        /// Overriding this will determine what change notifications will automatically
        /// trigger <c>StateHasChanged</c> in the inheriting control. Must return
        /// <c>true</c> to auto-refresh.
        /// </summary>
        protected virtual Predicate<string> PropertyFilter { get; } = str => true;

        /// <summary>
        /// When overloaded, is called regardless of the filter.
        /// </summary>
        protected virtual Task OnGridChangedAsync(string propertyName)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Listen for changes.
        /// </summary>
        protected override void OnInitialized()
        {
            Controls.OnGridControlsChanged += Controls_OnGridControlsChanged;
        }

        /// <summary>
        /// Refresh state.
        /// </summary>
        /// <param name="sender">Grid controls.</param>
        /// <param name="e">Property that changed.</param>
        private async void Controls_OnGridControlsChanged(object sender, PropertyChangedEventArgs e)
        {
            // only if opting for auto-refresh
            if (PropertyFilter(e.PropertyName))
            {
                await InvokeAsync(() => StateHasChanged());
            }
            await OnGridChangedAsync(e.PropertyName);
        }

        /// <summary>
        /// Dispose and properly unregister the change handler.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Controls.OnGridControlsChanged -= Controls_OnGridControlsChanged;
                }

                _disposedValue = true;
            }
        }

        /// <summary>
        /// Dispose pattern
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
