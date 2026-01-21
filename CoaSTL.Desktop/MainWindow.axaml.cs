using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CoaSTL.Core;
using CoaSTL.Core.Export;
using CoaSTL.Core.Models;
using CoaSTL.Core.Printers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CoaSTL.Desktop;

public partial class MainWindow : Window
{
    private readonly CoasterDesigner _designer;
    private string? _loadedImagePath;
    
    // UI controls
    private ComboBox? _shapeComboBox;
    private Slider? _diameterSlider;
    private Slider? _thicknessSlider;
    private Slider? _heightSlider;
    private ComboBox? _edgeStyleComboBox;
    private Slider? _polygonSidesSlider;
    private TextBlock? _polygonSidesLabel;
    private StackPanel? _polygonSidesPanel;
    private Slider? _reliefDepthSlider;
    private CheckBox? _invertReliefCheckBox;
    private TextBox? _textInputBox;
    private Slider? _fontSizeSlider;
    private Slider? _textDepthSlider;
    private CheckBox? _embossedCheckBox;
    private CheckBox? _nonSlipCheckBox;
    private CheckBox? _drainageCheckBox;
    private ComboBox? _printerComboBox;
    private TextBlock? _statusText;
    private TextBlock? _imageStatusText;
    private TextBlock? _printerInfoText;
    private Border? _previewStatsPanel;
    private TextBlock? _previewInfoText;
    private TextBlock? _triangleCountText;
    private TextBlock? _dimensionsText;
    private TextBlock? _filamentText;
    private TextBlock? _printTimeText;

    // Value display texts
    private TextBlock? _diameterText;
    private TextBlock? _thicknessText;
    private TextBlock? _heightText;
    private TextBlock? _polygonSidesText;
    private TextBlock? _reliefDepthText;
    private TextBlock? _fontSizeText;
    private TextBlock? _textDepthText;

    private static readonly Dictionary<string, BambuPrinterProfile> PrinterProfiles = new()
    {
        ["X1 Carbon"] = BambuPrinterProfiles.X1Carbon,
        ["X1E"] = BambuPrinterProfiles.X1E,
        ["P1P"] = BambuPrinterProfiles.P1P,
        ["P1S"] = BambuPrinterProfiles.P1S,
        ["P2S"] = BambuPrinterProfiles.P2S,
        ["A1"] = BambuPrinterProfiles.A1,
        ["A1 mini"] = BambuPrinterProfiles.A1Mini
    };

    public MainWindow()
    {
        InitializeComponent();
        _designer = new CoasterDesigner();
        
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // Find controls
        _shapeComboBox = this.FindControl<ComboBox>("ShapeComboBox");
        _diameterSlider = this.FindControl<Slider>("DiameterSlider");
        _thicknessSlider = this.FindControl<Slider>("ThicknessSlider");
        _heightSlider = this.FindControl<Slider>("HeightSlider");
        _edgeStyleComboBox = this.FindControl<ComboBox>("EdgeStyleComboBox");
        _polygonSidesSlider = this.FindControl<Slider>("PolygonSidesSlider");
        _polygonSidesLabel = this.FindControl<TextBlock>("PolygonSidesLabel");
        _polygonSidesPanel = this.FindControl<StackPanel>("PolygonSidesPanel");
        _reliefDepthSlider = this.FindControl<Slider>("ReliefDepthSlider");
        _invertReliefCheckBox = this.FindControl<CheckBox>("InvertReliefCheckBox");
        _textInputBox = this.FindControl<TextBox>("TextInputBox");
        _fontSizeSlider = this.FindControl<Slider>("FontSizeSlider");
        _textDepthSlider = this.FindControl<Slider>("TextDepthSlider");
        _embossedCheckBox = this.FindControl<CheckBox>("EmbossedCheckBox");
        _nonSlipCheckBox = this.FindControl<CheckBox>("NonSlipCheckBox");
        _drainageCheckBox = this.FindControl<CheckBox>("DrainageCheckBox");
        _printerComboBox = this.FindControl<ComboBox>("PrinterComboBox");
        _statusText = this.FindControl<TextBlock>("StatusText");
        _imageStatusText = this.FindControl<TextBlock>("ImageStatusText");
        _printerInfoText = this.FindControl<TextBlock>("PrinterInfoText");
        _previewStatsPanel = this.FindControl<Border>("PreviewStatsPanel");
        _previewInfoText = this.FindControl<TextBlock>("PreviewInfoText");
        _triangleCountText = this.FindControl<TextBlock>("TriangleCountText");
        _dimensionsText = this.FindControl<TextBlock>("DimensionsText");
        _filamentText = this.FindControl<TextBlock>("FilamentText");
        _printTimeText = this.FindControl<TextBlock>("PrintTimeText");

        // Value display texts
        _diameterText = this.FindControl<TextBlock>("DiameterText");
        _thicknessText = this.FindControl<TextBlock>("ThicknessText");
        _heightText = this.FindControl<TextBlock>("HeightText");
        _polygonSidesText = this.FindControl<TextBlock>("PolygonSidesText");
        _reliefDepthText = this.FindControl<TextBlock>("ReliefDepthText");
        _fontSizeText = this.FindControl<TextBlock>("FontSizeText");
        _textDepthText = this.FindControl<TextBlock>("TextDepthText");

        // Buttons
        var newButton = this.FindControl<Button>("NewButton");
        var openTemplateButton = this.FindControl<Button>("OpenTemplateButton");
        var saveTemplateButton = this.FindControl<Button>("SaveTemplateButton");
        var loadImageButton = this.FindControl<Button>("LoadImageButton");
        var generatePreviewButton = this.FindControl<Button>("GeneratePreviewButton");
        var exportStlButton = this.FindControl<Button>("ExportStlButton");
        var export3MfButton = this.FindControl<Button>("Export3MfButton");

        // Wire up event handlers
        if (_shapeComboBox != null) _shapeComboBox.SelectionChanged += OnShapeChanged;
        if (_diameterSlider != null) _diameterSlider.ValueChanged += OnDiameterChanged;
        if (_thicknessSlider != null) _thicknessSlider.ValueChanged += OnThicknessChanged;
        if (_heightSlider != null) _heightSlider.ValueChanged += OnHeightChanged;
        if (_polygonSidesSlider != null) _polygonSidesSlider.ValueChanged += OnPolygonSidesChanged;
        if (_reliefDepthSlider != null) _reliefDepthSlider.ValueChanged += OnReliefDepthChanged;
        if (_fontSizeSlider != null) _fontSizeSlider.ValueChanged += OnFontSizeChanged;
        if (_textDepthSlider != null) _textDepthSlider.ValueChanged += OnTextDepthChanged;
        if (_printerComboBox != null) _printerComboBox.SelectionChanged += OnPrinterChanged;
        
        if (newButton != null) newButton.Click += OnNewClick;
        if (openTemplateButton != null) openTemplateButton.Click += OnOpenTemplateClick;
        if (saveTemplateButton != null) saveTemplateButton.Click += OnSaveTemplateClick;
        if (loadImageButton != null) loadImageButton.Click += OnLoadImageClick;
        if (generatePreviewButton != null) generatePreviewButton.Click += OnGeneratePreviewClick;
        if (exportStlButton != null) exportStlButton.Click += OnExportStlClick;
        if (export3MfButton != null) export3MfButton.Click += OnExport3MfClick;

        // Initialize displays
        UpdatePrinterInfo();
        SetStatus("Ready - Select your coaster options and generate a preview");
    }

    private void OnShapeChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_shapeComboBox?.SelectedItem is ComboBoxItem item)
        {
            var showPolygonSides = item.Content?.ToString() == "Custom Polygon";
            if (_polygonSidesLabel != null) _polygonSidesLabel.IsVisible = showPolygonSides;
            if (_polygonSidesPanel != null) _polygonSidesPanel.IsVisible = showPolygonSides;
        }
    }

    private void OnDiameterChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_diameterText != null)
            _diameterText.Text = $"{e.NewValue:F0} mm";
    }

    private void OnThicknessChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_thicknessText != null)
            _thicknessText.Text = $"{e.NewValue:F1} mm";
    }

    private void OnHeightChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_heightText != null)
            _heightText.Text = $"{e.NewValue:F1} mm";
    }

    private void OnPolygonSidesChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_polygonSidesText != null)
            _polygonSidesText.Text = $"{(int)e.NewValue}";
    }

    private void OnReliefDepthChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_reliefDepthText != null)
            _reliefDepthText.Text = $"{e.NewValue:F1} mm";
    }

    private void OnFontSizeChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_fontSizeText != null)
            _fontSizeText.Text = $"{e.NewValue:F0} mm";
    }

    private void OnTextDepthChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_textDepthText != null)
            _textDepthText.Text = $"{e.NewValue:F1} mm";
    }

    private void OnPrinterChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdatePrinterInfo();
    }

    private void UpdatePrinterInfo()
    {
        if (_printerComboBox?.SelectedItem is ComboBoxItem item && _printerInfoText != null)
        {
            var printerName = item.Content?.ToString();
            if (printerName != null && PrinterProfiles.TryGetValue(printerName, out var profile))
            {
                _printerInfoText.Text = $"Build Volume: {profile.BuildVolumeX}×{profile.BuildVolumeY}×{profile.BuildVolumeZ} mm";
            }
        }
    }

    private void OnNewClick(object? sender, RoutedEventArgs e)
    {
        _designer.Reset();
        _loadedImagePath = null;
        
        // Reset UI
        if (_shapeComboBox != null) _shapeComboBox.SelectedIndex = 0;
        if (_diameterSlider != null) _diameterSlider.Value = 100;
        if (_thicknessSlider != null) _thicknessSlider.Value = 4;
        if (_heightSlider != null) _heightSlider.Value = 6;
        if (_edgeStyleComboBox != null) _edgeStyleComboBox.SelectedIndex = 0;
        if (_textInputBox != null) _textInputBox.Text = "";
        if (_nonSlipCheckBox != null) _nonSlipCheckBox.IsChecked = false;
        if (_drainageCheckBox != null) _drainageCheckBox.IsChecked = false;
        if (_imageStatusText != null) _imageStatusText.Text = "No image loaded";
        if (_previewStatsPanel != null) _previewStatsPanel.IsVisible = false;
        if (_previewInfoText != null) _previewInfoText.Text = "Click 'Generate Preview' to see your coaster";
        
        SetStatus("New design started");
    }

    private async void OnOpenTemplateClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Template",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("CoaSTL Template") { Patterns = new[] { "*.json" } }
                }
            });

            if (files.Count > 0)
            {
                var path = files[0].Path.LocalPath;
                var template = TemplateManager.LoadTemplate(path);
                _designer.LoadFromTemplate(template);
                ApplySettingsToUI();
                SetStatus($"Loaded template: {template.Name}");
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Error loading template: {ex.Message}");
        }
    }

    private async void OnSaveTemplateClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Template",
                DefaultExtension = "json",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("CoaSTL Template") { Patterns = new[] { "*.json" } }
                },
                SuggestedFileName = "coaster_template"
            });

            if (file != null)
            {
                ApplyUIToSettings();
                var template = _designer.SaveToTemplate(Path.GetFileNameWithoutExtension(file.Path.LocalPath));
                TemplateManager.SaveTemplate(template, file.Path.LocalPath);
                SetStatus($"Saved template: {template.Name}");
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Error saving template: {ex.Message}");
        }
    }

    private async void OnLoadImageClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Load Image for Relief",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Image Files") { Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" } }
                }
            });

            if (files.Count > 0)
            {
                _loadedImagePath = files[0].Path.LocalPath;
                _designer.LoadImage(_loadedImagePath);
                _designer.ProcessImage(grayscale: true);
                _designer.GenerateHeightMap(128);
                
                if (_imageStatusText != null)
                    _imageStatusText.Text = $"Loaded: {Path.GetFileName(_loadedImagePath)}";
                
                SetStatus($"Image loaded: {Path.GetFileName(_loadedImagePath)}");
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Error loading image: {ex.Message}");
        }
    }

    private void OnGeneratePreviewClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            SetStatus("Generating preview...");
            ApplyUIToSettings();
            
            var mesh = _designer.GenerateMesh();
            var validation = _designer.ValidateMesh();
            
            // Update preview stats
            if (_previewStatsPanel != null) _previewStatsPanel.IsVisible = true;
            if (_previewInfoText != null) _previewInfoText.Text = "Coaster generated successfully!";
            
            if (_triangleCountText != null)
                _triangleCountText.Text = $"Triangles: {mesh.TriangleCount:N0}";
            
            var (min, max) = mesh.GetBoundingBox();
            var size = max - min;
            if (_dimensionsText != null)
                _dimensionsText.Text = $"Size: {size.X:F1}×{size.Y:F1}×{size.Z:F1} mm";
            
            var filament = _designer.EstimateFilamentUsage();
            if (_filamentText != null)
                _filamentText.Text = $"Filament: ~{filament:F1}g";
            
            // Get print time estimate
            var printerName = (_printerComboBox?.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (printerName != null && PrinterProfiles.TryGetValue(printerName, out var profile))
            {
                var printTime = _designer.EstimatePrintTime(profile);
                if (_printTimeText != null)
                    _printTimeText.Text = $"Print Time: ~{printTime} min";
            }
            
            if (validation.IsValid)
            {
                SetStatus($"Preview generated - {mesh.TriangleCount:N0} triangles, {filament:F1}g filament");
            }
            else
            {
                SetStatus($"Preview generated with warnings: {string.Join(", ", validation.Warnings)}");
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Error generating preview: {ex.Message}");
        }
    }

    private async void OnExportStlClick(object? sender, RoutedEventArgs e)
    {
        if (!_designer.HasMesh)
        {
            SetStatus("Please generate a preview first");
            return;
        }

        try
        {
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Export STL",
                DefaultExtension = "stl",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("STL File") { Patterns = new[] { "*.stl" } }
                },
                SuggestedFileName = "coaster"
            });

            if (file != null)
            {
                _designer.ExportToStl(file.Path.LocalPath, new StlExportOptions
                {
                    Format = StlFormat.Binary,
                    ModelName = Path.GetFileNameWithoutExtension(file.Path.LocalPath)
                });
                SetStatus($"Exported to: {file.Path.LocalPath}");
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Error exporting STL: {ex.Message}");
        }
    }

    private async void OnExport3MfClick(object? sender, RoutedEventArgs e)
    {
        if (!_designer.HasMesh)
        {
            SetStatus("Please generate a preview first");
            return;
        }

        try
        {
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Export 3MF",
                DefaultExtension = "3mf",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("3MF File") { Patterns = new[] { "*.3mf" } }
                },
                SuggestedFileName = "coaster"
            });

            if (file != null)
            {
                _designer.ExportTo3Mf(file.Path.LocalPath, new ThreeMfExportOptions
                {
                    ModelName = Path.GetFileNameWithoutExtension(file.Path.LocalPath),
                    IncludeColorInfo = true
                });
                SetStatus($"Exported to: {file.Path.LocalPath}");
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Error exporting 3MF: {ex.Message}");
        }
    }

    private void ApplyUIToSettings()
    {
        var settings = new CoasterSettings
        {
            Shape = GetSelectedShape(),
            Diameter = (float)(_diameterSlider?.Value ?? 100),
            BaseThickness = (float)(_thicknessSlider?.Value ?? 4),
            TotalHeight = (float)(_heightSlider?.Value ?? 6),
            EdgeStyle = GetSelectedEdgeStyle(),
            PolygonSides = (int)(_polygonSidesSlider?.Value ?? 6),
            ReliefDepth = (float)(_reliefDepthSlider?.Value ?? 1.5),
            InvertRelief = _invertReliefCheckBox?.IsChecked ?? false,
            AddNonSlipBottom = _nonSlipCheckBox?.IsChecked ?? false
        };
        
        _designer.Settings = settings;
        
        // Advanced settings
        var advancedSettings = new AdvancedCoasterSettings
        {
            AddDrainageGrooves = _drainageCheckBox?.IsChecked ?? false
        };
        
        _designer.AdvancedSettings = advancedSettings;
        
        // Text
        _designer.ClearText();
        var text = _textInputBox?.Text;
        if (!string.IsNullOrWhiteSpace(text))
        {
            _designer.AddText(new TextElement
            {
                Text = text,
                FontSize = (float)(_fontSizeSlider?.Value ?? 8),
                Depth = (float)(_textDepthSlider?.Value ?? 1),
                Embossed = _embossedCheckBox?.IsChecked ?? true,
                Alignment = TextAlignment.Center
            });
        }
    }

    private void ApplySettingsToUI()
    {
        var settings = _designer.Settings;
        
        if (_shapeComboBox != null)
            _shapeComboBox.SelectedIndex = (int)settings.Shape;
        
        if (_diameterSlider != null)
            _diameterSlider.Value = settings.Diameter;
        
        if (_thicknessSlider != null)
            _thicknessSlider.Value = settings.BaseThickness;
        
        if (_heightSlider != null)
            _heightSlider.Value = settings.TotalHeight;
        
        if (_edgeStyleComboBox != null)
            _edgeStyleComboBox.SelectedIndex = (int)settings.EdgeStyle;
        
        if (_polygonSidesSlider != null)
            _polygonSidesSlider.Value = settings.PolygonSides;
        
        if (_reliefDepthSlider != null)
            _reliefDepthSlider.Value = settings.ReliefDepth;
        
        if (_invertReliefCheckBox != null)
            _invertReliefCheckBox.IsChecked = settings.InvertRelief;
        
        if (_nonSlipCheckBox != null)
            _nonSlipCheckBox.IsChecked = settings.AddNonSlipBottom;
        
        if (_drainageCheckBox != null)
            _drainageCheckBox.IsChecked = _designer.AdvancedSettings.AddDrainageGrooves;
    }

    private CoasterShape GetSelectedShape()
    {
        if (_shapeComboBox?.SelectedItem is ComboBoxItem item)
        {
            return item.Content?.ToString() switch
            {
                "Circle" => CoasterShape.Circle,
                "Square" => CoasterShape.Square,
                "Hexagon" => CoasterShape.Hexagon,
                "Octagon" => CoasterShape.Octagon,
                "Rounded Square" => CoasterShape.RoundedSquare,
                "Custom Polygon" => CoasterShape.CustomPolygon,
                _ => CoasterShape.Circle
            };
        }
        return CoasterShape.Circle;
    }

    private EdgeStyle GetSelectedEdgeStyle()
    {
        if (_edgeStyleComboBox?.SelectedItem is ComboBoxItem item)
        {
            return item.Content?.ToString() switch
            {
                "Flat" => EdgeStyle.Flat,
                "Beveled" => EdgeStyle.Beveled,
                "Rounded" => EdgeStyle.Rounded,
                "Raised Rim" => EdgeStyle.RaisedRim,
                _ => EdgeStyle.Flat
            };
        }
        return EdgeStyle.Flat;
    }

    private void SetStatus(string message)
    {
        if (_statusText != null)
            _statusText.Text = message;
    }

    protected override void OnClosed(EventArgs e)
    {
        try
        {
            _designer.Dispose();
        }
        catch
        {
            // Ignore disposal exceptions during close
        }
        base.OnClosed(e);
    }
}
