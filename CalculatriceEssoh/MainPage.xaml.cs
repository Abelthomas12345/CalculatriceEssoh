namespace CalculatriceEssoh
{
    public partial class MainPage : ContentPage
    {
        private readonly CalculatorEngine _engine = new();
        private bool? _isLandscape;

        public MainPage()
        {
            InitializeComponent();
            Refresh();
        }

        // Adaptation à la taille et à l'orientation de l'écran
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width <= 0 || height <= 0) return;

            bool landscape = width > height;
            // La grille occupe tout l'écran ; en dessous d'un minimum, le ScrollView prend le relais
            RootGrid.HeightRequest = Math.Max(height, landscape ? 320 : 480);

            if (_isLandscape == landscape) return;
            _isLandscape = landscape;

            if (landscape)
            {
                RootGrid.RowDefinitions = new RowDefinitionCollection { new RowDefinition(GridLength.Star) };
                RootGrid.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star)
            };
                Grid.SetRow(DisplayBorder, 0); Grid.SetColumn(DisplayBorder, 0);
                Grid.SetRow(Keypad, 0); Grid.SetColumn(Keypad, 1);
            }
            else
            {
                RootGrid.RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition(new GridLength(2, GridUnitType.Star)),
                new RowDefinition(new GridLength(5, GridUnitType.Star))
            };
                RootGrid.ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Star) };
                Grid.SetRow(DisplayBorder, 0); Grid.SetColumn(DisplayBorder, 0);
                Grid.SetRow(Keypad, 1); Grid.SetColumn(Keypad, 0);
            }
        }

        private void OnDigitClicked(object? sender, EventArgs e) { _engine.InputDigit(((Button)sender!).Text[0]); Refresh(); }
        private void OnDecimalClicked(object? sender, EventArgs e) { _engine.InputDecimal(); Refresh(); }
        private void OnOperatorClicked(object? sender, EventArgs e) { _engine.SetOperator(((Button)sender!).Text[0]); Refresh(); }
        private void OnEqualsClicked(object? sender, EventArgs e) { _engine.Equals(); Refresh(); }
        private void OnClearClicked(object? sender, EventArgs e) { _engine.Clear(); Refresh(); }
        private void OnBackspaceClicked(object? sender, EventArgs e) { _engine.Backspace(); Refresh(); }
        private void OnSignClicked(object? sender, EventArgs e) { _engine.ToggleSign(); Refresh(); }
        private void OnPercentClicked(object? sender, EventArgs e) { _engine.Percent(); Refresh(); }

        private void Refresh()
        {
            string text = _engine.Display;
            ResultLabel.Text = text;
            ExpressionLabel.Text = _engine.Expression;
            ResultLabel.LineBreakMode = _engine.IsError ? LineBreakMode.WordWrap : LineBreakMode.NoWrap;
            ResultLabel.FontSize = _engine.IsError ? 22
                : text.Length <= 9 ? 56 : text.Length <= 12 ? 44 : text.Length <= 15 ? 36 : 30;
        }
    }

}
