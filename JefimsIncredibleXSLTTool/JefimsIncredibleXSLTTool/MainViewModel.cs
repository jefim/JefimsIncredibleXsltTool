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
using ToastNotifications;
using Underscore;

namespace JefimsIncredibleXsltTool
{
    public class Observable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var evt = this.PropertyChanged;
            if (evt != null) evt(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum XsltProcessingMode
    {
        Saxon,
        DotNet
    }

    public class MainViewModel : Observable
    {
        private Document _document;
        private readonly TextDocument _xmlToTransformDocument = new TextDocument();
        private readonly TextDocument _resultingXmlDocument = new TextDocument();
        private readonly TextDocument _errorsDocument = new TextDocument();
        private NotificationsSource _notificationSource;
        private XsltProcessingMode _xsltProcessingMode = XsltProcessingMode.Saxon;

        private readonly ObservableCollection<XsltParameter> _xsltParameters = new ObservableCollection<XsltParameter>();
        private const string ProgramName = "Jefim's Incredible XSLT Tool";

        public MainViewModel()
        {
            this._xsltParameters = new ObservableCollection<XsltParameter>();
            this._xsltParameters.CollectionChanged += (a,b) => this.RunTransform();
            this.Document = new Document();
            this.XmlToTransformDocument.TextChanged += (a, b) => this.RunTransform();
            NotificationSource = new NotificationsSource();
        }

        public List<XsltProcessingMode> XsltProcessingModes => Enum.GetValues(typeof(XsltProcessingMode)).Cast<XsltProcessingMode>().ToList();
        
        public XsltProcessingMode XsltProcessingMode
        {
            get { return this._xsltProcessingMode; }
            set
            {
                this._xsltProcessingMode = value;
                OnPropertyChanged("XsltProcessingMode");
                this.RunTransform();
            }
        }


        public string WindowTitle
        {
            get
            {
                if(this.Document == null)
                {
                    return ProgramName;
                }

                return $"{this.Document.Display} - {ProgramName}";
            }
        }

        public Document Document
        {
            get
            {
                return this._document;
            }
            private set
            {
                if(this._document != null) this._document.TextDocument.TextChanged -= TextDocument_TextChanged;
                this._document = value;
                if (this._document != null)
                {
                    this._document.TextDocument.TextChanged += TextDocument_TextChanged;
                    this.UseSyntaxSugar = this._document.TextDocument.Text.StartsWith(XsltStylesheetSugar.Keyword);
                    this.RunTransform();
                }

                this.OnPropertyChanged("Document");
                this.OnPropertyChanged("WindowTitle");
            }
        }

        private void TextDocument_TextChanged(object sender, EventArgs e)
        {
            this.RunTransform();
            this.OnPropertyChanged("WindowTitle");
        }

        public TextDocument XmlToTransformDocument
        {
            get
            {
                return this._xmlToTransformDocument;
            }
        }
        
        public TextDocument ResultingXmlDocument
        {
            get
            {
                return this._resultingXmlDocument;
            }
        }

        private bool _useSyntaxSugar = false;
        public bool UseSyntaxSugar {
            get
            {
                return this._useSyntaxSugar;
            }
            set
            {
                if (this._useSyntaxSugar != value)
                {
                    this._useSyntaxSugar = value;
                    OnPropertyChanged("UseSyntaxSugar");
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            if (value)
                            {
                                this.Document.TextDocument.Text = this._jefimsMagicalTranspiler.PureXsltToXsltWithSugar(this.Document.TextDocument.Text);
                            }
                            else
                            {
                                this.Document.TextDocument.Text = this._jefimsMagicalTranspiler.XsltWithSugarToPureXslt(this.Document.TextDocument.Text);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                    }));

                    //this.RunTransform();
                }
            }
        }

        public TextDocument ErrorsDocument
        {
            get
            {
                return this._errorsDocument;
            }
        }

        public bool ErrorsExist
        {
            get { return this.ErrorsDocument.Text.Length > 0; }
        }
        public NotificationsSource NotificationSource
        {
            get { return _notificationSource; }
            set
            {
                _notificationSource = value;
                OnPropertyChanged("NotificationSource");
            }
        }

        public ObservableCollection<XsltParameter> XsltParameters { get { return this._xsltParameters; } }

        internal void OpenFile(string fileName)
        {
            this.Document = new Document(fileName);
            var paramNames = this.ExtractParamsFromXslt(this.Document.TextDocument.Text);
            this.XsltParameters.Clear();
            paramNames.ToList().ForEach((o) => this.XsltParameters.Add(new XsltParameter { Name = o }));
        }

        internal void New()
        {
            if(this.Document != null && this.Document.IsModified)
            {
                var answer = MessageBox.Show("You have unsaved changes in current document. Discard?", "Warning", MessageBoxButton.OKCancel);
                if (answer != MessageBoxResult.OK) return;
            }

            this.Document = new Document();
        }

        internal void Save()
        {
            if (this.Document == null)
            {
                this.NotificationSource.Show("No open file. This should not have happened :( Apologies.", NotificationType.Error);
                return;
            }

            if(this.Document.IsNew)
            {
                var ofd = new SaveFileDialog();
                ofd.Filter = "XSLT|*.xslt|All files|*.*";
                ofd.RestoreDirectory = true;
                ofd.Title = "Save new file as...";
                if(ofd.ShowDialog() == true)
                {
                    this.Document.FilePath = ofd.FileName;
                }
            }

            try
            {
                this.Document.Save();
                this.NotificationSource.Show("Saved! ☃", NotificationType.Information);
            }
            catch (Exception ex)
            {
                this.NotificationSource.Show(ex.ToString(), NotificationType.Error);
            }
        }

        public event EventHandler TransformStarting;
        public event EventHandler TransformFinished;

        protected void OnTransformStarting()
        {
            var evt = this.TransformStarting;
            if (evt != null) evt(this, EventArgs.Empty);
        }

        protected void OnTransformFinished()
        {
            var evt = this.TransformFinished;
            if (evt != null) evt(this, EventArgs.Empty);
        }

        /// <summary>
        /// Preventively checks for possible SO exception to avoid crashes.
        /// </summary>
        /// <param name="XSLT"></param>
        /// <returns></returns>
        public static string CheckForStackoverflow(string XSLT)
        {
            var xpath = "//*[local-name()='apply-templates']/@select";
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(XSLT);
            XmlNode root = doc.DocumentElement;

            var nodes = root.SelectNodes(xpath);
            var result = new List<string>();
            bool foundRefToRoot = false;
            foreach (var node in nodes)
            {
                if (((XmlAttribute)node).Value == "/")
                {
                    foundRefToRoot = true;
                    break;
                }
            }

            return foundRefToRoot ? "Prevented a StackOverflowException - you have an apply-templates element with select=\"/\" attribute in XSLT" : null;
        }

        private IEnumerable<string> ExtractParamsFromXslt(string xslt)
        {
            try
            {
                var xpath = "//*[local-name()='param']/@name";
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xslt);
                XmlNode root = doc.DocumentElement;

                var nodes = root.SelectNodes(xpath);
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
            if (this.Document != null && this.Document.TextDocument != null)
            {
                this.UseSyntaxSugar = this._document.TextDocument.Text.StartsWith(XsltStylesheetSugar.Keyword);
            }
        }

        Func<Task<bool>> _debouncedRunTransform = null;

        public void RunTransform()
        {
            if (_debouncedRunTransform == null)
            {
                _debouncedRunTransform = _.Function.Debounce(() => this.RunTransformImpl(), 200);
            }

            _debouncedRunTransform();
        }

        private JefimsMagicalTranspiler _jefimsMagicalTranspiler = new JefimsMagicalTranspiler();

        public bool RunTransformImpl()
        {
            if (this.XmlToTransformDocument == null || this.Document == null || this.Document.TextDocument == null || string.IsNullOrWhiteSpace(this.Document.TextDocument.Text))
                return false;

            var xml = this.XmlToTransformDocument.Text;
            var xslt = this.Document.TextDocument.Text;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    this.OnTransformStarting();
                    if(this.UseSyntaxSugar)
                    {
                        xslt = _jefimsMagicalTranspiler.XsltWithSugarToPureXslt(xslt);
                    }

                    string result = null;
                    switch (this.XsltProcessingMode)
                    {
                        case XsltProcessingMode.Saxon:
                            result = XsltTransformSaxon(xml, xslt, this.XsltParameters.Where(o => o != null && o.Name != null).ToArray());
                            break;
                        case XsltProcessingMode.DotNet:
                            result = XsltTransformDotNet(xml, xslt, this.XsltParameters.Where(o => o != null && o.Name != null).ToArray());
                            break;
                    }
                    
                    var validation = this.Validate(result);
                    if (validation != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.ResultingXmlDocument.Text = result;
                            this.ErrorsDocument.Text = validation;
                        }));
                    }
                    else
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.ResultingXmlDocument.Text = result;
                            this.ErrorsDocument.Text = string.Empty;
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.ErrorsDocument.Text = ex.ToString();
                    }));
                }
                finally
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                       this.OnPropertyChanged("ErrorsExist");
                        this.OnTransformFinished();
                    }));
                }
            });

            return true;
        }

        private string Validate(string xml)
        {
            if (string.IsNullOrWhiteSpace(this.ValidationSchemaFile)) return null;
            if (string.IsNullOrWhiteSpace(xml)) return null;
            XmlSchemaSet schemas = new XmlSchemaSet();
            var schema = XmlSchema.Read(new XmlTextReader(this.ValidationSchemaFile), null);
            schemas.Add(schema);

            XDocument doc = XDocument.Parse(xml);
            
            string message = null;
            doc.Validate(schemas, (o, e) =>
            {
                message = e.Message;
            });

            return message;
        }

        private string _validationSchemaFile;

        public string ValidationSchemaFile { get { return this._validationSchemaFile; } set
            {
                this._validationSchemaFile = value;
                OnPropertyChanged("ValidationSchemaFile");
            }
        }

        public static string XsltTransformDotNet(string xmlString, string XSLT, XsltParameter[] XSLTParameters)
        {
            var error = CheckForStackoverflow(XSLT);
            if (error != null) return error;

            using (var stringReader = new StringReader(XSLT))
            {
                var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString));
                using (XmlReader reader = XmlReader.Create(new StringReader(XSLT)), Inputdocument = XmlReader.Create(new StringReader(xmlString)))
                {
                    XsltSettings xslt_settings = new XsltSettings(true, true);
                    XslCompiledTransform myXslTransform = new XslCompiledTransform();
                    myXslTransform.Load(reader, xslt_settings, new XmlUrlResolver());
                    XsltArgumentList argsList = new XsltArgumentList();
                    if (XSLTParameters != null)
                        XSLTParameters.ToList().ForEach(x => argsList.AddParam(x.Name, "", x.Value));
                    MemoryStream memoryStream = new MemoryStream();
                    using (XmlWriter xmlTextWriter = XmlWriter.Create(memoryStream, myXslTransform.OutputSettings))
                    {
                        myXslTransform.Transform(Inputdocument, argsList, xmlTextWriter);
                        Encoding UTF8WithoutBom = myXslTransform.OutputSettings.Encoding; // Encoding.UTF8;
                        var output = UTF8WithoutBom.GetString(memoryStream.ToArray());
                        output = output.Replace("\n", Environment.NewLine);
                        output = output.Trim(new char[] { '\uFEFF' }); // removu utf-16 bom
                        return output;
                    }
                }
            }
        }

        public static string XsltTransformSaxon(dynamic Input, string XSLT, XsltParameter[] XSLTParameters)
        {
            string BaseUri = "";
            var processor = new Processor();
            var compiler = processor.NewXsltCompiler();
            compiler.ErrorList = new List<object>();
            var xmlString = "";

            if (Input.GetType() == typeof(string))
            {
                xmlString = (string)Input;
            }
            else if (Input.GetType() == typeof(XmlDocument))
            {
                var xmlDoc = (XmlDocument)Input;
                using (var stringWriter = new StringWriter())
                using (var xmlTextWriter = XmlWriter.Create(stringWriter))
                {
                    xmlDoc.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();
                    xmlString = stringWriter.GetStringBuilder().ToString();
                    //new ICSharpCode.AvalonEdit.TextEditor
                }
            }
            else
            {
                throw new FormatException("Unsupported input type. The supported types are XmlDocument and String.");
            }

            var error = CheckForStackoverflow(XSLT);
            if (error != null) return error;


            using (var stringReader = new StringReader(XSLT))
            {
                XsltExecutable executable = null;
                try
                {
                    executable = compiler.Compile(stringReader);
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
                    else
                    {
                        throw new Exception(ex.Message, new Exception(errorsStr));
                    }
                }
                var transformer = executable.Load();
                var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString));
                var uri = string.IsNullOrEmpty(BaseUri) ? new Uri("file://") : new Uri(BaseUri);
                transformer.SetInputStream(inputStream, uri);
                if (XSLTParameters != null)
                    XSLTParameters.ToList().ForEach(x => transformer.SetParameter(new QName(x.Name), new XdmAtomicValue(x.Value)));

                using (var stringWriter = new StringWriter())
                {
                    var serializer = new Serializer();
                    serializer.SetOutputWriter(stringWriter);
                    transformer.Run(serializer);
                    var output = stringWriter.GetStringBuilder().ToString();
                    output = output.Replace("\n", Environment.NewLine);
                    return output;
                }
            }
        }
    }

    public class Document : Observable
    {
        private string _filePath;
        private string _originalContents;
        private TextDocument _textDocument;

        public Document()
        {
            this.IsNew = true;
            var contents = string.Empty;
            this._originalContents = string.Empty;
            this.TextDocument = new TextDocument(new StringTextSource(contents));
        }

        public Document(string filePath)
        {
            this.IsNew = false;
            this.FilePath = filePath;
            var contents = File.ReadAllText(this.FilePath);
            this._originalContents = contents;
            this.TextDocument = new TextDocument(new StringTextSource(contents));
            this.TextDocument.Changed += TextDocument_Changed;
        }

        private void TextDocument_Changed(object sender, DocumentChangeEventArgs e)
        {
            this.OnPropertyChanged("Display");
            this.OnPropertyChanged("IsModified");
        }

        public bool IsNew { get; private set; }

        public bool IsModified
        {
            get
            {
                return this.TextDocument != null && this.TextDocument.Text != this._originalContents;
            }
        }

        public TextDocument TextDocument
        {
            get { return this._textDocument; }
            private set
            {
                this._textDocument = value;
                this.OnPropertyChanged("TextDocument");
            }
        }

        public string Display
        {
            get
            {
                var result = this.IsNew ? "Unsaved document" : Path.GetFileName(this.FilePath);
                if (this.IsModified) result += " *";
                return result;
            }
        }

        public string FilePath
        {
            get { return this._filePath; }
            set
            {
                this._filePath = value;
                this.IsNew = false;
                this.OnPropertyChanged("FilePath");
                this.OnPropertyChanged("Display");
            }
        }
        
        internal void Save()
        {
            if (this.FilePath == null)
            {
                MessageBox.Show("No file path for saving");
                return;
            }
            File.WriteAllText(this.FilePath, this.TextDocument.Text);
            this.IsNew = false;
            this._originalContents = this.TextDocument.Text;
            this.OnPropertyChanged("IsModified");
            this.OnPropertyChanged("Display");
        }
    }
}
