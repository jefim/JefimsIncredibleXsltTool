using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using JefimsIncredibleXsltTool;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ToastNotifications;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using JefimsIncredibleXsltTool.Lib;
using System.Text;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Linq;
using ICSharpCode.AvalonEdit;

namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<int> tmpFolderFoldingOffsets = new List<int>();
        public MainWindow()
        {
            this._mainViewModel = new MainViewModel();
            this._mainViewModel.TransformStarting += (a, b) =>
            {
                int c = 0;
                int cur = -1;
                tmpFolderFoldingOffsets.Clear();
                while (c < 1000000)
                {
                    var foldedFoldingStart = this.outputXmlFoldingManager.GetNextFoldedFoldingStart(cur+1);
                    if (foldedFoldingStart == -1) break;
                    cur = foldedFoldingStart;
                    var foldings = this.outputXmlFoldingManager.GetFoldingsAt(foldedFoldingStart);
                    foreach(var folding in foldings)
                    {
                        if (folding.IsFolded) this.tmpFolderFoldingOffsets.Add(folding.StartOffset);

                    }
                }
            };
            this._mainViewModel.TransformFinished += (a, b) =>
            {
                this.UpdateFolding();
                if (this.tmpFolderFoldingOffsets != null)
                {
                    foreach(var offset in this.tmpFolderFoldingOffsets)
                    {
                        var foldingsAtOffset = this.outputXmlFoldingManager.GetFoldingsAt(offset);
                        foreach(var folding in foldingsAtOffset)
                        {
                            folding.IsFolded = true;
                        }
                    }
                    this.tmpFolderFoldingOffsets.Clear();
                }
            };

            this.DataContext = _mainViewModel;
            InitializeComponent();
            this.sourceXmlFoldingManager = FoldingManager.Install(this.SourceXml.TextArea);
            this.sourceXsltFoldingManager = FoldingManager.Install(this.SourceXslt.TextArea);
            this.outputXmlFoldingManager = FoldingManager.Install(this.OutputXml.TextArea);

            //this.SourceXml.TextArea.DefaultInputHandler.NestedInputHandlers.Add(new SearchInputHandler(this.SourceXml.TextArea));
            SearchPanel.Install(this.SourceXml);
            SearchPanel.Install(this.OutputXml);
            SearchPanel.Install(this.SourceXslt);

            strategy = new XmlFoldingStrategy();
            UpdateFolding();
            
            this.SourceXslt.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            this.SourceXslt.TextArea.TextEntered += textEditor_TextArea_TextEntered;
            this.SourceXml.TextArea.KeyUp += (a, b) =>
            {
                try
                {
                    var xpath = GetXmlXPath(true);
                    textBlockXPath.Text = xpath;
                }
                catch (Exception)
                {
                    textBlockXPath.Text = "Error getting XPath";
                }
            };
        }

        private MainViewModel _mainViewModel;
        private CompletionWindow _completionWindow;
        /// Implements AvalonEdit ICompletionData interface to provide the entries in the
        /// completion drop down.
        public class MyCompletionData : ICompletionData
        {
            private const string _quotesStr = @"""Any idiot can face a crisis - it's day to day living that wears you out."" - Anton Chekhov
""If you are afraid of loneliness, don't marry."" - Anton Chekhov
""Medicine is my lawful wife and literature my mistress; when I get tired of one, I spend the night with the other."" - Anton Chekhov
""Don't tell me the moon is shining; show me the glint of light on broken glass."" - Anton Chekhov
""Doctors are the same as lawyers; the only difference is that lawyers merely rob you, whereas doctors rob you and kill you too."" - Anton Chekhov
""We shall find peace. We shall hear the angels, we shall see the sky sparkling with diamonds."" - Anton Chekhov
""Knowledge is of no value unless you put it into practice."" - Anton Chekhov
""Money, like vodka, turns a person into an eccentric."" - Anton Chekhov
""Love, friendship and respect do not unite people as much as a common hatred for something."" - Anton Chekhov
""You must trust and believe in people or life becomes impossible."" - Anton Chekhov
""The soul is healed by being with children."" - Fyodor Dostoyevsky
""The second half of a man's life is made up of nothing but the habits he has acquired during the first half."" - Fyodor Dostoyevsky
""Beauty is mysterious as well as terrible.God and devil are fighting there, and the battlefield is the heart of man."" - Fyodor Dostoyevsky
""Taking a new step, uttering a new word, is what people fear most."" - Fyodor Dostoyevsky
""What is hell? I maintain that it is the suffering of being unable to love."" - Fyodor Dostoyevsky
""If there is no God, everything is permitted."" - Fyodor Dostoyevsky
""The greatest happiness is to know the source of unhappiness."" - Fyodor Dostoyevsky
""To live without Hope is to Cease to live."" - Fyodor Dostoyevsky
""Much unhappiness has come into the world because of bewilderment and things left unsaid."" - Fyodor Dostoyevsky
""Man only likes to count his troubles, but he does not count his joys."" - Fyodor Dostoyevsky
""Everyone thinks of changing the world, but no one thinks of changing himself."" - Leo Tolstoy
""If you want to be happy, be."" - Leo Tolstoy
""Happy families are all alike; every unhappy family is unhappy in its own way."" - Leo Tolstoy
""All, everything that I understand, I understand only because I love."" - Leo Tolstoy
""The two most powerful warriors are patience and time."" - Leo Tolstoy
""There is no greatness where there is no simplicity, goodness and truth."" - Leo Tolstoy
""Truth, like gold, is to be obtained not by its growth, but by washing away from it all that is not gold."" - Leo Tolstoy
""In the name of God, stop a moment, cease your work, look around you."" - Leo Tolstoy
""We can know only that we know nothing. And that is the highest degree of human wisdom."" - Leo Tolstoy
""The sole meaning of life is to serve humanity."" - Leo Tolstoy
""Be not afraid of greatness: some are born great, some achieve greatness, and some have greatness thrust upon them."" - William Shakespeare
""To thine own self be true, and it must follow, as the night the day, thou canst not then be false to any man."" - William Shakespeare
""The course of true love never did run smooth."" - William Shakespeare
""There is nothing either good or bad, but thinking makes it so."" - William Shakespeare
""Love looks not with the eyes, but with the mind; and therefore is winged Cupid painted blind."" - William Shakespeare
""Cowards die many times before their deaths; the valiant never taste of death but once."" - William Shakespeare
""If music be the food of love, play on."" - William Shakespeare
""Love all, trust a few, do wrong to none."" - William Shakespeare
""Life... is a tale Told by an idiot, full of sound and fury, Signifying nothing."" - William Shakespeare
""Men at some time are the masters of their fates: The fault, dear Brutus, is not in our stars, but in ourselves, that we are underlings."" - William Shakespeare
""Imagination is more important than knowledge."" - Albert Einstein
""The important thing is not to stop questioning. Curiosity has its own reason for existing."" - Albert Einstein
""Anyone who has never made a mistake has never tried anything new."" - Albert Einstein
""Try not to become a man of success, but rather try to become a man of value."" - Albert Einstein
""Two things are infinite: the universe and human stupidity; and I'm not sure about the universe."" - Albert Einstein
""Science without religion is lame, religion without science is blind."" - Albert Einstein
""The significant problems we have cannot be solved at the same level of thinking with which we created them."" - Albert Einstein
""Everything should be made as simple as possible, but not simpler."" - Albert Einstein
""The most beautiful thing we can experience is the mysterious.It is the source of all true art and science."" - Albert Einstein
""I have no special talents.I am only passionately curious."" - Albert Einstein
""Pay no attention to what the critics say; no statue has ever been erected to a critic."" - Jean Sibelius
""A strong desire derives a person straight through the hardest rock."" - Aleksis Kivi
""The love goes where it wants; you hear it rustling but you do not know where it's going and where it goes."" - Aleksis Kivi
""I only want to live in peace, plant potatoes and dream!"" - Tove Jansson
""You can't ever be really free if you admire somebody too much."" - Tove Jansson
""Maybe my passion is nothing special, but at least it's mine."" - Tove Jansson";

            private static Random _random = new Random();
            private string _description = null;

            static MyCompletionData()
            {
                _quotes = _quotesStr.Split('\n').Select(o => o.Trim()).ToList();
            }

            public MyCompletionData(string text)
            {
                this.Text = text;
            }

            public MyCompletionData(string text, string description)
            {
                this.Text = text;
                this._description = description;
            }

            public System.Windows.Media.ImageSource Image
            {
                get { return null; }
            }

            public string Text { get; private set; }

            // Use this property if you want to show a fancy UIElement in the list.
            public object Content
            {
                get { return this.Text; }
            }

            private static List<string> _quotes;

            public object Description
            {
                get
                {
                    var showQuote = false; // _random.Next() % 5 == 0;
                    return this._description + (showQuote ? Environment.NewLine + _quotes[_random.Next(0, _quotes.Count - 1)] : "");
                }
            }

            public double Priority
            {
                get
                {
                    return 1;
                }
            }

            public void Complete(TextArea textArea, ISegment completionSegment,
                EventArgs insertionRequestEventArgs)
            {
                textArea.Document.Replace(completionSegment, this.Text);
            }
        }

        private void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "<" || e.Text == "/")
            {
                // Open code completion after the user has pressed dot:
                _completionWindow = new CompletionWindow(this.SourceXslt.TextArea);
                IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;
                data.Add(new MyCompletionData("xsl:apply-imports", "Invokes an overridden template rule."));
                data.Add(new MyCompletionData("xsl:apply-templates", "Directs the XSLT processor to find the appropriate template to apply, based on the type and context of each selected node."));
                data.Add(new MyCompletionData("xsl:attribute", "Creates an attribute node and attaches it to an output element."));
                data.Add(new MyCompletionData("xsl:attribute-set", "Defines a named set of attributes."));
                data.Add(new MyCompletionData("xsl:call-template", "Invokes a template by name."));
                data.Add(new MyCompletionData("xsl:choose", "Provides multiple conditional testing in conjunction with the <xsl:otherwise> element and <xsl:when> element."));
                data.Add(new MyCompletionData("xsl:comment", "Generates a comment in the output."));
                data.Add(new MyCompletionData("xsl:copy", "Copies the current node from the source to the output."));
                data.Add(new MyCompletionData("xsl:copy-of", "Inserts subtrees and result tree fragments into the result tree."));
                data.Add(new MyCompletionData("xsl:decimal-format", "Declares a decimal-format, which controls the interpretation of a format pattern used by the format-number function."));
                data.Add(new MyCompletionData("xsl:element", "Creates an element with the specified name in the output."));
                data.Add(new MyCompletionData("xsl:fallback", "Calls template content that can provide a reasonable substitute to the behavior of the new element when encountered."));
                data.Add(new MyCompletionData("xsl:for-each", "Applies a template repeatedly, applying it in turn to each node in a set."));
                data.Add(new MyCompletionData("xsl:if", "Allows simple conditional template fragments."));
                data.Add(new MyCompletionData("xsl:import", "Imports another XSLT file."));
                data.Add(new MyCompletionData("xsl:include", "Includes another XSLT file."));
                data.Add(new MyCompletionData("xsl:key", "Declares a named key for use with thekey() function in XML Path Language (XPath) expressions."));
                data.Add(new MyCompletionData("xsl:message", "Sends a text message to either the message buffer or a message dialog box."));
                data.Add(new MyCompletionData("xsl:namespace-alias", "Replaces the prefix associated with a given namespace with another prefix."));
                data.Add(new MyCompletionData("xsl:number", "Inserts a formatted number into the result tree."));
                data.Add(new MyCompletionData("xsl:otherwise", "Provides multiple conditional testing in conjunction with the <xsl:choose> element and <xsl:when> element."));
                data.Add(new MyCompletionData("xsl:output", "Specifies options for use in serializing the result tree."));
                data.Add(new MyCompletionData("xsl:param", "Declares a named parameter for use within an <xsl:stylesheet> element or an <xsl:template> element. Allows you to specify a default value."));
                data.Add(new MyCompletionData("xsl:preserve-space", "Preserves white space in a document."));
                data.Add(new MyCompletionData("xsl:processing-instruction", "Generates a processing instruction in the output."));
                data.Add(new MyCompletionData("msxsl:script", ".NET ONLY! Defines global variables and functions for script extensions."));
                data.Add(new MyCompletionData("xsl:sort", "Specifies sort criteria for node lists selected by <xsl:for-each> or <xsl:apply-templates>."));
                data.Add(new MyCompletionData("xsl:strip-space", "Strips white space from a document."));
                data.Add(new MyCompletionData("xsl:stylesheet", "Specifies the document element of an XSLT file. The document element contains all other XSLT elements."));
                data.Add(new MyCompletionData("xsl:template", "Defines a reusable template for generating the desired output for nodes of a particular type and context."));
                data.Add(new MyCompletionData("xsl:text", "Generates text in the output."));
                data.Add(new MyCompletionData("xsl:transform", "Performs the same function as <xsl:stylesheet>."));
                data.Add(new MyCompletionData("xsl:value-of", "Inserts the value of the selected node as text."));
                data.Add(new MyCompletionData("xsl:variable", "Specifies a value bound in an expression."));
                data.Add(new MyCompletionData("xsl:when", "Provides multiple conditional testing in conjunction with the <xsl:choose> element and <xsl:otherwise> element."));
                data.Add(new MyCompletionData("xsl:with-param", "Passes a parameter to a template."));

                _completionWindow.Show();
                _completionWindow.Closed += delegate
                {
                    _completionWindow = null;
                };
            }
        }
        
        private void textEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
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
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        public void UpdateFolding()
        {
            try
            {
                strategy.UpdateFoldings(sourceXsltFoldingManager, this.SourceXslt.Document);
                strategy.UpdateFoldings(sourceXmlFoldingManager, this.SourceXml.Document);
                strategy.UpdateFoldings(outputXmlFoldingManager, this.OutputXml.Document);
            }
            catch
            {
                Console.WriteLine("Folding error");
            }
        }

        XmlFoldingStrategy strategy;
        FoldingManager sourceXsltFoldingManager;
        FoldingManager sourceXmlFoldingManager;
        FoldingManager outputXmlFoldingManager;
        
        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FoldingManager.Uninstall(this.sourceXsltFoldingManager);
            this._mainViewModel.New();
            this.sourceXsltFoldingManager = FoldingManager.Install(this.SourceXslt.TextArea);
            this.UpdateFolding();
        }
        
        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this._mainViewModel.Save();
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Supported file types|*.xsl;*.xslt|All files|*.*";
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    FoldingManager.Uninstall(this.sourceXsltFoldingManager);
                    this._mainViewModel.OpenFile(ofd.FileName);
                    this.sourceXsltFoldingManager = FoldingManager.Install(this.SourceXslt.TextArea);
                    this.UpdateFolding();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void ButtonCopyEscapedXslt_Click(object sender, RoutedEventArgs e)
        {
            var text = this._mainViewModel.Document.TextDocument.Text;
            text = text.Replace("\"", "\"\"");
            text = "@\"" + text + "\"";
            try
            {
                Clipboard.SetText(text);
                this._mainViewModel.NotificationSource.Show("XSLT copied! :)\r\nHave a FRENDly day!", NotificationType.Success);
            }catch
            {
                try
                {
                    //var tmpFile = Path.GetTempFileName();
                    //File.WriteAllText(tmpFile, text);
                    //Process.Start("notepad.exe", tmpFile);
                    this._mainViewModel.NotificationSource.Show("Clipboard threw an exception, but it could've still worked - give it a try! :)\r\nHave a FRENDly day!", NotificationType.Success);
                }
                catch
                {
                    this._mainViewModel.NotificationSource.Show("Could not set clipboard text and saving a tmp file failed... Boohoo :*(", NotificationType.Error);
                }
            }
        }
    
        private void ButtonPasteEscapedXslt_Click(object sender, RoutedEventArgs e)
        {
            var text = Clipboard.GetText();
            text = text.Replace("\"\"", "\"");
            if (text.StartsWith("@\"")) text = text.Substring(2);
            if (text.EndsWith("\"")) text = text.Substring(0, text.Length - 1);
            this._mainViewModel.Document.TextDocument.Text = text;
            this._mainViewModel.NotificationSource.Show("XSLT pasted! :)\r\nHave a FRENDly day!", NotificationType.Success);
        }
        

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this._mainViewModel.XmlToTransformDocument != null)
            {
                JefimsIncredibleXsltTool.Properties.Settings.Default.xmlContent = this._mainViewModel.XmlToTransformDocument.Text;
            }
            if (this._mainViewModel.Document != null)
            {
                JefimsIncredibleXsltTool.Properties.Settings.Default.xsltPath = this._mainViewModel.Document.FilePath;
                JefimsIncredibleXsltTool.Properties.Settings.Default.xsltContentIfPathEmpty = this._mainViewModel.Document.TextDocument.Text;
            }

            if (this._mainViewModel.ValidationSchemaFile != null)
            {
                JefimsIncredibleXsltTool.Properties.Settings.Default.xsdPath = this._mainViewModel.ValidationSchemaFile;
            }


            JefimsIncredibleXsltTool.Properties.Settings.Default.xsltProcessingMode = (int)this._mainViewModel.XsltProcessingMode;
            JefimsIncredibleXsltTool.Properties.Settings.Default.useSyntaxConcoctions = this._mainViewModel.UseSyntaxSugar;

            JefimsIncredibleXsltTool.Properties.Settings.Default.Save();
        }

        private void Load()
        {

            try
            {
                FoldingManager.Uninstall(this.sourceXsltFoldingManager);
                FoldingManager.Uninstall(this.sourceXmlFoldingManager);
                if (JefimsIncredibleXsltTool.Properties.Settings.Default.xsltPath != null &&
                    File.Exists(JefimsIncredibleXsltTool.Properties.Settings.Default.xsltPath))
                {
                    this._mainViewModel.OpenFile(JefimsIncredibleXsltTool.Properties.Settings.Default.xsltPath);
                }
                else if (JefimsIncredibleXsltTool.Properties.Settings.Default.xsltContentIfPathEmpty != null)
                {
                    this._mainViewModel.Document.TextDocument.Text = JefimsIncredibleXsltTool.Properties.Settings.Default.xsltContentIfPathEmpty;
                }

                if (JefimsIncredibleXsltTool.Properties.Settings.Default.xmlContent != null)
                {
                    this._mainViewModel.XmlToTransformDocument.Text = JefimsIncredibleXsltTool.Properties.Settings.Default.xmlContent;
                }
                if (JefimsIncredibleXsltTool.Properties.Settings.Default.xsdPath != null &&
                    File.Exists(JefimsIncredibleXsltTool.Properties.Settings.Default.xsdPath))
                {
                    this._mainViewModel.ValidationSchemaFile = JefimsIncredibleXsltTool.Properties.Settings.Default.xsdPath;
                }

                this._mainViewModel.XsltProcessingMode = (XsltProcessingMode)JefimsIncredibleXsltTool.Properties.Settings.Default.xsltProcessingMode;
                
                this._mainViewModel.UpdateConcoctionsUsingFlag();

                this.sourceXmlFoldingManager = FoldingManager.Install(this.SourceXml.TextArea);
                this.sourceXsltFoldingManager = FoldingManager.Install(this.SourceXslt.TextArea);
                this.UpdateFolding();
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
            this.SourceXml.SyntaxHighlighting.MainRuleSet.Rules.Add(
                new HighlightingRule()
                {
                    Regex = new Regex("#here_be_xslt"),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(color), FontWeight = fontWeight, FontStyle = FontStyles.Italic }
                });
            this.SourceXml.SyntaxHighlighting.MainRuleSet.Rules.Add(
                new HighlightingRule()
                {
                    Regex = new Regex("#var"),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(color), FontWeight = fontWeight, FontStyle = FontStyles.Italic }
                });
            this.SourceXml.SyntaxHighlighting.MainRuleSet.Rules.Add(
                new HighlightingRule()
                {
                    Regex = new Regex("#param"),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(color), FontWeight = fontWeight, FontStyle = FontStyles.Italic }
                });
            this.SourceXml.SyntaxHighlighting.MainRuleSet.Rules.Add(
                new HighlightingRule()
                {
                    Regex = new Regex("#choose"),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(color), FontWeight = fontWeight, FontStyle = FontStyles.Italic }
                });
            this.SourceXml.SyntaxHighlighting.MainRuleSet.Rules.Add(
                new HighlightingRule()
                {
                    Regex = new Regex("#when"),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(color), FontWeight = fontWeight, FontStyle = FontStyles.Italic }
                });
            this.SourceXml.SyntaxHighlighting.MainRuleSet.Rules.Add(
                new HighlightingRule()
                {
                    Regex = new Regex("#else"),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(color), FontWeight = fontWeight, FontStyle = FontStyles.Italic }
                });
            this.SourceXml.SyntaxHighlighting.MainRuleSet.Rules.Add(
                new HighlightingRule()
                {
                    Regex = new Regex("#end-else"),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(color), FontWeight = fontWeight, FontStyle = FontStyles.Italic }
                });
            this.SourceXml.SyntaxHighlighting.MainRuleSet.Rules.Add(
                new HighlightingRule()
                {
                    Regex = new Regex("#end-choose"),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(color), FontWeight = fontWeight, FontStyle = FontStyles.Italic }
                });
            this.SourceXml.SyntaxHighlighting.MainRuleSet.Rules.Add(
                new HighlightingRule()
                {
                    Regex = new Regex("#echo"),
                    Color = new HighlightingColor() { Foreground = new SimpleHighlightingBrush(color), FontWeight = fontWeight, FontStyle = FontStyles.Italic }
                });
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                this.Dispatcher.BeginInvoke(new Action(() => this.Load()));
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
            catch(Exception ex)
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
            var position = textEditor.GetPositionFromPoint(e.GetPosition(textEditor));
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
                SourceXml.Document.Text = PrettyXml(SourceXml.Document.Text);
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
                SourceXslt.Document.Text = PrettyXml(SourceXslt.Document.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        
        static string PrettyXml(string xml)
        {
            XDocument doc = XDocument.Parse(xml);
            return doc.ToString();
        }

        private void SourceXml_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var xpath = GetXmlXPath(true);
                textBlockXPath.Text = xpath;
            }
            catch (Exception)
            {
                textBlockXPath.Text = "Error getting XPath";
            }
        }

        private void MenuItemLicenses_Click(object sender, RoutedEventArgs e)
        {
            new Licenses().ShowDialog();
        }
    }
}
