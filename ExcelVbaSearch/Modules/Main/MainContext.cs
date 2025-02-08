using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ExcelVbaSearch
{
    public partial class MainContext : ObservableValidator
    {

        [ObservableProperty]
        [Required(ErrorMessage = "Uzupełnij pole")]
        string _searchingText = "";
        partial void OnSearchingTextChanged(string value)
        {
            ValidateProperty(value, nameof(SearchingText));
        }

        [ObservableProperty]
        [Required(ErrorMessage = "Uzupełnij pole")]
        [DirectoryExists]
        string _folderPath = "";
        partial void OnFolderPathChanged(string value)
        {
            ValidateProperty(value, nameof(FolderPath));
        }

        [ObservableProperty]List<string> _excelsPath = new();
        [ObservableProperty] bool _isBusySearch;
        [ObservableProperty] bool _isBusyExport;
        [ObservableProperty] int _foundFilesCount;
        [ObservableProperty] TimeSpan _currentTime;
        [ObservableProperty] bool? _startStopReset;

        public MainContext() { }

        [RelayCommand]
        public async Task Search()
        {
            try
            {
                StartStopReset = null;
                ExcelsPath.Clear();
                FoundFilesCount = 0;
                if (!Validate())
                    return;

                IsBusySearch = true;
                StartStopReset = true;

                byte[] scriptBytes = Properties.Resources.script;
                string pythonScriptContent = System.Text.Encoding.UTF8.GetString(scriptBytes);

                if (string.IsNullOrWhiteSpace(pythonScriptContent))
                    throw new Exception("Nie udało się załadować skryptu Python z zasobów.");
                

                string? arguments = $"-c \"{pythonScriptContent.Replace("\"", "\\\"")}\" \"{FolderPath}\" \"{SearchingText}\"";

                ProcessStartInfo start = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                var outputLines = new List<string>();

                using (var process = new Process())
                {
                    process.StartInfo = start;

                    process.EnableRaisingEvents = true;
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data is not null)
                        {
                            lock (outputLines)
                            {
                                outputLines.Add(e.Data);
                            }
                        }
                    };
                    process.Start();
                    process.BeginOutputReadLine();
                    await process.WaitForExitAsync();
                }

                ExcelsPath = outputLines.Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
                FoundFilesCount = ExcelsPath.Count;
                StartStopReset = false;

                await StswMessageDialog.Show("Zakończono wyszukiwanie", "Informacja", $"Znaleziono frazę \"{SearchingText}\" w {FoundFilesCount} excelach. Czas: {CurrentTime.ToString("hh\\:mm\\:ss")}", StswDialogButtons.OK, StswDialogImage.Success);

            }
            catch (Exception ex)
            {
                await StswMessageDialog.Show("Błąd", "Błąd", ex.Message, StswDialogButtons.OK, StswDialogImage.Error);
            }
            finally
            {
                IsBusySearch = false;
            }
        }

        [RelayCommand]
        public async Task ExportToFile() 
        {
            try
            {
                if (ExcelsPath.Count != 0)
                {
                    IsBusyExport = true;
                    await StswContentDialog.Show(new ExportToFileContext(ExcelsPath, ExcelsPath.Count()), nameof(MainView));
                    IsBusyExport = false;
                }
            }
            catch (Exception ex)
            {
                await StswMessageDialog.Show("Błąd", "Błąd", ex.Message, StswDialogButtons.OK, StswDialogImage.Error);               
            }
            finally
            {
                IsBusyExport = false;
            }
        }

        [RelayCommand]
        private void ClearBox(object? parameter)
        {
            try
            {
                if (parameter is StswTextBox textBox)
                {
                    textBox.Text = string.Empty;
                }
                else if (parameter is StswPathPicker passwordBox)
                {
                    passwordBox.SelectedPath = string.Empty;
                }
            }
            catch (Exception ex)
            {
                StswMessageDialog.Show($"Error during cleaning box: {ex.Message}", "Error", "", StswDialogButtons.OK, StswDialogImage.Blockade);
            }
        }

        [RelayCommand]
        public void CopyNameExcel(string pathExcel) 
        {
            try
            {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(pathExcel);

                Clipboard.SetText(fileNameWithoutExt);
            }
            catch (Exception ex)
            {
                StswMessageDialog.Show($"Error during copy name excel: {ex.Message}", "Error", "", StswDialogButtons.OK, StswDialogImage.Blockade);
            }
        }

        private bool Validate()
        {
            ValidateAllProperties();
            return !HasErrors;
        }
    }
}
