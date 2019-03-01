using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using JefimsIncredibleXsltTool.Lib;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using ToastNotifications.Messages;

namespace JefimsIncredibleXsltTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly MainViewModel _mainViewModel;
        private CompletionWindow _completionWindow;
        private readonly List<int> _tmpFolderFoldingOffsets = new List<int>();

        public MainWindow()
        {
            _mainViewModel = new MainViewModel();
            _mainViewModel.TransformStarting += (a, b) =>
            {
                var cur = -1;
                _tmpFolderFoldingOffsets.Clear();
                while (true)
                {
                    var foldedFoldingStart = _outputXmlFoldingManager.GetNextFoldedFoldingStart(cur + 1);
                    if (foldedFoldingStart == -1) break;
                    cur = foldedFoldingStart;
                    var foldings = _outputXmlFoldingManager.GetFoldingsAt(foldedFoldingStart);
                    foreach (var folding in foldings)
                    {
                        if (folding.IsFolded) _tmpFolderFoldingOffsets.Add(folding.StartOffset);

                    }
                }
            };
            _mainViewModel.TransformFinished += (a, b) =>
            {
                UpdateFolding();
                if (_tmpFolderFoldingOffsets == null) return;
                foreach (var offset in _tmpFolderFoldingOffsets)
                {
                    var foldingsAtOffset = _outputXmlFoldingManager.GetFoldingsAt(offset);
                    foreach (var folding in foldingsAtOffset)
                    {
                        folding.IsFolded = true;
                    }
                }
                _tmpFolderFoldingOffsets.Clear();
            };

            DataContext = _mainViewModel;
            InitializeComponent();
            _sourceXmlFoldingManager = FoldingManager.Install(SourceXml.TextArea);
            _sourceXsltFoldingManager = FoldingManager.Install(SourceXslt.TextArea);
            _outputXmlFoldingManager = FoldingManager.Install(OutputXml.TextArea);

            SearchPanel.Install(SourceXml);
            SearchPanel.Install(OutputXml);
            SearchPanel.Install(SourceXslt);

            _strategy = new XmlFoldingStrategy();
            UpdateFolding();

            SourceXslt.TextArea.TextEntering += TextEditor_TextArea_TextEntering;
            SourceXslt.TextArea.TextEntered += TextEditor_TextArea_TextEntered;
            SourceXml.TextArea.KeyUp += (a, b) =>
            {
                try
                {
                    var xpath = GetXmlXPath(true);
                    TextBlockXPath.Text = xpath;
                }
                catch (Exception)
                {
                    TextBlockXPath.Text = "Error getting XPath";
                }
            };
        }

        private void TextEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text != "<" && e.Text != "/") return;

            _completionWindow = new CompletionWindow(SourceXslt.TextArea);
            var data = _completionWindow.CompletionList.CompletionData;
            data.Add(new XmlCompletionData("xsl:choose", "xsl:choose></xsl:choose>", "Provides multiple conditional testing in conjunction with the <xsl:otherwise> element and <xsl:when> element."));
            data.Add(new XmlCompletionData("xsl:for-each", "xsl:for-each select=\"\"></xsl:for-each>", "Applies a template repeatedly, applying it in turn to each node in a set."));
            data.Add(new XmlCompletionData("xsl:if", "xsl:if test=\"\"></xsl:if>", "Allows simple conditional template fragments."));
            data.Add(new XmlCompletionData("xsl:otherwise", "xsl:otherwise></xsl:otherwise>", "Provides multiple conditional testing in conjunction with the <xsl:choose> element and <xsl:when> element."));
            data.Add(new XmlCompletionData("xsl:text", "xsl:text></xsl:text>", "Generates text in the output."));
            data.Add(new XmlCompletionData("xsl:value-of", "xsl:value-of test=\"\"></xsl:value-of>/", "Inserts the value of the selected node as text."));
            data.Add(new XmlCompletionData("xsl:variable", "xsl:variable select=\"\"></xsl:variable>", "Specifies a value bound in an expression."));
            data.Add(new XmlCompletionData("xsl:when", "xsl:when test=\"\"></xsl:when>", "Provides multiple conditional testing in conjunction with the <xsl:choose> element and <xsl:otherwise> element."));

            _completionWindow.Show();
            _completionWindow.Closed += delegate
            {
                _completionWindow = null;
            };
        }

        private void TextEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length <= 0 || _completionWindow == null) return;
            if (!char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != ':' && e.Text[0] != '>' && e.Text[0] != '(')
            {
                // Whenever a non-letter is typed while the completion window is open,
                // insert the currently selected element.
                _completionWindow.CompletionList.RequestInsertion(e);
            }

            if (e.Text[0] == '>')
            {
                _completionWindow.Hide();
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        public void UpdateFolding()
        {
            try
            {
                _strategy.UpdateFoldings(_sourceXsltFoldingManager, SourceXslt.Document);
                _strategy.UpdateFoldings(_sourceXmlFoldingManager, SourceXml.Document);
                _strategy.UpdateFoldings(_outputXmlFoldingManager, OutputXml.Document);
            }
            catch
            {
                Console.WriteLine("Folding error");
            }
        }

        private readonly XmlFoldingStrategy _strategy;
        private FoldingManager _sourceXsltFoldingManager;
        private FoldingManager _sourceXmlFoldingManager;
        private readonly FoldingManager _outputXmlFoldingManager;

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FoldingManager.Uninstall(_sourceXsltFoldingManager);
            _mainViewModel.New();
            _sourceXsltFoldingManager = FoldingManager.Install(SourceXslt.TextArea);
            UpdateFolding();
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _mainViewModel.Save();
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var ofd = new OpenFileDialog { Filter = "Supported file types|*.xsl;*.xslt|All files|*.*" };
            if (ofd.ShowDialog() != true) return;
            try
            {
                FoldingManager.Uninstall(_sourceXsltFoldingManager);
                _mainViewModel.OpenFile(ofd.FileName);
                _sourceXsltFoldingManager = FoldingManager.Install(SourceXslt.TextArea);
                UpdateFolding();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ButtonCopyEscapedXslt_Click(object sender, RoutedEventArgs e)
        {
            var text = _mainViewModel.Document.TextDocument.Text;
            text = text.Replace("\"", "\"\"");
            text = "@\"" + text + "\"";
            try
            {
                Clipboard.SetText(text);
                _mainViewModel.Notifier.ShowSuccess("XSLT copied! :)\r\nHave a FRENDly day!");
            }
            catch
            {
                _mainViewModel.Notifier.ShowError("Could not set clipboard text and saving a tmp file failed... Boohoo :*(");
            }
        }

        private void ButtonPasteEscapedXslt_Click(object sender, RoutedEventArgs e)
        {
            var text = Clipboard.GetText();
            text = text.Replace("\"\"", "\"");
            if (text.StartsWith("@\"")) text = text.Substring(2);
            if (text.EndsWith("\"")) text = text.Substring(0, text.Length - 1);
            _mainViewModel.Document.TextDocument.Text = text;
            _mainViewModel.Notifier.ShowSuccess("XSLT pasted! :)\r\nHave a FRENDly day!");
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_mainViewModel.XmlToTransformDocument != null)
            {
                Properties.Settings.Default.xmlContent = _mainViewModel.XmlToTransformDocument.Text;
            }
            if (_mainViewModel.Document != null)
            {
                Properties.Settings.Default.xsltPath = _mainViewModel.Document.FilePath;
                Properties.Settings.Default.xsltContentIfPathEmpty = _mainViewModel.Document.TextDocument.Text;
            }

            if (_mainViewModel.ValidationSchemaFile != null)
            {
                Properties.Settings.Default.xsdPath = _mainViewModel.ValidationSchemaFile;
            }

            Properties.Settings.Default.xsltProcessingMode = (int)_mainViewModel.XsltProcessingMode;
            Properties.Settings.Default.useSyntaxConcoctions = _mainViewModel.UseSyntaxSugar;

            Properties.Settings.Default.Save();
        }

        private void Load()
        {

            try
            {
                FoldingManager.Uninstall(_sourceXsltFoldingManager);
                FoldingManager.Uninstall(_sourceXmlFoldingManager);
                if (Properties.Settings.Default.xsltPath != null &&
                    File.Exists(Properties.Settings.Default.xsltPath))
                {
                    _mainViewModel.OpenFile(Properties.Settings.Default.xsltPath);
                }
                else if (Properties.Settings.Default.xsltContentIfPathEmpty != null)
                {
                    _mainViewModel.Document.TextDocument.Text = Properties.Settings.Default.xsltContentIfPathEmpty;
                }

                if (Properties.Settings.Default.xmlContent != null)
                {
                    _mainViewModel.XmlToTransformDocument.Text = Properties.Settings.Default.xmlContent;
                }
                if (Properties.Settings.Default.xsdPath != null &&
                    File.Exists(Properties.Settings.Default.xsdPath))
                {
                    _mainViewModel.ValidationSchemaFile = Properties.Settings.Default.xsdPath;
                }

                _mainViewModel.XsltProcessingMode = (XsltProcessingMode)Properties.Settings.Default.xsltProcessingMode;

                _mainViewModel.UpdateConcoctionsUsingFlag();

                _sourceXmlFoldingManager = FoldingManager.Install(SourceXml.TextArea);
                _sourceXsltFoldingManager = FoldingManager.Install(SourceXslt.TextArea);
                UpdateFolding();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var color = Colors.Blue;
            var fontWeight = FontWeights.Bold;
            SourceXml.SyntaxHighlighting.MainRuleSet.Rules.Add(
                new HighlightingRule
                {
                    Regex = new Regex("#here_be_xslt"),
                    Color = new HighlightingColor { Foreground = new SimpleHighlightingBrush(color), FontWeight = fontWeight, FontStyle = FontStyles.Italic }
                });
            SourceXml.SyntaxHighlighting.MainRuleSet.Rules.Add(
                new HighlightingRule
                {
                    Regex = new Regex("#var"),
                    Color = new HighlightingColor { Foreground = new SimpleHighlightingBrush(color), FontWeight = fontWeight, FontStyle = FontStyles.Italic }
                });
            SourceXml.SyntaxHighlighting.MainRuleSet.Rules.Add(
                new HighlightingRule
                {
                    Regex = new Regex("#param"),
                    Color = new HighlightingColor { Foreground = new SimpleHighlightingBrush(color), FontWeight = fontWeight, FontStyle = FontStyles.Italic }
                });
            SourceXml.SyntaxHighlighting.MainRuleSet.Rules.Add(
                new HighlightingRule
                {
                    Regex = new Regex("#choose"),
                    Color = new HighlightingColor { Foreground = new SimpleHighlightingBrush(color), FontWeight = fontWeight, FontStyle = FontStyles.Italic }
                });
            SourceXml.SyntaxHighlighting.MainRuleSet.Rules.Add(
                new HighlightingRule
                {
                    Regex = new Regex("#when"),
                    Color = new HighlightingColor { Foreground = new SimpleHighlightingBrush(color), FontWeight = fontWeight, FontStyle = FontStyles.Italic }
                });
            SourceXml.SyntaxHighlighting.MainRuleSet.Rules.Add(
                new HighlightingRule
                {
                    Regex = new Regex("#else"),
                    Color = new HighlightingColor { Foreground = new SimpleHighlightingBrush(color), FontWeight = fontWeight, FontStyle = FontStyles.Italic }
                });
            SourceXml.SyntaxHighlighting.MainRuleSet.Rules.Add(
                new HighlightingRule
                {
                    Regex = new Regex("#end-else"),
                    Color = new HighlightingColor { Foreground = new SimpleHighlightingBrush(color), FontWeight = fontWeight, FontStyle = FontStyles.Italic }
                });
            SourceXml.SyntaxHighlighting.MainRuleSet.Rules.Add(
                new HighlightingRule
                {
                    Regex = new Regex("#end-choose"),
                    Color = new HighlightingColor { Foreground = new SimpleHighlightingBrush(color), FontWeight = fontWeight, FontStyle = FontStyles.Italic }
                });
            SourceXml.SyntaxHighlighting.MainRuleSet.Rules.Add(
                new HighlightingRule
                {
                    Regex = new Regex("#echo"),
                    Color = new HighlightingColor { Foreground = new SimpleHighlightingBrush(color), FontWeight = fontWeight, FontStyle = FontStyles.Italic }
                });
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                Dispatcher.BeginInvoke(new Action(Load));
            });
        }

        private string GetXmlXPath(bool includeIndexes)
        {
            var xel = XPathHelpers.GetXElementFromCursor(SourceXml.Document.Text, SourceXml.TextArea.Caret.Position.Line, SourceXml.TextArea.Caret.Position.Column);
            var xpath = XPathHelpers.GetAbsoluteXPath(xel, includeIndexes);
            return xpath;
        }

        private void MenuItemCopyXPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var xpath = GetXmlXPath(true);
                Clipboard.SetText(xpath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void MenuItemCopyXPathNoIndexes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var xpath = GetXmlXPath(false);
                Clipboard.SetText(xpath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SourceXml_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var textEditor = sender as ICSharpCode.AvalonEdit.TextEditor;
            var position = textEditor?.GetPositionFromPoint(e.GetPosition(textEditor));
            if (position.HasValue)
            {
                textEditor.TextArea.Caret.Position = position.Value;
            }
        }

        private void ButtonCopyXPathNoIndexes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var xpath = GetXmlXPath(false);
                Clipboard.SetText(xpath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ButtonCopyXPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var xpath = GetXmlXPath(true);
                Clipboard.SetText(xpath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ButtonPrettyPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SourceXml.Document.Text = _mainViewModel.XsltProcessingMode == XsltProcessingMode.Just ? PrettyJson(SourceXml.Document.Text) : PrettyXml(SourceXml.Document.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ButtonPrettyPrintXslt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SourceXslt.Document.Text = _mainViewModel.XsltProcessingMode == XsltProcessingMode.Just ? PrettyJson(SourceXslt.Document.Text) : PrettyXml(SourceXslt.Document.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        internal static string PrettyJson(string json)
        {
            return JToken.Parse(json).ToString(Newtonsoft.Json.Formatting.Indented);
        }

        private static string PrettyXml(string xml)
        {
            var doc = XDocument.Parse(xml);
            return doc.ToString();
        }

        private void SourceXml_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var xpath = GetXmlXPath(true);
                TextBlockXPath.Text = xpath;
            }
            catch (Exception)
            {
                TextBlockXPath.Text = "Error getting XPath";
            }
        }

        private void MenuItemLicenses_Click(object sender, RoutedEventArgs e)
        {
            new Licenses().ShowDialog();
        }
    }
}
