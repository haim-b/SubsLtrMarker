using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using Prism.Commands;
using SubsLtrMarker;

namespace SubsLtrMarkerUI.wpf
{
    public class ConverterViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _createBackup = true;

        private string _folderPath = Environment.CurrentDirectory;

        public ConverterViewModel()
        {
            ConvertCommand = new DelegateCommand(Convert);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Console.SetOut(new LogWriter(this));
        }

        public string FolderPath
        {
            get { return _folderPath; }
            set
            {
                _folderPath = value;
                OnPropertyChanged();
            }
        }

        public bool CreateBackup
        {
            get { return _createBackup; }
            set
            {
                _createBackup = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand ConvertCommand { get; }

        private string _log;

        public string Log
        {
            get { return _log; }
            set
            {
                _log = value;
                OnPropertyChanged();
            }
        }


        private void Convert()
        {
            try
            {
                Program.FixFolder(_folderPath, _createBackup);
                MessageBox.Show("Finished");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Console.Out.Flush();
            }
        }
    }

    internal class LogWriter : TextWriter
    {
        private readonly ConverterViewModel _parent;
        public LogWriter(ConverterViewModel parent)
        {
            _parent = parent;
        }

        public override void Write(char value)
        {
            _parent.Log += value;
        }

        public override void Write(string value)
        {
            _parent.Log += value;
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
}
