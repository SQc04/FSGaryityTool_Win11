using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using NAudio.Wave;
using MathNet.Numerics.IntegralTransforms;

namespace FSGaryityTool_Win11.Core.SpeechReconition
{
    public class Utils
    {
        public static double[] ReadWavFile(string filePath)
        {
            using (var reader = new AudioFileReader(filePath))
            {
                int sampleCount = (int)reader.Length / sizeof(float);
                float[] buffer = new float[sampleCount];
                int readSamples = reader.Read(buffer, 0, sampleCount);

                // 将float转换为double
                double[] signal = new double[readSamples];
                for (int i = 0; i < readSamples; i++)
                {
                    signal[i] = buffer[i];
                }
                return signal;
            }
        }

        //预处理组

        // 预处理去除直流分量
        public static double[] Preprocess(double[] signal)
        {
            // 减去信号的均值来去除直流分量
            double mean = signal.Average();
            return signal.Select(s => s - mean).ToArray();
        }

        // 预加重
        public static double[] PreEmphasis(double[] signal, double preEmphasisCoefficient = 0.97)
        {
            double[] emphasizedSignal = new double[signal.Length];
            emphasizedSignal[0] = signal[0];
            for (int i = 1; i < signal.Length; i++)
            {
                emphasizedSignal[i] = signal[i] - preEmphasisCoefficient * signal[i - 1];
            }
            return emphasizedSignal;
        }

        // 分帧
        public static double[][] FrameSignal(double[] signal, int sampleRate, int frameStrideMs = 20, int frameSizeMs = 20)
        {
            // 计算每帧的样本数
            int frameSize = (int)(sampleRate * frameSizeMs / 1000.0);
            int frameStride = (int)(sampleRate * frameStrideMs / 1000.0);

            // 计算帧的数量
            int numFrames = (int)Math.Ceiling((double)(signal.Length - frameSize) / frameStride) + 1;
            double[][] frames = new double[numFrames][];

            for (int i = 0; i < numFrames; i++)
            {
                int startIdx = i * frameStride;
                frames[i] = new double[frameSize];
                Array.Copy(signal, startIdx, frames[i], 0, Math.Min(frameSize, signal.Length - startIdx));
            }
            return frames;
        }


        // 加窗（Hamming窗）
        public static double[] Windowing(double[] frame)
        {
            int frameSize = frame.Length;
            double[] windowedFrame = new double[frameSize];
            for (int i = 0; i < frameSize; i++)
            {
                windowedFrame[i] = frame[i] * (0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (frameSize - 1)));
            }
            return windowedFrame;
        }
        public static double[] ZeroPadToNextPowerOfTwo(double[] frame)
        {
            int n = frame.Length;
            int nextPowerOfTwo = 1;
            while (nextPowerOfTwo < n)
            {
                nextPowerOfTwo <<= 1;
            }

            double[] paddedFrame = new double[nextPowerOfTwo];
            Array.Copy(frame, paddedFrame, n);
            return paddedFrame;
        }

        // FFT
        public static Complex[] FFT(double[] frame)
        {
            int n = frame.Length;

            // 检查信号长度是否为 2 的幂次
            if ((n & (n - 1)) != 0)
            {
                throw new ArgumentException("信号长度必须是 2 的幂次。");
            }

            // 将 double[] 转换为 Complex[]
            Complex[] complexSignal = new Complex[n];
            for (int i = 0; i < n; i++)
            {
                complexSignal[i] = new Complex(frame[i], 0);
            }

            // 调用迭代 FFT
            Complex[] fftResult = IterativeFFT(complexSignal);

            return fftResult;
        }

        // 迭代 FFT 实现
        private static Complex[] IterativeFFT(Complex[] signal)
        {
            int n = signal.Length;

            // 位反转重排
            Complex[] result = BitReverseReorder(signal);

            // 迭代计算 FFT
            for (int s = 1; s <= Math.Log(n, 2); s++)
            {
                int m = 1 << s; // 当前段的长度
                double angle = -2 * Math.PI / m;
                Complex wm = new Complex(Math.Cos(angle), Math.Sin(angle)); // 旋转因子

                for (int k = 0; k < n; k += m)
                {
                    Complex w = new Complex(1, 0); // 初始化旋转因子
                    for (int j = 0; j < m / 2; j++)
                    {
                        // 蝶形运算
                        Complex t = w * result[k + j + m / 2];
                        Complex u = result[k + j];
                        result[k + j] = u + t;
                        result[k + j + m / 2] = u - t;
                        w *= wm; // 更新旋转因子
                    }
                }
            }

            return result;
        }

        // 位反转重排
        private static Complex[] BitReverseReorder(Complex[] signal)
        {
            int n = signal.Length;
            Complex[] result = new Complex[n];
            int bitCount = (int)Math.Log(n, 2);

            for (int i = 0; i < n; i++)
            {
                int reversedIndex = ReverseBits(i, bitCount);
                result[reversedIndex] = signal[i];
            }

            return result;
        }

        // 位反转函数
        private static int ReverseBits(int value, int bitCount)
        {
            int reversed = 0;
            for (int i = 0; i < bitCount; i++)
            {
                reversed = (reversed << 1) | (value & 1);
                value >>= 1;
            }
            return reversed;
        }

        // 计算谱线能量
        public static double[] ComputeSpectrumEnergy(Complex[] fftResult)
        {
            int spectrumLength = fftResult.Length;
            int validLength = spectrumLength / 2 + 1;
            double[] spectrumEnergy = new double[validLength];

            for (int i = 0; i < validLength; i++)
            {
                spectrumEnergy[i] = fftResult[i].Magnitude * fftResult[i].Magnitude;
            }

            return spectrumEnergy;
        }

        // 对数化
        public static double[] ApplyLog(double[] signal)
        {
            double maxVal = signal.Max();
            double epsilon = Math.Max(1e-20, maxVal * 1e-10); // 动态设置最小阈值

            const double MIN_DB = -100; // 防止数值溢出
            const double MAX_DB = 100;

            double[] result = new double[signal.Length];
            Parallel.For(0, signal.Length, i =>
            {
                double compressed = Math.Sqrt(signal[i] + epsilon); // 预压缩提高数值稳定性
                double db = 10 * Math.Log10(compressed);

                db = db < MIN_DB ? MIN_DB * (1 - Math.Exp(-(db - MIN_DB))) : db;
                db = db > MAX_DB ? MAX_DB - 0.1 * (db - MAX_DB) : db;

                result[i] = db;
            });

            // 全局归一化
            double mean = result.Average();
            double stdDev = Math.Sqrt(result.Select(x => Math.Pow(x - mean, 2)).Average());
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (result[i] - mean) / stdDev; // Z-score标准化
            }

            return result;
        }


        // 离散余弦变换DCT
        public static double[] DCT(double[] signal)
        {
            int n = signal.Length;
            double[] result = new double[n];
            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    result[k] += signal[i] * Math.Cos(Math.PI * k * (2 * i + 1) / (2 * n));
                }
            }
            return result;
        }

        // 倒谱分析
        public static double[] ComputeCepstrum(double[] signal, bool isDeltaCepstrum = false)
        {
            int n = signal.Length;
            double[] cepstrum = new double[n];
            double[] dctResult = DCT(signal);

            if (isDeltaCepstrum)
            {
                // 差分倒谱分析
                for (int i = 1; i < n; i++)
                {
                    cepstrum[i - 1] = dctResult[i] - dctResult[i - 1];
                }
                cepstrum[n - 1] = 0; // 最后一个位置填零
            }
            else
            {
                // 普通倒谱分析
                Array.Copy(dctResult, cepstrum, n);
            }

            return cepstrum;
        }


        //滤波器组

        //Cochlear滤波器
        public static double[] CochlearFilterBank(double[] signal, int sampleRate, int numFilters, double bandWidth = 100)
        {
            double[] filteredSignal = new double[signal.Length];
            double[] centerFrequencies = Linspace(20, sampleRate / 2.0, numFilters);

            for (int i = 0; i < numFilters; i++)
            {
                // 设置滤波器参数
                double centerFrequency = centerFrequencies[i];
                for (int j = 0; j < signal.Length; j++)
                {
                    filteredSignal[j] += signal[j] * Math.Exp(-Math.Pow((centerFrequency - j), 2) / (2 * Math.Pow(bandWidth, 2)));
                }
            }
            return filteredSignal;
        }

        public static List<double[]> GenerateCochlearFilterBankResponse(int sampleRate, int numFilters, int fftSize, double bandWidth = 100)
        {
            List<double[]> cochlearFilters = new List<double[]>();
            double[] centerFrequencies = Linspace(20, sampleRate / 2.0, numFilters);

            for (int i = 0; i < numFilters; i++)
            {
                double[] filter = new double[fftSize];
                double centerFrequency = centerFrequencies[i];

                // 模拟 CochlearFilterBank 的时域滤波器定义
                for (int j = 0; j < fftSize; j++)
                {
                    filter[j] = Math.Exp(-Math.Pow((centerFrequency - j), 2) / (2 * Math.Pow(bandWidth, 2)));
                }

                // 计算频率响应（FFT）
                Complex[] complexFilter = Array.ConvertAll(filter, x => new Complex(x, 0));
                Fourier.Forward(complexFilter, FourierOptions.Matlab);
                double[] magnitudeResponse = new double[fftSize / 2 + 1];
                for (int k = 0; k < magnitudeResponse.Length; k++)
                {
                    magnitudeResponse[k] = complexFilter[k].Magnitude;
                }

                // 归一化频率响应
                double maxMagnitude = magnitudeResponse.Max();
                if (maxMagnitude > 0) // 防止除以零
                {
                    for (int k = 0; k < magnitudeResponse.Length; k++)
                    {
                        magnitudeResponse[k] /= maxMagnitude;
                    }
                }

                cochlearFilters.Add(magnitudeResponse);
            }

            return cochlearFilters;
        }

        //GammaTone滤波器
        public static double[] GammatoneFilterBank(double[] spectrumEnergy, int sampleRate, int numFilters, double minBandWidth = 24.7, double order = 1, double earQ = 9.26449)
        {
            int validLength = spectrumEnergy.Length;
            double[] filteredSignal = new double[validLength];

            double lowFreq = 0;
            double highFreq = sampleRate / 2.0;
            double[] centerFrequencies = Linspace(lowFreq, highFreq, numFilters);

            for (int i = 0; i < numFilters; i++)
            {
                double centerFrequency = centerFrequencies[i];
                double ERB = (Math.Pow((centerFrequency / earQ), order) + Math.Pow(minBandWidth, order));
                int centerBin = (int)(centerFrequency * validLength / (sampleRate / 2));

                for (int j = 0; j < validLength; j++)
                {
                    filteredSignal[j] += spectrumEnergy[j] * Math.Exp(-Math.Pow((centerBin - j), 2) / (2 * ERB));
                }
            }

            return filteredSignal;
        }
        public static List<double[]> GenerateGammatoneFilterBankResponse(int sampleRate, int numFilters, int fftSize, double minBandWidth = 24.7, double order = 1, double earQ = 9.26449)
        {
            List<double[]> gammatoneFilters = new List<double[]>();

            double lowFreq = 0;
            double highFreq = sampleRate / 2.0;
            double[] centerFrequencies = Linspace(lowFreq, highFreq, numFilters);

            for (int i = 0; i < numFilters; i++)
            {
                double[] filter = new double[fftSize / 2 + 1];
                double centerFrequency = centerFrequencies[i];
                double ERB = (Math.Pow((centerFrequency / earQ), order) + Math.Pow(minBandWidth, order));
                int centerBin = (int)(centerFrequency * (fftSize / 2 + 1) / (sampleRate / 2));

                for (int j = 0; j < filter.Length; j++)
                {
                    filter[j] = Math.Exp(-Math.Pow((centerBin - j), 2) / (2 * ERB));
                }

                gammatoneFilters.Add(filter);
            }

            return gammatoneFilters;
        }

        //Mel滤波器
        public static double[] MelFilterBank(double[] spectrum, int sampleRate, int numFilters, bool useHamming = false, bool useHm = false, double alpha = 0.54, double beta = 0.46, double hmRate = 0.1)
        {
            int validLength = spectrum.Length;
            double[] melEnergies = new double[numFilters];
            double lowMel = 2595 * Math.Log10(1 + 0 / 700.0);
            double highMel = 2595 * Math.Log10(1 + (sampleRate / 2.0) / 700.0);
            double[] melPoints = Linspace(lowMel, highMel, numFilters + 2);
            int[] bin = melPoints.Select(mel => (int)Math.Floor(validLength * (700 * (Math.Pow(10, mel / 2595.0) - 1)) / sampleRate)).ToArray();

            for (int i = 1; i <= numFilters; i++)
            {
                int leftBin = bin[i - 1];
                int centerBin = bin[i];
                int rightBin = bin[i + 1];

                for (int j = leftBin; j < centerBin; j++)
                {
                    double weight = (j - leftBin) / (double)(centerBin - leftBin);
                    melEnergies[i - 1] += spectrum[j] * weight;
                }

                for (int j = centerBin; j < rightBin; j++)
                {
                    double weight = (rightBin - j) / (double)(rightBin - centerBin);
                    melEnergies[i - 1] += spectrum[j] * weight;
                }

                if (useHamming)
                {
                    melEnergies[i - 1] *= alpha - beta * Math.Cos(2 * Math.PI * i / (numFilters - 1));
                }

                if (useHm)
                {
                    double hm = 1.0 / (1.0 + Math.Exp(hmRate * (i - numFilters / 2)));
                    melEnergies[i - 1] *= hm;
                }
            }

            return melEnergies;
        }
        public static List<double[]> GenerateMelFilterBankResponse(int sampleRate, int numFilters, int fftSize, bool useHamming = false, bool useHm = false, double alpha = 0.54, double beta = 0.46, double hmRate = 0.1)
        {
            List<double[]> melFilters = new List<double[]>();

            double lowMel = 2595 * Math.Log10(1 + 0 / 700.0);
            double highMel = 2595 * Math.Log10(1 + (sampleRate / 2.0) / 700.0);
            double[] melPoints = Linspace(lowMel, highMel, numFilters + 2);
            int[] bin = melPoints.Select(mel => (int)Math.Floor((fftSize + 1) * (700 * (Math.Pow(10, mel / 2595.0) - 1)) / sampleRate)).ToArray();

            for (int i = 1; i <= numFilters; i++)
            {
                double[] filter = new double[fftSize / 2 + 1];
                int leftBin = bin[i - 1];
                int centerBin = bin[i];
                int rightBin = bin[i + 1];

                for (int j = leftBin; j < centerBin; j++)
                {
                    filter[j] = (j - leftBin) / (double)(centerBin - leftBin);
                }

                for (int j = centerBin; j < rightBin; j++)
                {
                    filter[j] = (rightBin - j) / (double)(rightBin - centerBin);
                }

                if (useHamming)
                {
                    for (int j = 0; j < filter.Length; j++)
                    {
                        filter[j] *= alpha - beta * Math.Cos(2 * Math.PI * j / (filter.Length - 1));
                    }
                }

                if (useHm)
                {
                    double hm = 1.0 / (1.0 + Math.Exp(hmRate * (i - numFilters / 2)));
                    for (int j = 0; j < filter.Length; j++)
                    {
                        filter[j] *= hm;
                    }
                }

                melFilters.Add(filter);
            }

            return melFilters;
        }

        // 生成等间隔的数值序列
        private static double[] Linspace(double start, double stop, int num)
        {
            double[] result = new double[num];
            double step = (stop - start) / (num - 1);
            for (int i = 0; i < num; i++)
            {
                result[i] = start + i * step;
            }
            return result;
        }

        //线性插值
        public static double[] Interpolate(double[] input, int targetLength)
        {
            double[] result = new double[targetLength];
            double scale = (double)(input.Length - 1) / (targetLength - 1);

            for (int i = 0; i < targetLength; i++)
            {
                double index = i * scale;
                int leftIndex = (int)Math.Floor(index);
                int rightIndex = (int)Math.Ceiling(index);

                if (leftIndex == rightIndex)
                {
                    result[i] = input[leftIndex];
                }
                else
                {
                    double leftValue = input[leftIndex];
                    double rightValue = input[rightIndex];
                    double weight = index - leftIndex;
                    result[i] = leftValue * (1 - weight) + rightValue * weight;
                }
            }

            return result;
        }

    }
    public class SpeechRecognition
    {
        public static double[][] Spectrogram(double[] signal, int sampleRate, int frameStrideMs = 48, int frameSizeMs = 64)
        {
            double[] preprocessedSignal = Utils.Preprocess(signal);
            double[] emphasizedSignal = Utils.PreEmphasis(preprocessedSignal);
            double[][] frames = Utils.FrameSignal(emphasizedSignal, sampleRate, frameStrideMs, frameSizeMs);
            double[][] windowedFrames = frames.AsParallel().Select(frame => Utils.Windowing(frame)).ToArray();
            double[][] paddedFrames = windowedFrames.AsParallel().Select(frame => Utils.ZeroPadToNextPowerOfTwo(frame)).ToArray();
            Complex[][] fftResults = paddedFrames.AsParallel().Select(frame => Utils.FFT(frame)).ToArray();
            double[][] spectrumEnergies = fftResults.AsParallel().Select(fftResult => Utils.ComputeSpectrumEnergy(fftResult)).ToArray();
            return spectrumEnergies;
        }
        public static double[][] CFCC(double[] signal, int sampleRate, int frameStrideMs = 48, int frameSizeMs = 64, int numFilters = 26)
        {
            double[] preprocessedSignal = Utils.Preprocess(signal);
            double[] emphasizedSignal = Utils.PreEmphasis(preprocessedSignal);
            double[][] frames = Utils.FrameSignal(emphasizedSignal, sampleRate, frameStrideMs, frameSizeMs);
            double[][] windowedFrames = frames.AsParallel().Select(frame => Utils.Windowing(frame)).ToArray();
            double[][] paddedFrames = windowedFrames.AsParallel().Select(frame => Utils.ZeroPadToNextPowerOfTwo(frame)).ToArray();
            Complex[][] fftResults = paddedFrames.AsParallel().Select(frame => Utils.FFT(frame)).ToArray();
            double[][] spectrumEnergies = fftResults.AsParallel().Select(fftResult => Utils.ComputeSpectrumEnergy(fftResult)).ToArray();
            double[][] filterBankEnergies = spectrumEnergies.AsParallel().Select(spectrumEnergy => Utils.CochlearFilterBank(spectrumEnergy, sampleRate, numFilters)).ToArray();
            double[][] cepstrums = filterBankEnergies.AsParallel().Select(filterBankEnergy => Utils.ComputeCepstrum(filterBankEnergy)).ToArray();
            return cepstrums;
        }

        public static double[][] GFCC(double[] signal, int sampleRate, int frameStrideMs = 48, int frameSizeMs = 64, int numFilters = 26)
        {
            double[] preprocessedSignal = Utils.Preprocess(signal);
            double[] emphasizedSignal = Utils.PreEmphasis(preprocessedSignal);
            double[][] frames = Utils.FrameSignal(emphasizedSignal, sampleRate, frameStrideMs, frameSizeMs);
            double[][] windowedFrames = frames.AsParallel().Select(frame => Utils.Windowing(frame)).ToArray();
            double[][] paddedFrames = windowedFrames.AsParallel().Select(frame => Utils.ZeroPadToNextPowerOfTwo(frame)).ToArray();
            Complex[][] fftResults = paddedFrames.AsParallel().Select(frame => Utils.FFT(frame)).ToArray();
            double[][] spectrumEnergies = fftResults.AsParallel().Select(fftResult => Utils.ComputeSpectrumEnergy(fftResult)).ToArray();
            double[][] filterBankEnergies = spectrumEnergies.AsParallel().Select(spectrumEnergy => Utils.GammatoneFilterBank(spectrumEnergy, sampleRate, numFilters)).ToArray();
            double[][] cepstrums = filterBankEnergies.AsParallel().Select(filterBankEnergy => Utils.ComputeCepstrum(filterBankEnergy)).ToArray();
            return cepstrums;
        }

        public static double[][] Fbank(double[] signal, int sampleRate, int frameStrideMs = 48, int frameSizeMs = 64, int numFilters = 26)
        {
            double[] preprocessedSignal = Utils.Preprocess(signal);
            double[] emphasizedSignal = Utils.PreEmphasis(preprocessedSignal);
            double[][] frames = Utils.FrameSignal(emphasizedSignal, sampleRate, frameStrideMs, frameSizeMs);
            double[][] windowedFrames = frames.AsParallel().Select(frame => Utils.Windowing(frame)).ToArray();
            double[][] paddedFrames = windowedFrames.AsParallel().Select(frame => Utils.ZeroPadToNextPowerOfTwo(frame)).ToArray();
            Complex[][] fftResults = paddedFrames.AsParallel().Select(frame => Utils.FFT(frame)).ToArray();
            double[][] spectrumEnergies = fftResults.AsParallel().Select(fftResult => Utils.ComputeSpectrumEnergy(fftResult)).ToArray();
            double[][] filterBankEnergies = spectrumEnergies.AsParallel().Select(spectrumEnergy => Utils.MelFilterBank(spectrumEnergy, sampleRate, numFilters, false, false)).ToArray();
            return filterBankEnergies;
        }

        public static double[][] MFCC(double[] signal, int sampleRate, int frameStrideMs = 48, int frameSizeMs = 64, int numFilters = 26)
        {
            double[][] fbank = Fbank(signal, sampleRate);
            double[][] mfcc = fbank.AsParallel().Select(filterBankEnergy => Utils.ComputeCepstrum(filterBankEnergy)).ToArray();
            return mfcc;
        }

        public static double[][] SDC(double[] signal, int sampleRate, int frameStrideMs = 48, int frameSizeMs = 64, int numFilters = 40, int numDelta = 2)
        {
            double[][] fbank = Fbank(signal, sampleRate);
            double[][] delta = fbank.AsParallel().Select(mfccFeature => Utils.ComputeCepstrum(mfccFeature, true)).ToArray(); ;
            return delta;
        }

        public static double[][] Processing(double[] signal, int sampleRate, int frameStrideMs = 48, int frameSizeMs = 64)
        {
            double[] preprocessedSignal = Utils.Preprocess(signal);
            double[] emphasizedSignal = Utils.PreEmphasis(preprocessedSignal);
            double[][] frames = Utils.FrameSignal(emphasizedSignal, sampleRate, frameStrideMs, frameSizeMs);
            double[][] windowedPaddedFrames = frames.AsParallel().Select(frame => Utils.ZeroPadToNextPowerOfTwo(Utils.Windowing(frame))).ToArray();
            double[][] spectrumEnergies = windowedPaddedFrames.AsParallel().Select(frame => Utils.ComputeSpectrumEnergy(Utils.FFT(frame))).ToArray();
            return spectrumEnergies;
        }

        public static double[][] FastProcessing(double[] signal, int sampleRate, int frameStrideMs = 48, int frameSizeMs = 64)
        {
            // 预处理信号
            double[] preprocessedSignal = Utils.Preprocess(signal);

            // 预加重
            double[] emphasizedSignal = Utils.PreEmphasis(preprocessedSignal);

            // 分帧
            int frameSize = (int)(sampleRate * frameSizeMs / 1000.0);
            int frameStride = (int)(sampleRate * frameStrideMs / 1000.0);

            int numFrames = (int)Math.Ceiling((double)(emphasizedSignal.Length - frameSize) / frameStride) + 1;
            double[][] frames = new double[numFrames][];

            Parallel.For(0, numFrames, i =>
            {
                int startIdx = i * frameStride;
                frames[i] = new double[frameSize];
                Array.Copy(emphasizedSignal, startIdx, frames[i], 0, Math.Min(frameSize, emphasizedSignal.Length - startIdx));
            });

            // 加窗和零填充
            double[][] windowedPaddedFrames = new double[numFrames][];
            Parallel.For(0, numFrames, i =>
            {
                double[] windowedFrame = Utils.Windowing(frames[i]);
                windowedPaddedFrames[i] = Utils.ZeroPadToNextPowerOfTwo(windowedFrame);
            });

            // 进行FFT计算和计算谱线能量
            double[][] spectrumEnergies = new double[numFrames][];
            Parallel.For(0, numFrames, i =>
            {
                Complex[] fftResult = Utils.FFT(windowedPaddedFrames[i]);
                spectrumEnergies[i] = Utils.ComputeSpectrumEnergy(fftResult);
            });

            return spectrumEnergies;
        }
        public static double[][] WeightedFeatureFusion(double[][] cfccFeatures, double[][] gfccFeatures, double[][] mfccFeatures, double cfccWeight, double gfccWeight, double mfccWeight)
        {
            int maxLength = Math.Max(cfccFeatures.Length, Math.Max(gfccFeatures.Length, mfccFeatures.Length));
            int numFeatures = cfccFeatures[0].Length;

            double[][] interpolatedCfcc = cfccFeatures.Select(frame => Utils.Interpolate(frame, numFeatures)).ToArray();
            double[][] interpolatedGfcc = gfccFeatures.Select(frame => Utils.Interpolate(frame, numFeatures)).ToArray();
            double[][] interpolatedMfcc = mfccFeatures.Select(frame => Utils.Interpolate(frame, numFeatures)).ToArray();

            double[][] fusedFeatures = new double[maxLength][];

            for (int i = 0; i < maxLength; i++)
            {
                fusedFeatures[i] = new double[numFeatures];
                for (int j = 0; j < numFeatures; j++)
                {
                    double cfccValue = i < interpolatedCfcc.Length ? interpolatedCfcc[i][j] : 0;
                    double gfccValue = i < interpolatedGfcc.Length ? interpolatedGfcc[i][j] : 0;
                    double mfccValue = i < interpolatedMfcc.Length ? interpolatedMfcc[i][j] : 0;

                    fusedFeatures[i][j] = cfccWeight * cfccValue + gfccWeight * gfccValue + mfccWeight * mfccValue;
                }
            }

            return fusedFeatures;
        }

        public static double[][] FeatureSplicingFusion(double[][] cfccFeatures, double[][] gfccFeatures, double[][] mfccFeatures)
        {
            int maxLength = Math.Max(cfccFeatures.Length, Math.Max(gfccFeatures.Length, mfccFeatures.Length));
            int numFeatures = cfccFeatures[0].Length + gfccFeatures[0].Length + mfccFeatures[0].Length;

            double[][] interpolatedCfcc = cfccFeatures.Select(frame => Utils.Interpolate(frame, cfccFeatures[0].Length)).ToArray();
            double[][] interpolatedGfcc = gfccFeatures.Select(frame => Utils.Interpolate(frame, gfccFeatures[0].Length)).ToArray();
            double[][] interpolatedMfcc = mfccFeatures.Select(frame => Utils.Interpolate(frame, mfccFeatures[0].Length)).ToArray();

            double[][] splicedFeatures = new double[maxLength][];

            for (int i = 0; i < maxLength; i++)
            {
                splicedFeatures[i] = new double[numFeatures];
                int index = 0;

                if (i < interpolatedCfcc.Length)
                {
                    Array.Copy(interpolatedCfcc[i], 0, splicedFeatures[i], index, interpolatedCfcc[i].Length);
                    index += interpolatedCfcc[i].Length;
                }

                if (i < interpolatedGfcc.Length)
                {
                    Array.Copy(interpolatedGfcc[i], 0, splicedFeatures[i], index, interpolatedGfcc[i].Length);
                    index += interpolatedGfcc[i].Length;
                }

                if (i < interpolatedMfcc.Length)
                {
                    Array.Copy(interpolatedMfcc[i], 0, splicedFeatures[i], index, interpolatedMfcc[i].Length);
                }
            }

            return splicedFeatures;
        }

        /// <summary>
        /// 从频谱能量生成CFCC特征
        /// </summary>
        public static double[][] ProcessCFCC(double[][] spectrumEnergies, int sampleRate, int numFilters = 26)
        {
            double[][] filterBankEnergies = spectrumEnergies.AsParallel().Select(spectrumEnergy => Utils.CochlearFilterBank(spectrumEnergy, sampleRate, numFilters)).ToArray();
            double[][] cepstrums = filterBankEnergies.AsParallel().Select(filterBankEnergy => Utils.ComputeCepstrum(filterBankEnergy)).ToArray();
            return cepstrums;
        }

        /// <summary>
        /// 从频谱能量生成GFCC特征
        /// </summary>
        public static double[][] ProcessGFCC(double[][] spectrumEnergies, int sampleRate, int numFilters = 26)
        {
            double[][] filterBankEnergies = spectrumEnergies.AsParallel().Select(spectrumEnergy => Utils.GammatoneFilterBank(spectrumEnergy, sampleRate, numFilters)).ToArray();
            double[][] cepstrums = filterBankEnergies.AsParallel().Select(filterBankEnergy => Utils.ComputeCepstrum(filterBankEnergy)).ToArray();
            return cepstrums;
        }

        /// <summary>
        /// 从频谱能量生成MFCC特征
        /// </summary>
        public static double[][] ProcessMFCC(double[][] spectrumEnergies, int sampleRate, int numFilters = 26)
        {
            double[][] filterBankEnergies = spectrumEnergies.AsParallel().Select(spectrumEnergy => Utils.MelFilterBank(spectrumEnergy, sampleRate, numFilters, false, false)).ToArray();
            double[][] cepstrums = filterBankEnergies.AsParallel().Select(filterBankEnergy => Utils.ComputeCepstrum(filterBankEnergy)).ToArray();
            return cepstrums;
        }
        /// <summary>
        /// 从频谱能量生成SDC特征
        /// </summary>
        public static double[][] ProcessSDC(double[][] spectrumEnergies, int sampleRate, int numFilters = 40, int numDelta = 2)
        {
            double[][] filterBankEnergies = spectrumEnergies.AsParallel().Select(spectrumEnergy => Utils.MelFilterBank(spectrumEnergy, sampleRate, numFilters, false, false)).ToArray();
            double[][] deltaCepstrums = filterBankEnergies.AsParallel().Select(filterBankEnergy => Utils.ComputeCepstrum(filterBankEnergy, true)).ToArray();
            return deltaCepstrums;
        }

    }
}
/*
Speech/
├── CharacteristicData/
│   └── CFCC/                  # 数据集文件夹
│       ├── audio1.txt         # 特征文件
│       └── audio2.txt
├── AudioLib/
│   └── THCHS-30Low/           # 标签目录
│       ├── audio1.trn
│       └── audio2.trn
└── Model/                     # 模型输出目录
    ├── CFCC-A.pth
    └── GFCC-B.pth
*/
