using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using MorningPod.Models;
using MorningPod.Ultilities;
using scb = System.Windows.Media.SolidColorBrush;
using col = System.Windows.Media.Color;

namespace MorningPod.Ultilities
{
    public class GenericEventArgs : EventArgs { public object Object; public GenericEventArgs(object obj = null) { Object = obj; } }


    public class mt
    {
        public static void SerializeObject<T>(T serializableObject , string fileName)
        {
            if (serializableObject == null)
            {
                return;
            }
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream , serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                    stream.Close();
                }
            }
            catch (Exception ex) { mt.Log(ex); }
        }

        public static T DeSerializeObject<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return default(T);
            }

            T objectOut = default(T);

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;

                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof (T);

                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T) serializer.Deserialize(reader);
                        reader.Close();
                    }

                    read.Close();
                }
            }
            catch (Exception ex) { mt.Log(ex); }

            return objectOut;
        }
        

        public static BackgroundWorker StartAWorker(Action WorkAction, Action DoneAction)
        {
            BackgroundWorker worker = new BackgroundWorker {WorkerReportsProgress = true};
            worker.DoWork += (sender, args) => WorkAction();

            worker.RunWorkerCompleted += delegate
            {
                if (DoneAction != null) Application.Current.Dispatcher.Invoke(DoneAction);
            };
            worker.RunWorkerAsync();
            return worker;
        }

        public static void DoDelay(Action action, int sleep = 100)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender , args) =>
            {
                Thread.Sleep(sleep);
            };

            worker.RunWorkerCompleted += delegate
            {
                if (action != null) Application.Current.Dispatcher.Invoke(action);
            };
            worker.RunWorkerAsync();
        }
        

        public static void HandleAutoColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(DateTime))
            {
                DataGridTextColumn dataGridTextColumn = e.Column as DataGridTextColumn;
                if (dataGridTextColumn != null)
                {
                    dataGridTextColumn.Binding.StringFormat = "yy-MM-dd : HH:mm";
                }
            }
            if (e.PropertyType == typeof(TimeSpan))
            {
                DataGridTextColumn dataGridTextColumn = e.Column as DataGridTextColumn;
                if (dataGridTextColumn != null)
                {
                    dataGridTextColumn.Binding.StringFormat = "mm";
                }
            }
            if (e.PropertyType == typeof (uint))
            {
                FrameworkElementFactory prog = new FrameworkElementFactory(typeof (ProgressBar));
                prog.SetBinding(Control.BackgroundProperty , new Binding("IsDLed") { Converter = BoolToggleColorBrs.Get, ConverterParameter = new scb[] { vals.MediumSlateBlue, vals.Trans }});
                prog.SetBinding(RangeBase.ValueProperty, new Binding("DLPerc"));
                prog.SetBinding(UIElement.VisibilityProperty , new Binding("IsDLedOrDLing") { Converter = vals.BoolToVis });

                e.Column = new DataGridTemplateColumn { Header = "Download" , CellTemplate = new DataTemplate { VisualTree = prog } };
            }
        }


        public static bool IsNotDuplicated(string url , ObservableCollection<Epi> epis)
        {
            IEnumerable<Epi> found = epis.Where(p => p.Url == url);
            return !found.Any();
        }
        public static bool IsNotDuplicated(string url , List<Epi> epis)
        {
            IEnumerable<Epi> found = epis.Where(p => p.Url == url);
            return !found.Any();
        }


        public static void Log(Exception ex) { string str = ex.ToString(); Log(str); return; if (str.Length > 2500) str = str.GetMaxLengthDots(2500); Log(str); }
        public static void Log(string log)
        {
            int tries = 0;
            try 
            {
                using (StreamWriter w = File.AppendText(vals.LogPath))
                {
                    w.Write(".\n\n" + vals.now 
                            //+ " ***************************************************************************************************************************************\n" 
                            + "  ::  " 
                            + log);
                    w.Dispose();
                }
            } 
            catch (Exception ex) { if (tries < 5) { tries++; Thread.Sleep(10); Log(log); } }
        }


        public static string AudioDuration(string FileFullPath)
        {
            TagLib.File file = TagLib.File.Create(FileFullPath);
            int s_time = (int) file.Properties.Duration.TotalSeconds;
            int s_minutes = s_time/60;
            int s_seconds = s_time%60;
            return s_minutes.ToString() + ":" + s_seconds.ToString() + " minutes";
        }
        public static double FileDuration(string FileFullPath)
        {
            TagLib.File file = TagLib.File.Create(FileFullPath);
            return file.Properties.Duration.TotalSeconds;
        }


        
        public static RoutedCommand RegCmd<T>(string name , ExecutedRoutedEventHandler handler)
        {
            RoutedCommand com = new RoutedCommand(name , typeof (T));
            CommandManager.RegisterClassCommandBinding(typeof (object) , new CommandBinding(com, handler, vals.CanExe));
            return com;
        }

    }








    

    
}