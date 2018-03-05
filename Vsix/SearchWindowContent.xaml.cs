using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using SourcePath.CSharp;
using SourcePath.Roslyn;

namespace SourcePath.Vsix {
    /// <summary>
    /// Interaction logic for ToolWindow1Control.
    /// </summary>
    public partial class SearchWindowContent : UserControl {
        private SearchEngine _searchEngine;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchWindowContent"/> class.
        /// </summary>
        public SearchWindowContent() {
            InitializeComponent();
            var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
            _searchEngine = new SearchEngine(
                new SyntaxQueryParser(),
                new RoslynCSharpSyntaxQueryExecutor(),
                () => componentModel.GetService<VisualStudioWorkspace>()
            );
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private async void buttonSearch_Click(object sender, RoutedEventArgs e) {
            var results = await _searchEngine.SearchAsync(textPath.Text);
            listResults.ItemsSource = results;
        }
    }
}