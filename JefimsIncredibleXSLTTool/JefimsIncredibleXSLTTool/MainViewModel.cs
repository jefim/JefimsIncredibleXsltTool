using ICSharpCode.AvalonEdit.Document;
using JefimsIncredibleXsltTool.Lib;
using JefimsMagicalXsltSyntaxConcoctions;
using JefimsMagicalXsltSyntaxConcoctions.SyntaxSugars;
using Microsoft.Win32;
using Saxon.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Xsl;
using JUST;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;
using Underscore;

namespace JefimsIncredibleXsltTool
{
    public class Observable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var evt = PropertyChanged;
            evt?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum XsltProcessingMode
    {
        Saxon,
        DotNet,
        Just
    }

    public class MainViewModel : Observable
    {
        public Notifier Notifier = new Notifier(cfg =>
        {
            cfg.PositionProvider = new WindowPositionProvider(
                parentWindow: Application.Current.MainWindow,
                corner: Corner.TopRight,
                offsetX: 10,
                offsetY: 10);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(3),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.Dispatcher = Application.Current.Dispatcher;
        });

        private Document _document;
        private XsltProcessingMode _xsltProcessingMode = XsltProcessingMode.Saxon;
        private const string ProgramName = "Jefim's Incredible XSLT Tool";
        public event EventHandler TransformStarting;
        public event EventHandler TransformFinished;
        private readonly JefimsMagicalTranspiler _jefimsMagicalTranspiler = new JefimsMagicalTranspiler();

        public MainViewModel()
        {
            XsltParameters = new ObservableCollection<XsltParameter>();
            XsltParameters.CollectionChanged += (a, b) => RunTransform();
            Document = new Document();
            XmlToTransformDocument.TextChanged += (a, b) => RunTransform();
        }

        public List<XsltProcessingMode> XsltProcessingModes => Enum.GetValues(typeof(XsltProcessingMode)).Cast<XsltProcessingMode>().ToList();

        public XsltProcessingMode XsltProcessingMode
        {
            get => _xsltProcessingMode;
            set
            {
                _xsltProcessingMode = value;
                OnPropertyChanged("XsltProcessingMode");
                RunTransform();
            }
        }

        public string WindowTitle => Document == null ? ProgramName : $"{Document.Display} - {ProgramName}";

        public Document Document
        {
            get => _document;
            private set
            {
                if (_document != null) _document.TextDocument.TextChanged -= TextDocument_TextChanged;
                _document = value;
                if (_document != null)
                {
                    _document.TextDocument.TextChanged += TextDocument_TextChanged;
                    UseSyntaxSugar = _document.TextDocument.Text.StartsWith(XsltStylesheetSugar.Keyword);
                    RunTransform();
                }

                OnPropertyChanged("Document");
                OnPropertyChanged("WindowTitle");
            }
        }

        private void TextDocument_TextChanged(object sender, EventArgs e)
        {
            RunTransform();
            OnPropertyChanged("WindowTitle");
        }

        public TextDocument XmlToTransformDocument { get; } = new TextDocument();

        public TextDocument ResultingXmlDocument { get; } = new TextDocument();

        private bool _useSyntaxSugar;
        public bool UseSyntaxSugar
        {
            get => _useSyntaxSugar;
            set
            {
                if (_useSyntaxSugar == value) return;
                _useSyntaxSugar = value;
                OnPropertyChanged("UseSyntaxSugar");
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        Document.TextDocument.Text = value ? _jefimsMagicalTranspiler.PureXsltToXsltWithSugar(Document.TextDocument.Text) : _jefimsMagicalTranspiler.XsltWithSugarToPureXslt(Document.TextDocument.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }));
            }
        }

        public TextDocument ErrorsDocument { get; } = new TextDocument();

        public bool ErrorsExist => ErrorsDocument.Text.Length > 0;

        public ObservableCollection<XsltParameter> XsltParameters { get; }

        internal void OpenFile(string fileName)
        {
            Document = new Document(fileName);
            var paramNames = ExtractParamsFromXslt(Document.TextDocument.Text);
            XsltParameters.Clear();
            paramNames.ToList().ForEach((o) => XsltParameters.Add(new XsltParameter { Name = o }));
        }

        internal void New()
        {
            if (Document != null && Document.IsModified)
            {
                var answer = MessageBox.Show("You have unsaved changes in current document. Discard?", "Warning", MessageBoxButton.OKCancel);
                if (answer != MessageBoxResult.OK) return;
            }

            Document = new Document();
        }

        internal void Save()
        {
            if (Document == null)
            {
                Notifier.ShowWarning("No open file. This should not have happened :( Apologies.");
                return;
            }

            if (Document.IsNew)
            {
                var ofd = new SaveFileDialog
                {
                    Filter = "XSLT|*.xslt|All files|*.*",
                    RestoreDirectory = true,
                    Title = "Save new file as..."
                };
                if (ofd.ShowDialog() == true)
                {
                    Document.FilePath = ofd.FileName;
                }
            }

            try
            {
                Document.Save();
                Notifier.ShowSuccess("Saved! ☃");
            }
            catch (Exception ex)
            {
                Notifier.ShowError(ex.ToString());
            }
        }

        protected void OnTransformStarting()
        {
            var evt = TransformStarting;
            evt?.Invoke(this, EventArgs.Empty);
        }

        protected void OnTransformFinished()
        {
            var evt = TransformFinished;
            evt?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Preventively checks for possible SO exception to avoid crashes.
        /// </summary>
        /// <param name="xslt"></param>
        /// <returns></returns>
        public static string CheckForStackoverflow(string xslt)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xslt);
            XmlNode root = doc.DocumentElement;

            var nodes = root?.SelectNodes("//*[local-name()='apply-templates']/@select");
            var foundRefToRoot = false;
            foreach (var node in nodes)
            {
                if (((XmlAttribute)node).Value != "/") continue;
                foundRefToRoot = true;
                break;
            }

            return foundRefToRoot ? "Prevented a StackOverflowException - you have an apply-templates element with select=\"/\" attribute in XSLT" : null;
        }

        private static IEnumerable<string> ExtractParamsFromXslt(string xslt)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xslt);
                XmlNode root = doc.DocumentElement;

                var nodes = root?.SelectNodes("//*[local-name()='param']/@name");
                var result = new List<string>();
                foreach (var node in nodes)
                {
                    result.Add(((XmlAttribute)node).Value);
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new string[0];
            }
        }

        internal void UpdateConcoctionsUsingFlag()
        {
            if (Document?.TextDocument != null)
            {
                UseSyntaxSugar = _document.TextDocument.Text.StartsWith(XsltStylesheetSugar.Keyword);
            }
        }

        private Func<Task<bool>> _debouncedRunTransform;

        public void RunTransform()
        {
            if (_debouncedRunTransform == null)
            {
                _debouncedRunTransform = _.Function.Debounce(RunTransformImpl, 200);
            }

            _debouncedRunTransform();
        }

        public bool RunTransformImpl()
        {
            if (XmlToTransformDocument == null || string.IsNullOrWhiteSpace(Document?.TextDocument?.Text))
                return false;

            var xml = XmlToTransformDocument.Text;
            var xslt = Document.TextDocument.Text;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    OnTransformStarting();
                    if (UseSyntaxSugar)
                    {
                        xslt = _jefimsMagicalTranspiler.XsltWithSugarToPureXslt(xslt);
                    }

                    string result = null;
                    switch (XsltProcessingMode)
                    {
                        case XsltProcessingMode.Saxon:
                            result = XsltTransformSaxon(xml, xslt, XsltParameters.Where(o => o?.Name != null).ToArray());
                            break;
                        case XsltProcessingMode.DotNet:
                            result = XsltTransformDotNet(xml, xslt, XsltParameters.Where(o => o?.Name != null).ToArray());
                            break;
                        case XsltProcessingMode.Just:
                            result = JsonTransformUsingJustNet(xml, xslt);
                            break;
                        default:
                            MessageBox.Show("Unknown transform method: " + XsltProcessingMode);
                            break;
                    }

                    var validation = Validate(result);
                    if (validation != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ResultingXmlDocument.Text = result;
                            ErrorsDocument.Text = validation;
                        }));
                    }
                    else
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ResultingXmlDocument.Text = result;
                            ErrorsDocument.Text = string.Empty;
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ErrorsDocument.Text = ex.Message;
                    }));
                }
                finally
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        OnPropertyChanged("ErrorsExist");
                        OnTransformFinished();
                    }));
                }
            });

            return true;
        }

        private string Validate(string xml)
        {
            if (string.IsNullOrWhiteSpace(ValidationSchemaFile)) return null;
            if (string.IsNullOrWhiteSpace(xml)) return null;
            var schemas = new XmlSchemaSet();
            var schema = XmlSchema.Read(new XmlTextReader(ValidationSchemaFile), null);
            schemas.Add(schema);

            var doc = XDocument.Parse(xml);

            string message = null;
            doc.Validate(schemas, (o, e) =>
            {
                message = e.Message;
            });

            return message;
        }

        private string _validationSchemaFile;

        public string ValidationSchemaFile
        {
            get => _validationSchemaFile;
            set
            {
                _validationSchemaFile = value;
                OnPropertyChanged("ValidationSchemaFile");
            }
        }

        public static string XsltTransformDotNet(string xmlString, string xslt, XsltParameter[] xsltParameters)
        {
            var error = CheckForStackoverflow(xslt);
            if (error != null) return error;

            using (var xmlDocumenOut = new StringWriter())
            using (StringReader xmlReader = new StringReader(xmlString), xsltReader = new StringReader(xslt))
            using (XmlReader xmlDocument = XmlReader.Create(xmlReader), xsltDocument = XmlReader.Create(xsltReader))
            {
                var xsltSettings = new XsltSettings(true, true);
                var myXslTransform = new XslCompiledTransform();
                myXslTransform.Load(xsltDocument, xsltSettings, new XmlUrlResolver());
                var argsList = new XsltArgumentList();
                xsltParameters?.ToList().ForEach(x => argsList.AddParam(x.Name, "", x.Value));
                using (var xmlTextWriter = XmlWriter.Create(xmlDocumenOut, myXslTransform.OutputSettings))
                {
                    myXslTransform.Transform(xmlDocument, argsList, xmlTextWriter);
                    return xmlDocumenOut.ToString().Replace("\n", Environment.NewLine).Trim('\uFEFF');
                }
            }
        }

        public static string XsltTransformSaxon(string xmlString, string xslt, XsltParameter[] xsltParameters)
        {
            var error = CheckForStackoverflow(xslt);
            if (error != null) return error;

            var processor = new Processor();
            var compiler = processor.NewXsltCompiler();
            compiler.ErrorList = new List<object>();

            using (var xmlDocumenOut = new StringWriter())
            using (var xsltReader = new StringReader(xslt))
            using (var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString)))
            {
                XsltExecutable executable;
                try
                {
                    executable = compiler.Compile(xsltReader);
                }
                catch (Exception ex)
                {
                    var staticErrors = string.Join(Environment.NewLine, ((List<object>)compiler.ErrorList).OfType<StaticError>().Select(o => $"{o.Message} at line {o.LineNumber}, column {o.ColumnNumber}").Distinct());
                    var dynamicErrors = string.Join(Environment.NewLine, ((List<object>)compiler.ErrorList).OfType<DynamicError>().Select(o => $"{o.Message} at line {o.LineNumber}").Distinct());
                    var errorsStr = staticErrors + Environment.NewLine + dynamicErrors;
                    if (string.IsNullOrWhiteSpace(errorsStr))
                    {
                        throw;
                    }
                    throw new Exception(ex.Message, new Exception(errorsStr));
                }

                var transformer = executable.Load();
                transformer.SetInputStream(xmlStream, new Uri("file://"));
                xsltParameters?.ToList().ForEach(x => transformer.SetParameter(new QName(x.Name), new XdmAtomicValue(x.Value)));

                var serializer = new Serializer();
                serializer.SetOutputWriter(xmlDocumenOut);
                transformer.Run(serializer);
                return xmlDocumenOut.ToString().Replace("\n", Environment.NewLine);
            }
        }

        private static string JsonTransformUsingJustNet(string json, string transformer)
        {
            return MainWindow.PrettyJson(JsonTransformer.Transform(transformer, json));
        }
    }

    public class Document : Observable
    {
        private string _filePath;
        private string _originalContents;
        private TextDocument _textDocument;

        public Document()
        {
            IsNew = true;
            var contents = string.Empty;
            _originalContents = string.Empty;
            TextDocument = new TextDocument(new StringTextSource(contents));
        }

        public Document(string filePath)
        {
            IsNew = false;
            FilePath = filePath;
            var contents = File.ReadAllText(FilePath);
            _originalContents = contents;
            TextDocument = new TextDocument(new StringTextSource(contents));
            TextDocument.Changed += TextDocument_Changed;
        }

        private void TextDocument_Changed(object sender, DocumentChangeEventArgs e)
        {
            OnPropertyChanged("Display");
            OnPropertyChanged("IsModified");
        }

        public bool IsNew { get; private set; }

        public bool IsModified => TextDocument != null && TextDocument.Text != _originalContents;

        public TextDocument TextDocument
        {
            get => _textDocument;
            private set
            {
                _textDocument = value;
                OnPropertyChanged("TextDocument");
            }
        }

        public string Display
        {
            get
            {
                var result = IsNew ? "Unsaved document" : Path.GetFileName(FilePath);
                if (IsModified) result += " *";
                return result;
            }
        }

        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                IsNew = false;
                OnPropertyChanged("FilePath");
                OnPropertyChanged("Display");
            }
        }

        internal void Save()
        {
            if (FilePath == null)
            {
                MessageBox.Show("No file path for saving");
                return;
            }
            File.WriteAllText(FilePath, TextDocument.Text);
            IsNew = false;
            _originalContents = TextDocument.Text;
            OnPropertyChanged("IsModified");
            OnPropertyChanged("Display");
        }
    }
}
