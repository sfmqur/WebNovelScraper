using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using WebNovelScraper.ViewModels;

namespace WebNovelScraper.Views;

public partial class MainWindow : Window
{
  public MainWindow()
  {
    InitializeComponent();
  }

  protected override void OnClosed(EventArgs e)
  {
    base.OnClosed(e);
    if (DataContext is MainWindowViewModel vm)
      vm.SaveConfig();
  }

  private async void BrowseOutputDir_Click(object? sender, RoutedEventArgs e)
  {
    var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
    {
      Title = "Select Output Directory",
      AllowMultiple = false
    });
    
    if (folders.Count > 0 && DataContext is MainWindowViewModel vm)
    {
      vm.OutputDir = folders[0].Path.LocalPath;
    }
    else if (folders.Count == 0 && DataContext is MainWindowViewModel vm2)
    {
      vm2.OutputDir = vm2.DefaultOutputPath;
    }
  }
}