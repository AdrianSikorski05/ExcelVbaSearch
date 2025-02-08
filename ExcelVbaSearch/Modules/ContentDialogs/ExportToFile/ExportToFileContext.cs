using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;

namespace ExcelVbaSearch
{
    partial class  ExportToFileContext : ObservableValidator
    {
        private List<string> _excelsPath;

        [ObservableProperty]
        string _fileName;

        [ObservableProperty] 
        [Required(ErrorMessage = "Uzupełnij pole")]
        string _folderPath = "";
        partial void OnFolderPathChanged(string value)
        {
            ValidateProperty(value, nameof(FolderPath));
        }

        [ObservableProperty] int _progress = 0;
        [ObservableProperty] int _progressMaximum;
        [ObservableProperty] StswProgressState _progressState = StswProgressState.Ready;

        public ExportToFileContext(List<string> pathsExcel, int progressMaximum)
        {          
            _excelsPath = pathsExcel;
            _progressMaximum = progressMaximum;
        }

        [RelayCommand]
        private async Task Export()
        {
            try
            {
                if (!Validate())
                {
                    return;
                }

                string outputFile = System.IO.Path.Combine(FolderPath, string.IsNullOrEmpty(FileName) ? "test.txt" : FileName + ".txt");
                Progress = 0;
                ProgressState = StswProgressState.Running;

                using (var writer = new System.IO.StreamWriter(outputFile))
                {
                    
                    for (int i = 0; i < ProgressMaximum; i++)
                    {                     
                        await writer.WriteLineAsync(_excelsPath[i]);                      
                        Progress = (i + 1) * 100 / ProgressMaximum;                                             
                    }
                }
                ProgressState = StswProgressState.Finished;

                StswContentDialog.Close(nameof(MainView), true);
            }
            catch (Exception ex)
            {
                await StswMessageDialog.Show("Błąd", "Błąd", ex.Message, StswDialogButtons.OK, StswDialogImage.Error);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            StswContentDialog.Close(nameof(MainView), false);
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

        private bool Validate() 
        {
            ValidateAllProperties();
            return !HasErrors;
        }
    }
}
