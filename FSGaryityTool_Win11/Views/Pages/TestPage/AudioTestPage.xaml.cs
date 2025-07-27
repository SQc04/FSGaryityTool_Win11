using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using FSGaryityTool_Win11.Core.SpeechReconition;
using FSGaryityTool_Win11.Controls;
using NAudio.Wave;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Views.Pages.TestPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AudioTestPage : Page
    {
        string filePath = Path.Combine(AppContext.BaseDirectory, "Audio", "TestAudio", "D13_905.wav");

        public AudioTestPage()
        {
            this.InitializeComponent();
        }

        private int LoadAudioData()
        {
            int sampleRate;
            using (var reader = new AudioFileReader(filePath))
            {
                sampleRate = reader.WaveFormat.SampleRate;
            }
            return sampleRate;
        }

        private int GetAudioDatasampleRate(string filePath)
        {
            int sampleRate;
            using (var reader = new AudioFileReader(filePath))
            {
                sampleRate = reader.WaveFormat.SampleRate;
            }
            return sampleRate;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // 打开文件选择器
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            picker.FileTypeFilter.Add(".wav");
            picker.FileTypeFilter.Add(".mp3");

            // 获取当前窗口的句柄
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            // 显示文件选择器
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // 更新 filePath
                filePath = file.Path;

                // 加载并处理音频文件
                LoadwaveFormBox();
            }
        }

        private async void LoadwaveFormBox()
        {
            try
            {
                int sampleRate = LoadAudioData();
                double[] audioData = Utils.ReadWavFile(filePath);
                waveFormBox.AudioData = audioData;
                waveFormBox.SampleRate = sampleRate;

                double[][] spectrumData2 = await Task.Run(() => SpeechRecognition.Spectrogram(audioData, sampleRate));
                waveFormBox2.SpectrumData = spectrumData2;
                waveFormBox2.SampleRate = sampleRate;

                DataPlotBox2.Data = spectrumData2;// [100]
                DataPlotBox2.SampleRate = sampleRate;

                double[][] spectrumData3 = await Task.Run(() => SpeechRecognition.CFCC(audioData, sampleRate));
                waveFormBox3.SpectrumData = spectrumData3;
                waveFormBox3.SampleRate = sampleRate;

                DataPlotBox3.Data = spectrumData3;// [100]

                double[][] spectrumData4 = await Task.Run(() => SpeechRecognition.GFCC(audioData, sampleRate));
                waveFormBox4.SpectrumData = spectrumData4;
                waveFormBox4.SampleRate = sampleRate;

                DataPlotBox4.Data = spectrumData4;// [100]

                double[][] spectrumData5 = await Task.Run(() => SpeechRecognition.MFCC(audioData, sampleRate));
                waveFormBox5.SpectrumData = spectrumData5;
                waveFormBox5.SampleRate = sampleRate;

                DataPlotBox5.Data = spectrumData5;// [100]

                double[][] spectrumData6 = await Task.Run(() => SpeechRecognition.Fbank(audioData, sampleRate));
                waveFormBox6.SpectrumData = spectrumData6;
                waveFormBox6.SampleRate = sampleRate;

                List<double[]> gammatoneFreqInfo = Utils.GenerateGammatoneFilterBankResponse(sampleRate, 26, 512);
                DataPlotBoxGammatone.Data = gammatoneFreqInfo;
                DataPlotBoxGammatone.SampleRate = sampleRate;
                DataPlotBoxGammatone.NumFilters = 26;
                DataPlotBoxGammatone.FftSize = 512;

                List<double[]> melFreqInfo = Utils.GenerateMelFilterBankResponse(sampleRate, 26, 512, false, true);
                DataPlotBox.Data = melFreqInfo;
                DataPlotBox.SampleRate = sampleRate;
                DataPlotBox.NumFilters = 26;
                DataPlotBox.FftSize = 512;

                List<double[]> cochlearFreqInfo = Utils.GenerateCochlearFilterBankResponse(sampleRate, 26, 512);
                DataPlotBoxCochlear.Data = cochlearFreqInfo;
                DataPlotBoxCochlear.SampleRate = sampleRate;
                DataPlotBoxCochlear.NumFilters = 26;
                DataPlotBoxCochlear.FftSize = 512;

                IsLoadeds = true;

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading waveform data: {ex.Message}");
                // 处理异常
            }
        }
        private bool IsLoadeds = false;

        private void SetGammatoneFilterBank()
        {
            if (filePath == null)
            {
                return;
            }
            if (!IsLoadeds)
            {
                return;
            }
            int sampleRate = LoadAudioData();
            double[] audioData = Utils.ReadWavFile(filePath);
            List<double[]> gammatoneFreqInfo = Utils.GenerateGammatoneFilterBankResponse(sampleRate, 26, 512);
            DataPlotBoxGammatone.Data = gammatoneFreqInfo;
            DataPlotBoxGammatone.SampleRate = sampleRate;
            DataPlotBoxGammatone.NumFilters = 26;
            DataPlotBoxGammatone.FftSize = 512;
        }
        private void SetMelFilterBank()
        {
            if(filePath == null)
            {
                return;
            }
            if (!IsLoadeds)
            {
                return;
            }
            int sampleRate = LoadAudioData();
            double[] audioData = Utils.ReadWavFile(filePath);
            List<double[]> melFreqInfo = Utils.GenerateMelFilterBankResponse(sampleRate, 26, 512, (bool)HammingWindowToggle.IsChecked, (bool)HmRateToggle.IsChecked, HammingAlphaValue.Value, HammingBetaValue.Value, HmRateValue.Value);
            DataPlotBox.Data = melFreqInfo;
            DataPlotBox.SampleRate = sampleRate;
            DataPlotBox.NumFilters = 26;
            DataPlotBox.FftSize = 512;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SetMelFilterBank(); 
            SetGammatoneFilterBank();
        }

        private void HammingAlphaValue_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            SetMelFilterBank();
        }

        private void HammingWindowToggle_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetMelFilterBank();
        }

        private string FolderPath;

        private async void SetFolderPath_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            // 获取当前窗口的句柄
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Window);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                FolderPath = folder.Path;
                FolderPathText.Text = folder.Path;
            }
            else
            {
                FolderPath = null;
            }
        }


        private async void Processing_Click(object sender, RoutedEventArgs e)
        {
            // 确保文件夹路径已选择
            if (string.IsNullOrEmpty(FolderPath))
            {
                ShowError("请选择文件夹路径");
                return;
            }

            string inputDirectory = Path.Combine(FolderPath, "AudioLib", "THCHS-30Low");
            string outputDirectory = Path.Combine(FolderPath, "ProcessingData");

            // 创建输出目录（如果不存在）
            Directory.CreateDirectory(outputDirectory);

            // 获取所有音频文件
            var audioFiles = Directory.EnumerateFiles(inputDirectory, "*.*", SearchOption.AllDirectories).Where(file => file.EndsWith(".wav") || file.EndsWith(".mp3")).ToList();

            // 开始处理进度
            ProcessingCharacteristicingProgressBar.ShowError = false;
            ProcessingCharacteristicingProgressBar.Minimum = 0;
            ProcessingCharacteristicingProgressBar.Maximum = audioFiles.Count;
            ProcessingCharacteristicingProgressBar.Value = 0;

            foreach (var audioFile in audioFiles)
            {
                try
                {
                    // 读取音频文件
                    double[] signal = Utils.ReadWavFile(audioFile);
                    int sampleRate = GetAudioDatasampleRate(audioFile);

                    // 处理音频文件
                    double[][] processedData = await Task.Run(() => SpeechRecognition.Processing(signal, sampleRate));

                    // 将处理后的数据保存为文件
                    string fileName = Path.GetFileNameWithoutExtension(audioFile) + ".txt";
                    string outputFilePath = Path.Combine(outputDirectory, fileName);
                    await SaveProcessedDataAsync(outputFilePath, processedData);

                    // 更新进度条
                    ProcessingCharacteristicingProgressBar.Value += 1;
                }
                catch (Exception ex)
                {
                    ShowError($"处理文件 {audioFile} 失败：{ex.Message}");
                }
            }

            // 完成处理
            ProcessingCharacteristicingProgressBar.Value = ProcessingCharacteristicingProgressBar.Maximum;
        }

        private async Task SaveProcessedDataAsync(string filePath, double[][] processedData)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var frame in processedData)
                {
                    await writer.WriteLineAsync(string.Join(",", frame));
                }
            }
        }

        private async void Characteristicing_Click(object sender, RoutedEventArgs e)
        {
            string mode = (CharacteristicingMode.SelectedItem as ComboBoxItem)?.Content.ToString();

            // 设置训练数据输入/输出路径
            string trainInputDirectory = Path.Combine(FolderPath, "ProcessingData");
            string audioDirectory = Path.Combine(FolderPath, "AudioLib", "THCHS-30Low");
            string trainOutputBaseDir = Path.Combine(FolderPath, "CharacteristicData");
            string trainOutputDirectory = Path.Combine(trainOutputBaseDir, mode);

            // 设置验证数据输入/输出路径
            string valInputDirectory = Path.Combine(FolderPath, "PredictData");
            string valAudioDirectory = Path.Combine(FolderPath, "AudioLib", "THCHS-30Pre");
            string valOutputBaseDir = Path.Combine(FolderPath, "PreCharacteristicData");
            string valOutputDirectory = Path.Combine(valOutputBaseDir, mode);

            // 检查训练数据目录是否存在
            if (!Directory.Exists(trainInputDirectory) || !Directory.GetFiles(trainInputDirectory).Any())
            {
                ShowError("未找到训练数据，请先进行Processing处理。");
                return;
            }

            // 检查验证数据目录是否存在
            if (!Directory.Exists(valInputDirectory) || !Directory.GetFiles(valInputDirectory).Any())
            {
                ShowError("未找到验证数据，请先进行ProcessingPre处理。");
                return;
            }

            try
            {
                // 创建训练和验证数据输出目录
                Directory.CreateDirectory(trainOutputDirectory);
                Directory.CreateDirectory(valOutputDirectory);

                // 获取所有训练数据文件
                var trainProcessedFiles = Directory.GetFiles(trainInputDirectory, "*.txt");

                // 获取所有验证数据文件
                var valProcessedFiles = Directory.GetFiles(valInputDirectory, "*.txt");

                // 开始处理进度
                ProcessingCharacteristicingProgressBar.ShowError = false;
                ProcessingCharacteristicingProgressBar.Minimum = 0;
                ProcessingCharacteristicingProgressBar.Maximum = trainProcessedFiles.Length + valProcessedFiles.Length;
                ProcessingCharacteristicingProgressBar.Value = 0;

                // 处理所有训练数据文件
                await Task.Run(() =>
                {
                    Parallel.ForEach(trainProcessedFiles, file =>
                    {
                        try
                        {
                            // 从文件中读取预处理数据
                            double[][] processedData = File.ReadAllLines(file).Select(line => line.Split(',').Select(double.Parse).ToArray()).ToArray();

                            // 获取对应的音频文件路径
                            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                            string audioFilePath = Directory.GetFiles(audioDirectory, $"{fileNameWithoutExtension}.*")
                                .FirstOrDefault(f => f.EndsWith(".wav") || f.EndsWith(".mp3"));

                            if (audioFilePath == null)
                            {
                                throw new FileNotFoundException($"未找到与 {file} 对应的音频文件。");
                            }

                            // 获取音频文件的采样率
                            int sampleRate = GetAudioDatasampleRate(audioFilePath);

                            // 根据模式选择处理方式
                            double[][] result;
                            switch (mode)
                            {
                                case "CFCC":
                                    result = SpeechRecognition.ProcessCFCC(processedData, sampleRate);
                                    break;
                                case "GFCC":
                                    result = SpeechRecognition.ProcessGFCC(processedData, sampleRate);
                                    break;
                                case "MFCC":
                                    result = SpeechRecognition.ProcessMFCC(processedData, sampleRate);
                                    break;
                                case "SDC":
                                    result = SpeechRecognition.ProcessSDC(processedData, sampleRate);
                                    break;
                                case "WeightedFusion":
                                    double[][] cfccFeatures = LoadFeatureData(Path.Combine(trainOutputBaseDir, "CFCC", Path.GetFileName(file)));
                                    double[][] gfccFeatures = LoadFeatureData(Path.Combine(trainOutputBaseDir, "GFCC", Path.GetFileName(file)));
                                    double[][] mfccFeatures = LoadFeatureData(Path.Combine(trainOutputBaseDir, "MFCC", Path.GetFileName(file)));
                                    result = SpeechRecognition.WeightedFeatureFusion(cfccFeatures, gfccFeatures, mfccFeatures, 0.33, 0.33, 0.34);
                                    break;
                                case "FeatureSplicingFusion":
                                    double[][] cfccFeaturesSplice = LoadFeatureData(Path.Combine(trainOutputBaseDir, "CFCC", Path.GetFileName(file)));
                                    double[][] gfccFeaturesSplice = LoadFeatureData(Path.Combine(trainOutputBaseDir, "GFCC", Path.GetFileName(file)));
                                    double[][] mfccFeaturesSplice = LoadFeatureData(Path.Combine(trainOutputBaseDir, "MFCC", Path.GetFileName(file)));
                                    result = SpeechRecognition.FeatureSplicingFusion(cfccFeaturesSplice, gfccFeaturesSplice, mfccFeaturesSplice);
                                    break;
                                default:
                                    throw new InvalidOperationException("未知模式");
                            }

                            // 保存训练数据
                            string outputPath = Path.Combine(trainOutputDirectory, Path.GetFileName(file));
                            SaveProcessedData(outputPath, result);

                            // 更新进度条
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                ProcessingCharacteristicingProgressBar.Value += 1;
                            });
                        }
                        catch (Exception ex)
                        {
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                ShowError($"处理训练数据文件 {file} 失败：{ex.Message}");
                            });
                        }
                    });
                });

                // 处理所有验证数据文件
                await Task.Run(() =>
                {
                    Parallel.ForEach(valProcessedFiles, file =>
                    {
                        try
                        {
                            // 从文件中读取预处理数据
                            double[][] processedData = File.ReadAllLines(file).Select(line => line.Split(',').Select(double.Parse).ToArray()).ToArray();

                            // 获取对应的音频文件路径
                            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                            string audioFilePath = Directory.GetFiles(valAudioDirectory, $"{fileNameWithoutExtension}.*")
                                .FirstOrDefault(f => f.EndsWith(".wav") || f.EndsWith(".mp3"));

                            if (audioFilePath == null)
                            {
                                throw new FileNotFoundException($"未找到与 {file} 对应的音频文件。");
                            }

                            // 获取音频文件的采样率
                            int sampleRate = GetAudioDatasampleRate(audioFilePath);

                            // 根据模式选择处理方式
                            double[][] result;
                            switch (mode)
                            {
                                case "CFCC":
                                    result = SpeechRecognition.ProcessCFCC(processedData, sampleRate);
                                    break;
                                case "GFCC":
                                    result = SpeechRecognition.ProcessGFCC(processedData, sampleRate);
                                    break;
                                case "MFCC":
                                    result = SpeechRecognition.ProcessMFCC(processedData, sampleRate);
                                    break;
                                case "SDC":
                                    result = SpeechRecognition.ProcessSDC(processedData, sampleRate);
                                    break;
                                case "WeightedFusion":
                                    double[][] cfccFeatures = LoadFeatureData(Path.Combine(valOutputBaseDir, "CFCC", Path.GetFileName(file)));
                                    double[][] gfccFeatures = LoadFeatureData(Path.Combine(valOutputBaseDir, "GFCC", Path.GetFileName(file)));
                                    double[][] mfccFeatures = LoadFeatureData(Path.Combine(valOutputBaseDir, "MFCC", Path.GetFileName(file)));
                                    result = SpeechRecognition.WeightedFeatureFusion(cfccFeatures, gfccFeatures, mfccFeatures, 0.33, 0.33, 0.34);
                                    break;
                                case "FeatureSplicingFusion":
                                    double[][] cfccFeaturesSplice = LoadFeatureData(Path.Combine(valOutputBaseDir, "CFCC", Path.GetFileName(file)));
                                    double[][] gfccFeaturesSplice = LoadFeatureData(Path.Combine(valOutputBaseDir, "GFCC", Path.GetFileName(file)));
                                    double[][] mfccFeaturesSplice = LoadFeatureData(Path.Combine(valOutputBaseDir, "MFCC", Path.GetFileName(file)));
                                    result = SpeechRecognition.FeatureSplicingFusion(cfccFeaturesSplice, gfccFeaturesSplice, mfccFeaturesSplice);
                                    break;
                                default:
                                    throw new InvalidOperationException("未知模式");
                            }

                            // 保存验证数据
                            string outputPath = Path.Combine(valOutputDirectory, Path.GetFileName(file));
                            SaveProcessedData(outputPath, result);

                            // 更新进度条
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                ProcessingCharacteristicingProgressBar.Value += 1;
                            });
                        }
                        catch (Exception ex)
                        {
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                ShowError($"处理验证数据文件 {file} 失败：{ex.Message}");
                            });
                        }
                    });
                });

                // 完成处理
                ProcessingCharacteristicingProgressBar.Value = ProcessingCharacteristicingProgressBar.Maximum;
            }
            catch (Exception ex)
            {
                ShowError($"处理失败：{ex.Message}");
            }
        }

        private double[][] LoadFeatureData(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"未找到特征数据文件：{filePath}");
            }

            return File.ReadAllLines(filePath).Select(line => line.Split(',').Select(double.Parse).ToArray()).ToArray();
        }

        private static double[] Flatten(double[][] data)
        {
            return data.SelectMany(frame => frame).ToArray();
        }

        private void SaveProcessedData(string path, double[][] data)
        {
            using var writer = new StreamWriter(path);
            foreach (var frame in data)
            {
                writer.WriteLine(string.Join(",", frame));
            }
        }
        private void ShowError(string message)
        {
            // 显示错误信息
            // 这里可以根据需要实现具体的错误显示逻辑，例如弹出对话框或在界面上显示错误信息
            ProcessingCharacteristicingProgressBar.ShowError = true;
            Debug.WriteLine(message);
        }

        private async void DebugData_Click(object sender, RoutedEventArgs e)
        {
            // 打开文件选择器
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".txt");

            // 获取当前窗口的句柄
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            // 显示文件选择器
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                try
                {
                    // 读取文件内容
                    IList<string> lines = await FileIO.ReadLinesAsync(file);
                    double[][] data = lines.Select(line => line.Split(',').Select(double.Parse).ToArray()).ToArray();

                    // 获取对应的音频文件路径
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Path);
                    string audioDirectory = Path.Combine(FolderPath, "AudioLib", "THCHS-30Low");
                    string[] audioFiles = Directory.GetFiles(audioDirectory, $"{fileNameWithoutExtension}.*");

                    if (audioFiles.Length == 0)
                    {
                        throw new FileNotFoundException($"未找到与 {file.Path} 对应的音频文件。");
                    }

                    string audioFilePath = audioFiles.FirstOrDefault(f => f.EndsWith(".wav") || f.EndsWith(".mp3"));

                    if (audioFilePath == null)
                    {
                        throw new FileNotFoundException($"未找到与 {file.Path} 对应的音频文件。");
                    }

                    // 获取音频文件的采样率
                    int sampleRate;
                    using (var reader = new AudioFileReader(audioFilePath))
                    {
                        sampleRate = reader.WaveFormat.SampleRate;
                    }

                    // 将数据加载到 waveFormBox7
                    waveFormBox7.SpectrumData = data;
                    waveFormBox7.SampleRate = sampleRate;

                    // 更新进度条
                    ProcessingCharacteristicingProgressBar.Value = ProcessingCharacteristicingProgressBar.Maximum;
                }
                catch (Exception ex)
                {
                    ShowError($"处理失败：{ex.Message}");
                }
            }
            else
            {
                ShowError("未选择文件");
            }
        }

        private async void ProcessingPre_Click(object sender, RoutedEventArgs e)
        {
            // 确保文件夹路径已选择
            if (string.IsNullOrEmpty(FolderPath))
            {
                ShowError("请选择文件夹路径");
                return;
            }

            string preDirectory = Path.Combine(FolderPath, "AudioLib", "THCHS-30Pre");
            string inputDirectory = Path.Combine(FolderPath, "PredictData");

            // 创建输入目录（如果不存在）
            Directory.CreateDirectory(inputDirectory);

            // 获取所有音频文件
            var audioFiles = Directory.EnumerateFiles(preDirectory, "*.*", SearchOption.AllDirectories)
                                      .Where(file => file.EndsWith(".wav") || file.EndsWith(".mp3")).ToList();

            // 开始处理进度
            ProcessingCharacteristicingProgressBar.ShowError = false;
            ProcessingCharacteristicingProgressBar.Minimum = 0;
            ProcessingCharacteristicingProgressBar.Maximum = audioFiles.Count;
            ProcessingCharacteristicingProgressBar.Value = 0;

            foreach (var audioFile in audioFiles)
            {
                try
                {
                    // 读取音频文件
                    double[] signal = Utils.ReadWavFile(audioFile);
                    int sampleRate = GetAudioDatasampleRate(audioFile);

                    // 处理音频文件
                    double[][] processedData = await Task.Run(() => SpeechRecognition.Processing(signal, sampleRate));

                    // 将处理后的数据保存为文件
                    string fileName = Path.GetFileNameWithoutExtension(audioFile) + ".txt";
                    string outputFilePath = Path.Combine(inputDirectory, fileName);
                    await SaveProcessedDataAsync(outputFilePath, processedData);

                    // 更新进度条
                    ProcessingCharacteristicingProgressBar.Value += 1;
                }
                catch (Exception ex)
                {
                    ShowError($"处理文件 {audioFile} 失败：{ex.Message}");
                }
            }

            // 完成处理
            ProcessingCharacteristicingProgressBar.Value = ProcessingCharacteristicingProgressBar.Maximum;
        }
    }
}
