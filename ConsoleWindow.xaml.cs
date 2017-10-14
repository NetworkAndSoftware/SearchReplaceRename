using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace SearchReplaceRename
{
  /// <summary>
  ///   Interaction logic for ConsoleWindow.xaml
  /// </summary>
  public partial class ConsoleWindow : Window
  {
    public ConsoleWindow(string path, string search, string replace)
    {
      Path = path;
      Search = search;
      Replace = replace;

      InitializeComponent();
    }

    public string Path { get; set; }
    public string Search { get; set; }
    public string Replace { get; set; }

    private void Window_Initialized(object sender, EventArgs e)
    {
      var worker = new BackgroundWorker();
      worker.DoWork += DoWork;
      worker.RunWorkerCompleted += RunWorkerCompleted;
      worker.RunWorkerAsync();
    }

    private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
    {
      btnDismiss.IsEnabled = true;
    }

    private void DoWork(object sender, DoWorkEventArgs args)
    {
      if (!Directory.Exists(Path))
      {
        WriteLine("ERROR: Path does not exist. Please select a valid path and try again");
      }
      else
      {
        WriteLine(
          string.Format(@"Renaming all files and directories containing '{0}' to have '{1}' instead in path '{2}'",
            Search, Replace, Path));
        SearchAndReplaceRename(Search, Replace, Path);
      }
    }

    private void WriteLineSync(string line)
    {
      textBox1.Text += "\n" + line;
    }

    private void WriteLine(string line)
    {
      Dispatcher.Invoke(DispatcherPriority.Normal, new Action<string>(WriteLineSync), line);
    }

    private void SearchAndReplaceRename(string search, string replace, string path)
    {
      foreach (var file in Directory.GetFiles(path))
        RenameIfMatch(file, search, replace);

      foreach (var directory in Directory.GetDirectories(path))
      {
        SearchAndReplaceRename(search, replace, directory);
        RenameIfMatch(directory, search, replace);
      }
    }

    private static string LastPathSegment(string path)
    {
      var parts = path.Split('\\');
      return parts.Length > 0 ? parts[parts.Length - 1] : path;
    }

    private static string ExtendPath(string path, string name)
    {
      const string FORMAT = @"{0}\{1}";
      return string.Format(FORMAT, path, name);
    }

    private void RenameIfMatch(string path, string search, string replace)
    {
      var index = path.LastIndexOf('\\');
      var pathbase = index == -1 ? string.Empty : path.Substring(0, index);
      var last = path.Substring(index + 1);

      if (last.Contains(search))
      {
        var oldname = ExtendPath(pathbase, last);
        var newname = ExtendPath(pathbase, last.Replace(search, replace));

        WriteLine(string.Format(@"Renaming '{0}' to '{1}'", oldname, newname));

        try
        {
          Directory.Move(oldname, newname);
        }
        catch (Exception x)
        {
          WriteLine(string.Format(@"Exception: {0}", x.Message));
        }
      }
    }

    private void btnDismiss_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }
  }
}